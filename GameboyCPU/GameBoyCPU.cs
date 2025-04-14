using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameboyCPU
{

    public class GameBoyCPU
    {
        public unsafe struct Registers
        {
            public ushort registerAF;
            public ushort registerBC;
            public ushort registerDE;
            public ushort registerHL;

            public ushort registerSP;
            public ushort registerPC;
        }

        public bool carryFlag = false;

        public bool HalfCarryFlag = false;

        public bool subtractFlagN = false;

        public bool IMEFlag = false;

        public bool zeroFLag = false;

        private MemoryMap memoryMap;

        public GameBoyCPU(MemoryMap memoryMap)
        {
            this.memoryMap = memoryMap;
            registers = new Registers();
            registers.registerPC = 0x01000; // Apparent start of first instruction for all games
            instructionSet = new Dictionary<byte, Action>();
            InitializeInstructionSet();
        }

        public unsafe struct Instruction
        {
            public byte operationCode;
            public ushort* instruction;
        }

        public unsafe Registers registers;

        public unsafe Dictionary<byte, Action> instructionSet;

        public unsafe void InitializeInstructionSet()
        {
            instructionSet[0x00] = NOP;
            instructionSet[0x01] = () => LD(ref registers.registerBC, FetchParameters16Bit());
            instructionSet[0x02] = () => LD(ref registers.registerBC, (byte)registers.registerAF, registers.registerBC);
            instructionSet[0x03] = () => INC(ref registers.registerBC);
            instructionSet[0x04] = () => INC(ref registers.registerBC, true);
            instructionSet[0x05] = () => DEC(ref registers.registerBC, true);
            instructionSet[0X06] = () => LD(ref registers.registerBC, FetchParameters8Bit(), true);
            instructionSet[0x07] = () => RLCA();
            instructionSet[0x08] = () => LD(ref registers.registerAF, registers.registerSP, registers.registerAF);
            instructionSet[0x09] = () => ADD(ref registers.registerHL, registers.registerBC);
            instructionSet[0x0A] = () => LD(ref registers.registerAF, registers.registerBC, registers.registerBC);
            instructionSet[0x0B] = () => DEC(ref registers.registerBC);
            instructionSet[0x0C] = () => INC(ref registers.registerBC, false);
            instructionSet[0x0D] = () => DEC(ref registers.registerBC, false);
            instructionSet[0x0E] = () => LD(ref registers.registerBC, FetchParameters8Bit(), false);
            instructionSet[0x0F] = () => RRCA();
            instructionSet[0x10] = () => STOP();
            instructionSet[0x11] = () => LD(ref registers.registerDE, FetchParameters16Bit());
            instructionSet[0x12] = () => LD(ref registers.registerDE, (byte)(registers.registerAF >> 8), registers.registerDE);
            instructionSet[0x13] = () => INC(ref registers.registerDE);
            instructionSet[0x14] = () => INC(ref registers.registerDE, true);
            instructionSet[0x15] = () => DEC(ref registers.registerDE, true);
            instructionSet[0x16] = () => LD(ref registers.registerDE, FetchParameters8Bit(), true);
            instructionSet[0x17] = () => RLA();
            instructionSet[0x18] = () => JR(registers.registerDE, true);
            instructionSet[0x19] = () => ADD(ref registers.registerHL, registers.registerDE);
            instructionSet[0x1A] = () => LD(ref registers.registerAF, registers.registerDE, registers.registerDE, true);
            instructionSet[0x1B] = () => DEC(ref registers.registerDE);
            instructionSet[0x1C] = () => INC(ref registers.registerDE, false);
            instructionSet[0x1D] = () => DEC(ref registers.registerDE, false);
            instructionSet[0x1E] = () => LD(ref registers.registerDE, FetchParameters8Bit(), false);
            instructionSet[0x1F] = () => RRA();
            instructionSet[0x20] = () => JRWithNC();
            instructionSet[0x21] = () => LD(ref registers.registerHL, FetchParameters16Bit());
            instructionSet[0x22] = () => LDWithHCIncrement(ref registers.registerHL, (byte)(registers.registerAF), registers.registerHL, false);
            instructionSet[0x23] = () => INC(ref registers.registerHL);
            instructionSet[0x24] = () => INC(ref registers.registerHL, true);
            instructionSet[0x25] = () => DEC(ref registers.registerHL, true);
            instructionSet[0x26] = () => LD(ref registers.registerHL, FetchParameters8Bit(), true);
            instructionSet[0x27] = () => DAA();
            instructionSet[0x28] = () => JR(registers.registerBC, false);
            instructionSet[0x29] = () => ADD(ref registers.registerHL, registers.registerHL);
            instructionSet[0x2A] = () => LDWithHCIncrement(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0x2B] = () => DEC(ref registers.registerHL);
            instructionSet[0x2C] = () => INC(ref registers.registerHL, false);
            instructionSet[0x2D] = () => DEC(ref registers.registerHL, false);
            instructionSet[0x2E] = () => LD(ref registers.registerHL, FetchParameters8Bit(), false);
            instructionSet[0x2F] = () => CPL();
            instructionSet[0x30] = () => JRWithNC();
            instructionSet[0x31] = () => LD(ref registers.registerSP, FetchParameters16Bit());
            instructionSet[0x32] = () => LDWithHCDecrement(ref registers.registerHL, (byte)(registers.registerAF), registers.registerHL, true);
            instructionSet[0x33] = () => INC(ref registers.registerSP);
            instructionSet[0x34] = () => INC(ref registers.registerHL, true);
            instructionSet[0x35] = () => DECReference(registers.registerHL);
            instructionSet[0x36] = () => LD(ref registers.registerHL, FetchParameters8Bit(), true);
            instructionSet[0x37] = () => SCF();
            instructionSet[0x38] = () => JRWithNC();
            instructionSet[0x39] = () => ADD(ref registers.registerHL, registers.registerSP);
            instructionSet[0x3A] = () => LDWithHCDecrement(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0x3B] = () => DEC(ref registers.registerSP);
            instructionSet[0x3C] = () => DEC(ref registers.registerAF, true);
            instructionSet[0x3D] = () => DEC(ref registers.registerHL, false);
            instructionSet[0x3E] = () => LD(ref registers.registerHL, FetchParameters8Bit(), false);
            instructionSet[0x3F] = () => CPL();
            instructionSet[0x40] = () => LD(ref registers.registerBC, (byte)registers.registerBC, true);
            instructionSet[0x41] = () => LD(ref registers.registerBC, (byte)(registers.registerBC << 8), true);
            instructionSet[0x42] = () => LD(ref registers.registerBC, (byte)registers.registerDE, true);
            instructionSet[0x43] = () => LD(ref registers.registerBC, (byte)(registers.registerDE << 8), true);
            instructionSet[0x44] = () => LD(ref registers.registerBC, (byte)registers.registerHL, true);
            instructionSet[0x45] = () => LD(ref registers.registerBC, (byte)(registers.registerHL << 8), true);
            instructionSet[0x46] = () => LD(ref registers.registerBC, registers.registerHL, registers.registerHL, true);
            instructionSet[0x47] = () => LD(ref registers.registerBC, (byte)registers.registerAF, true);
            instructionSet[0x48] = () => LD(ref registers.registerBC, (byte)registers.registerBC, false);
            instructionSet[0x49] = () => LD(ref registers.registerBC, (byte)(registers.registerBC << 8), false);
            instructionSet[0x4A] = () => LD(ref registers.registerBC, (byte)registers.registerDE, false);
            instructionSet[0x4B] = () => LD(ref registers.registerBC, (byte)(registers.registerBC << 8), false);
            instructionSet[0x4C] = () => LD(ref registers.registerBC, (byte)registers.registerHL, false);
            instructionSet[0x4D] = () => LD(ref registers.registerBC, (byte)(registers.registerHL << 8), false);
            instructionSet[0x4E] = () => LD(ref registers.registerBC, registers.registerHL, registers.registerHL, false);
            instructionSet[0x4F] = () => LD(ref registers.registerBC, (byte)registers.registerAF, false);
            instructionSet[0x50] = () => LD(ref registers.registerDE, (byte)registers.registerBC, true);
            instructionSet[0x51] = () => LD(ref registers.registerDE, (byte)(registers.registerBC << 8), true);
            instructionSet[0x52] = () => LD(ref registers.registerDE, (byte)registers.registerDE, true);
            instructionSet[0x53] = () => LD(ref registers.registerDE, (byte)(registers.registerDE << 8), true);
            instructionSet[0x54] = () => LD(ref registers.registerDE, (byte)registers.registerHL, true);
            instructionSet[0x55] = () => LD(ref registers.registerDE, (byte)(registers.registerHL << 8), true);
            instructionSet[0x56] = () => LD(ref registers.registerDE, registers.registerHL, registers.registerHL, true);
            instructionSet[0x57] = () => LD(ref registers.registerDE, (byte)registers.registerAF, true);
            instructionSet[0x58] = () => LD(ref registers.registerDE, (byte)registers.registerBC, false);
            instructionSet[0x59] = () => LD(ref registers.registerDE, (byte)(registers.registerBC >> 8), false);
            instructionSet[0x5A] = () => LD(ref registers.registerDE, (byte)registers.registerDE, false);
            instructionSet[0x5B] = () => LD(ref registers.registerDE, (byte)(registers.registerDE << 8), false);
            instructionSet[0x5C] = () => LD(ref registers.registerDE, (byte)(registers.registerHL), false);
            instructionSet[0x5D] = () => LD(ref registers.registerDE, (byte)(registers.registerHL << 8), false);
            instructionSet[0x5E] = () => LD(ref registers.registerDE, registers.registerHL, registers.registerHL, false);
            instructionSet[0x5F] = () => LD(ref registers.registerDE, (byte)registers.registerAF, false);
            instructionSet[0x60] = () => LD(ref registers.registerHL, (byte)registers.registerBC, true);
            instructionSet[0x61] = () => LD(ref registers.registerHL, (byte)(registers.registerBC << 8), true);
            instructionSet[0x62] = () => LD(ref registers.registerHL, (byte)registers.registerDE, true);
            instructionSet[0x63] = () => LD(ref registers.registerHL, (byte)(registers.registerDE << 8), true);
            instructionSet[0x64] = () => LD(ref registers.registerHL, (byte)(registers.registerHL), true);
            instructionSet[0x65] = () => LD(ref registers.registerHL, (byte)(registers.registerHL << 8), true);
            instructionSet[0x66] = () => LD(ref registers.registerHL, registers.registerHL, registers.registerHL, true);
            instructionSet[0x67] = () => LD(ref registers.registerHL, (byte)registers.registerAF, true);
            instructionSet[0x68] = () => LD(ref registers.registerHL, (byte)registers.registerBC, false);
            instructionSet[0x69] = () => LD(ref registers.registerHL, (byte)(registers.registerBC << 8), false);
            instructionSet[0x6A] = () => LD(ref registers.registerHL, (byte)registers.registerDE, false);
            instructionSet[0x6B] = () => LD(ref registers.registerHL, (byte)(registers.registerDE << 8), false);
            instructionSet[0x6C] = () => LD(ref registers.registerHL, (byte)(registers.registerHL), false);
            instructionSet[0x6D] = () => LD(ref registers.registerHL, (byte)(registers.registerHL << 8), false);
            instructionSet[0x6E] = () => LD(ref registers.registerHL, registers.registerHL, registers.registerHL, false);
            instructionSet[0x6F] = () => LD(ref registers.registerHL, (byte)registers.registerAF, false);
            instructionSet[0x70] = () => LD(ref registers.registerHL, (byte)registers.registerBC, registers.registerHL, true);
            instructionSet[0x71] = () => LD(ref registers.registerHL, (byte)(registers.registerBC << 8), registers.registerHL, true);
            instructionSet[0x72] = () => LD(ref registers.registerHL, (byte)registers.registerDE, registers.registerHL, true);
            instructionSet[0x73] = () => LD(ref registers.registerHL, (byte)(registers.registerDE << 8), registers.registerHL, true);
            instructionSet[0x74] = () => LD(ref registers.registerHL, (byte)(registers.registerHL), registers.registerHL, true);
            instructionSet[0x75] = () => LD(ref registers.registerHL, (byte)(registers.registerHL << 8), registers.registerHL, true);
            instructionSet[0x76] = () => HALT();
            instructionSet[0x77] = () => LD(ref registers.registerHL, (byte)registers.registerAF, registers.registerHL, true);
            instructionSet[0x78] = () => LD(ref registers.registerAF, (byte)registers.registerBC, registers.registerAF, true);
            instructionSet[0x79] = () => LD(ref registers.registerAF, (byte)(registers.registerBC << 8), registers.registerAF, true);
            instructionSet[0x7A] = () => LD(ref registers.registerAF, (byte)registers.registerDE, registers.registerAF, true);
            instructionSet[0x7B] = () => LD(ref registers.registerAF, (byte)(registers.registerDE << 8), registers.registerAF, true);
            instructionSet[0x7C] = () => LD(ref registers.registerAF, (byte)(registers.registerHL), registers.registerAF, true);
            instructionSet[0x7D] = () => LD(ref registers.registerAF, (byte)(registers.registerHL << 8), registers.registerAF, true);
            instructionSet[0x7E] = () => LD(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0x7F] = () => LD(ref registers.registerAF, (byte)registers.registerAF, registers.registerAF, true);
            instructionSet[0x80] = () => ADD(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0x81] = () => ADD(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0x82] = () => ADD(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0x83] = () => ADD(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0x84] = () => ADD(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0x85] = () => ADD(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0x86] = () => ADD(ref registers.registerAF, registers.registerHL, registers.registerHL);
            instructionSet[0x87] = () => ADD(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0x88] = () => ADC(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0x89] = () => ADC(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0x8A] = () => ADC(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0x8B] = () => ADC(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0x8C] = () => ADC(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0x8D] = () => ADC(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0x8E] = () => ADC(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0x8F] = () => ADC(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0x90] = () => SUB(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0x91] = () => SUB(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0x92] = () => SUB(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0x93] = () => SUB(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0x94] = () => SUB(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0x95] = () => SUB(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0x96] = () => SUB(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0x97] = () => SUB(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0x98] = () => SBC(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0x99] = () => SBC(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0x9A] = () => SBC(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0x9B] = () => SBC(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0x9C] = () => SBC(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0x9D] = () => SBC(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0x9E] = () => SBC(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0x9F] = () => SBC(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0xA0] = () => AND(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0xA1] = () => AND(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0xA2] = () => AND(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0xA3] = () => AND(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0xA4] = () => AND(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0xA5] = () => AND(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0xA6] = () => AND(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0xA7] = () => AND(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0xA8] = () => XOR(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0xA9] = () => XOR(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0xAA] = () => XOR(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0xAB] = () => XOR(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0xAC] = () => XOR(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0xAD] = () => XOR(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0xAE] = () => XOR(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0xAF] = () => XOR(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0xB0] = () => OR(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0xB1] = () => OR(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0xB2] = () => OR(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0xB3] = () => OR(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0xB4] = () => OR(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0xB5] = () => OR(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0xB6] = () => OR(ref registers.registerAF, registers.registerHL, registers.registerHL, true);
            instructionSet[0xB7] = () => OR(ref registers.registerAF, (byte)registers.registerAF, true);
            instructionSet[0xB8] = () => CP(ref registers.registerAF, (byte)registers.registerBC, true);
            instructionSet[0xB9] = () => CP(ref registers.registerAF, (byte)(registers.registerBC << 8), true);
            instructionSet[0xBA] = () => CP(ref registers.registerAF, (byte)registers.registerDE, true);
            instructionSet[0xBB] = () => CP(ref registers.registerAF, (byte)(registers.registerDE << 8), true);
            instructionSet[0xBC] = () => CP(ref registers.registerAF, (byte)registers.registerHL, true);
            instructionSet[0xBD] = () => CP(ref registers.registerAF, (byte)(registers.registerHL << 8), true);
            instructionSet[0xBE] = () => CP(ref registers.registerAF, registers.registerHL, registers.registerHL, true); // compare with memory location
            instructionSet[0xBF] = () => CP(ref registers.registerAF, (byte)registers.registerAF, true); // compare with itself, should always be 0 and set flags accordingly
            instructionSet[0xC0] = () => RET(!zeroFLag);
            instructionSet[0xC1] = () => POP(ref registers.registerBC); // Pop BC from stack
            instructionSet[0xC2] = () => JP(!carryFlag, FetchParameters16Bit());
            instructionSet[0xC3] = () => JP(FetchParameters16Bit()); // Jump to address in PC
            instructionSet[0xC4] = () => CALL(!zeroFLag, FetchParameters16Bit()); // Call if not zero flag is set
            instructionSet[0xC5] = () => PUSH(ref registers.registerBC); // Push BC onto stack
            instructionSet[0xC6] = () => ADD(ref registers.registerAF, FetchParameters8Bit(), true); // Add immediate value to A register
            instructionSet[0xC7] = () => RST(0x00); // Restart at 0x00
            instructionSet[0xC8] = () => RET(zeroFLag); // Return if zero flag is set
            instructionSet[0xC9] = () => RET(); // Unconditional return from subroutine
            instructionSet[0xCA] = () => JP(zeroFLag, FetchParameters16Bit()); // Jump to address if zero flag is set
            //instructionSet[0xCB] = () => PREFIX();
            instructionSet[0xCC] = () => CALL(zeroFLag, FetchParameters16Bit()); // Call if zero flag is set
            instructionSet[0xCD] = () => CALL(FetchParameters16Bit()); // Call subroutine at address in PC
            instructionSet[0xCE] = () => ADC(ref registers.registerAF, FetchParameters8Bit(), true); // Add with carry immediate value to A register
            instructionSet[0xCF] = () => RST(0x08); // Restart at 0x08, typically used for interrupts or specific routines
            instructionSet[0xD0] = () => RET(!zeroFLag); // Return if no carry flag is set
            instructionSet[0xD1] = () => POP(ref registers.registerDE); // Pop DE from stack
            instructionSet[0xD2] = () => JP(!carryFlag, FetchParameters16Bit()); // Jump to address if no carry flag is set
            instructionSet[0xD4] = () => CALL(!carryFlag, FetchParameters16Bit()); // Call if no carry flag is set
            instructionSet[0xD5] = () => PUSH(ref registers.registerDE); // Push DE onto stack
            instructionSet[0xD6] = () => SUB(ref registers.registerAF, FetchParameters8Bit(), true); // Subtract immediate value from A register
            instructionSet[0xD7] = () => RST(0x10); // Restart at 0x10, typically used for interrupts or specific routines
            instructionSet[0xD8] = () => RET(carryFlag); // Return if carry flag is set
            instructionSet[0xD9] = () => RET(); // Unconditional return from subroutine (this is a duplicate, should be removed)
            instructionSet[0xDA] = () => JP(carryFlag, FetchParameters16Bit()); // Jump to address if carry flag is set
            instructionSet[0xDC] = () => CALL(carryFlag, FetchParameters16Bit()); // Call if carry flag is set
            instructionSet[0xDE] = () => ADC(ref registers.registerAF, FetchParameters8Bit(), true); // Add with carry immediate value to A register
            instructionSet[0xDF] = () => RST(0x18); // Restart at 0x18, typically used for interrupts or specific routines
            instructionSet[0xE0] = () => LD(ref registers.registerAF, FetchParameters8Bit(), true); // Load immediate value into A register
            instructionSet[0xE1] = () => POP(ref registers.registerHL); // Pop HL from stack
            instructionSet[0xE2] = () => LD(ref registers.registerAF, (byte)(registers.registerBC >> 8), registers.registerAF); // Load BC into A register
            instructionSet[0xE5] = () => PUSH(ref registers.registerHL); // Push HL onto stack
            instructionSet[0xE6] = () => AND(ref registers.registerAF, FetchParameters8Bit(), true); // AND immediate value with A register
            instructionSet[0xE7] = () => RST(0x20); // Restart at 0x20, typically used for interrupts or specific routines
            instructionSet[0xE8] = () => ADD(ref registers.registerSP, FetchParameters8Bit()); // Add signed immediate value to stack pointer
            instructionSet[0xE9] = () => JP(registers.registerHL); // Jump to address in HL register
            instructionSet[0xEA] = () => LD(ref registers.registerAF, FetchParameters16Bit(), registers.registerAF); // Load immediate value into A register from address
            instructionSet[0xEE] = () => XOR(ref registers.registerAF, FetchParameters8Bit(), true); // XOR immediate value with A register
            instructionSet[0xEF] = () => RST(0x28); // Restart at 0x28, typically used for interrupts or specific routines
            instructionSet[0xF0] = () => LD(ref registers.registerAF, FetchParameters8Bit(), true); // Load immediate value into A register
            instructionSet[0xF1] = () => POP(ref registers.registerAF); // Pop AF from stack, typically used for restoring flags
            instructionSet[0xF2] = () => LD(ref registers.registerAF, (byte)(registers.registerBC >> 8), registers.registerAF); // Load BC into A register
            instructionSet[0xF3] = () => DI(); // Disable interrupts (set IMEFlag to false)
            instructionSet[0xF5] = () => PUSH(ref registers.registerAF); // Push AF onto stack, typically used for saving flags
            instructionSet[0xF6] = () => OR(ref registers.registerAF, FetchParameters8Bit(), true); // OR immediate value with A register
            instructionSet[0xF7] = () => RST(0x30); // Restart at 0x30, typically used for interrupts or specific routines
            instructionSet[0xF8] = () => LD(ref registers.registerHL, (ushort)(registers.registerSP + (sbyte)FetchParameters8Bit())); // Load HL from stack pointer with offset
            instructionSet[0xF9] = () => LD(ref registers.registerSP, registers.registerHL); // Load stack pointer from HL register
            instructionSet[0xFA] = () => LD(ref registers.registerAF, registers.registerAF, registers.registerAF); // Load immediate value into A register from address
            instructionSet[0xFB] = () => EI(); // Enable interrupts (set IMEFlag to true)
            instructionSet[0xFE] = () => CP(ref registers.registerAF, FetchParameters8Bit(), true); // Compare immediate value with A register
            instructionSet[0xFF] = () => RST(0x38); // Restart at 0x38, typically used for interrupts or specific routines

        }

        private void InitializePrefixSet()
        {

        }

        public void CPUMainLoop()
        {
            while (true)
            {
                byte opCode = ReadInstruction(registers.registerPC);
                instructionSet[opCode](); // Execute the instruction based on the opcode
                Console.WriteLine($"Executed instruction: {opCode:X2} at PC: {registers.registerPC:X4}");
            }
        }

        public byte ReadInstruction(ushort programCounter)
        {
            return this.memoryMap.GetMemoryValue(programCounter);
        }

        private ushort FetchParameters16Bit()
        {
            byte firstByte = this.memoryMap.GetMemoryValue((ushort)(registers.registerPC + 1));
            byte secondByte = this.memoryMap.GetMemoryValue((ushort)(registers.registerPC + 2));

            ushort value = (ushort)((secondByte << 8) | firstByte);
            registers.registerPC += 2;
            return value;
        }

        private byte FetchParameters8Bit()
        {

            var value = this.memoryMap.GetMemoryValue((ushort)(registers.registerPC + 1));
            registers.registerPC++;
            return value;
        }

        #region INSTRUCTION_FUNCTIONS

        private void DI()
        {
            IMEFlag = false;
            registers.registerPC++;
        }

        private void EI()
        {
            IMEFlag = true;
            registers.registerPC++;
        }

        private void SCF()
        {
            carryFlag = true;
            registers.registerPC++;
        }

        private void RST(byte address)
        {
            registers.registerPC++;
        }

        /// <summary>
        /// This pushes the address of the instruction after the CALL on the stack, such that RET can pop it later; then, it executes an implicit JP n16.
        /// </summary>
        /// <param name="address"></param>
        private void CALL(ushort address)
        {
            memoryMap.PushToStack((ushort)(registers.registerPC + 3));
            registers.registerPC = address;
        }

        private void CALL(bool flag, ushort address)
        {
            if (flag)
            {
                CALL(address);
            }
            else
            {
                registers.registerPC++;
            }
        }

        private void NOP()
        {
            registers.registerPC++;
        }

        private void RET(bool flag)
        {
            if (flag)
            {
                RET();
            }
            else
            {
                registers.registerPC++;
            }
        }

        private void RET()
        {
            registers.registerPC = memoryMap.PopFromStack16Bit();
        }

        // Pop the value from the stack into the register
        // This would typically involve reading from memory where the stack pointer is pointing to
        // and then incrementing the stack pointer.
        private void POP(ref ushort register)
        {
            register = memoryMap.PopFromStack();
            registers.registerPC++;
        }

        private void SetPOPFlags(byte result)
        {
            // set zero flag if bit 7 is set
            zeroFLag = result == 0;
            // set subtract flag if bit 6 is set
            subtractFlagN = (result & 0x20) != 0;
            // set half carry flag if bit 5 is set
            HalfCarryFlag = (result & 0x10) != 0;
            // set carry flag if bit 4 is set
            carryFlag = (result & 0x08) != 0;
        }

        private void PUSH(ref ushort register)
        {
            memoryMap.PushToStack(register);
            registers.registerPC++;
        }
        /// <summary>
        /// Halts the current game state and sets flags
        /// </summary>
        private void HALT()
        {

        }
        //{
        //    if (IMEFlag)
        //    {
        //        // actually suspend the CPU;
        //    }
        //    else if (// no interrupts are enabled)
        //    {
        //        // do nothing
        //    }
        //    else
        //    {
        //        // CPU continues execution after halt but the byte after it is read twice in a row. PC is not incremented 
        //    }
        //    registers.registerPC++;
        //}

        /// <summary>
        /// Jumps to address provided
        /// </summary>
        /// <param name="address"></param>
        public void JP(ushort address)
        {
            registers.registerPC = address;
        }

        public void JP(bool flag, ushort address)
        {
            if (flag)
            {
                JP(address);
            }
            else
            {
                registers.registerPC++;
            }
        }

        /// <summary>
        /// We could just have registser1, register2, and the address as parameters and then always save to that address just for these operational functions
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void CP(ref ushort register1, byte register2, bool isUpper)
        {
            byte chosenByte;
            byte result;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                result = (byte)(chosenByte - register2);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                result = (byte)(chosenByte - register2);
            }

            SetCPFlags(result, register1, register2);
            registers.registerPC++;
        }

        private void CP(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            byte chosenByte;
            byte result;

            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
            }
            var value = this.memoryMap.GetMemoryValue(address);
            result = (byte)(value - chosenByte);
            SetCPFlags(result, register1, register2);
        }

        private void SetCPFlags(byte result, ushort register1, ushort register2)
        {
            zeroFLag = result == 0;
            subtractFlagN = true;
            HalfCarryFlag = (result & 0xF) > (result & 0xF);
            carryFlag = register1 > register2;
        }

        /// <summary>
        /// Does a bitwise or operation and stores it into register 1 
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void OR(ref ushort register1, byte register2, bool isUpper)
        {
            byte lowerByte = (byte)(register1 & 0xFF);
            byte upperByte = (byte)(register1 >> 8);

            byte chosenByte;

            if (isUpper)
            {
                upperByte = (byte)(upperByte | register2);
                chosenByte = upperByte;
            }
            else
            {
                lowerByte = (byte)(lowerByte | register2);
                chosenByte = lowerByte;
            }
            register1 = (ushort)((upperByte << 8) | lowerByte);
            SetORFlags(chosenByte);
            registers.registerPC++;
        }

        /// <summary>
        /// Does a bitwise OR operation and stores it in the A register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void OR(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            byte chosenByte;
            ushort result = 0;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                result = (byte)(chosenByte | value);
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                result = (byte)(chosenByte | value);
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            SetORFlags(result);
            registers.registerPC++;
        }

        /// <summary>
        /// Does a bitwise OR operation and stores it in the A register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void OR(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            byte chosenByte;
            byte result = 0;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                result = (byte)(chosenByte | value);
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                result = (byte)(chosenByte | value);
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            SetORFlags(result);
            registers.registerPC++;
        }

        private void SetORFlags(ushort result)
        {
            zeroFLag = result == 0;
            HalfCarryFlag = false;
            subtractFlagN = false;
            carryFlag = false;
        }

        /// <summary>
        /// provides bitwise not on A register
        /// </summary>
        private void CPL()
        {
            byte a = (byte)(registers.registerAF >> 8);

            a = (byte)~a;

            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
            subtractFlagN = true;
            HalfCarryFlag = true;
            registers.registerPC++;
        }

        /// <summary>
        /// Decimal Adjust Accumulator; sets flags based off of current register states
        /// </summary>
        private void DAA()
        {
            byte adjustment = 0;
            byte a = (byte)(registers.registerAF >> 8);

            if (subtractFlagN)
            {

                if (HalfCarryFlag)
                {
                    adjustment += 0x6;
                }

                if (carryFlag)
                {
                    adjustment += 0x60;
                }

                a -= adjustment;
            }
            else
            {
                if (HalfCarryFlag || ((a & 0xF) > 0x9))
                {
                    adjustment += 0x6;
                }

                if (carryFlag || (a > 0x99))
                {
                    adjustment += 0x60;
                    carryFlag = true;
                }

                a += adjustment;
            }
            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
            SetDAAFlags(a);
            registers.registerPC++;
        }

        private void SetDAAFlags(ushort result)
        {
            zeroFLag = result == 0;
            HalfCarryFlag = false;
        }

        /// <summary>
        /// Jump to the address provided in memory
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="isUpper"></param>
        private void JR(ushort parameter, bool isUpper)
        {
            // jumps to the address provided in memory
            if (isUpper)
            {
                registers.registerPC += (ushort)(parameter >> 8);
            }
            else
            {
                registers.registerPC += (ushort)(parameter & 0xFF);
            }
        }

        /// <summary>
        /// Jump to the address provided in memory if the zero flag is set
        /// </summary>
        private void JRWithNC()
        {
            byte NCBytes = (byte)(registers.registerAF);
            // jumps to the address provided in memory from NC bits which are the two first bits of the F register
        }

        /// <summary>
        /// subtract teh value of the second register to the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void SUB(ref ushort register1, byte register2, bool isUpper)
        {
            byte upperByte = (byte)(register1 >> 8);
            byte lowerByte = (byte)(register1 & 0xFF);

            byte chosenByte;
            if (isUpper)
            {
                upperByte -= (byte)(register2);
                chosenByte = upperByte;
            }
            else
            {
                lowerByte -= register2;
                chosenByte = lowerByte;
            }

            register1 = (ushort)((upperByte << 8) | lowerByte);
            SetSUBFlags(chosenByte, register1, register2);
            registers.registerPC++;
        }

        /// <summary>
        /// subtract the value of the second register to the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void SUB(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            ushort chosenByte;
            ushort result = 0;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                result = (ushort)(chosenByte - value);
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                result = (ushort)(chosenByte - value);
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            SetSUBFlags(result, chosenByte, register2);
            registers.registerPC++;
        }

        /// <summary>
        /// subtract the value of the second register to the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void SUB(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            ushort chosenByte;
            ushort result = 0;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                result = (ushort)(chosenByte - value);
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                result = (ushort)(chosenByte - value);
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            SetSUBFlags(result, chosenByte, register2);
            registers.registerPC++;
        }

        private void SetSUBFlags(ushort result, ushort register1, ushort register2)
        {
            zeroFLag = result == 0;
            subtractFlagN = true;
            this.HalfCarryFlag = (result & 0xF) > 0xF;
            carryFlag = register2 > register1;
        }

        /// <summary>
        /// Add the value of the second register to the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void AND(ref ushort register1, byte register2, bool isUpper)
        {
            byte upperByte = (byte)(register1 >> 8);
            byte lowerByte = (byte)(register1 & 0xFF);

            ushort chosenByte;
            if (isUpper)
            {
                upperByte &= register2;
                chosenByte = upperByte;
            }
            else
            {
                lowerByte &= register2;
                chosenByte = lowerByte;
            }
            register1 = (ushort)((upperByte << 8) | lowerByte);
            registers.registerPC++;
        }

        /// <summary>
        /// Do a bitwise AND operation on the first register with the second register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void AND(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            ushort chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                var result = chosenByte & value;
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                var result = chosenByte & value;
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            registers.registerPC++;
        }

        /// <summary>
        /// Do a bitwise AND operation on the first register with the second register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void AND(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            ushort chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                var result = chosenByte & value;
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                var result = chosenByte & value;
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            registers.registerPC++;
        }

        /// add function is wrong pls fix
        private void ADD(ref ushort register1, ushort register2, ushort address)
        {
            if (register1 == address)
            {
                byte memoryValue = memoryMap.GetMemoryValueAsRefrence(address);
                memoryValue += (byte)(register2 >> 8);
            }
            else if (register2 == address)
            {
                register1 = (ushort)(memoryMap.GetMemoryValue(address) + register1);
            }
            registers.registerPC++;
        }

        private void SetADDFlags(ushort result)
        {
            zeroFLag = result == 0;
            subtractFlagN = false;
            HalfCarryFlag = (result & 0xF) > 0xF;
            carryFlag = result > 0xFF;
        }

        /// <summary>
        /// Add the value of the second register to the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void XOR(ref ushort register1, byte register2, bool isUpper)
        {
            byte upperByte = (byte)(register1 >> 8);
            byte lowerByte = (byte)(register1 & 0xFF);
            ushort chosenByte;

            if (isUpper)
            {
                upperByte ^= register2;
                chosenByte = lowerByte;
            }
            else
            {
                lowerByte ^= register2; 
                chosenByte = lowerByte;
            }
            register1 = (ushort)((upperByte << 8) | lowerByte);
            SetXORFlags(chosenByte);
            registers.registerPC++;
        }

        /// <summary>
        /// Do a xor operation on the first register with the second register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void XOR(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            ushort chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                var result = chosenByte ^ value;
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                var result = chosenByte ^ value;
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            SetXORFlags(chosenByte);
            registers.registerPC++;
        }

        /// <summary>
        /// Do a xor operation on the first register with the second register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void XOR(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            ushort chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                var result = chosenByte ^ value;
                register1 = (ushort)((register1 & 0x00FF) | result);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                var result = chosenByte ^ value;
                register1 = (ushort)((register1 & 0xFF00) | result);
            }
            SetXORFlags(chosenByte);
            registers.registerPC++;
        }

        private void SetXORFlags(ushort result)
        {
            zeroFLag = result == 0;
            HalfCarryFlag = false;
            subtractFlagN = false;
            carryFlag = false;
        }
        /// <summary>
        /// Add the value of the second register to the first register including the carry flag
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void SBC(ref ushort register1, ushort register2, bool isUpper)
        {
            byte chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                chosenByte -= (byte)register2;
                if (carryFlag)
                {
                    chosenByte--;
                    SetSBCFlags(chosenByte, register1, register2, true);
                    return;
                }
                register1 = (ushort)((register1 & 0x00FF) | chosenByte);

            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                chosenByte -= (byte)(register2);
                if (carryFlag)
                {
                    chosenByte--;
                    SetSBCFlags(chosenByte, register1, register2, true);
                    return;
                }
                register1 = (ushort)((register1 & 0xFF00) | chosenByte);
            }
            SetSBCFlags(chosenByte, register1, register2, false);
            registers.registerPC++;
        }

        /// <summary>
        /// Subtract the value of the second register from the first register including the carry flag
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void SBC(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            byte chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                var result = chosenByte - value;
                if (carryFlag)
                {
                    chosenByte--;
                    SetSBCFlags(chosenByte, register1, register2, true);
                }
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                chosenByte -= (byte)(register2 & 0xFF);
                if (carryFlag)
                {
                    chosenByte--;
                    SetSBCFlags(chosenByte, register1, register2, true);
                    return;
                }

            }
            SetSBCFlags(chosenByte, register1, register2, false);
            registers.registerPC++;
        }

        /// <summary>
        /// Subtract the value of the second register from the first register including the carry flag
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        /// <exception cref="Exception"></exception>
        private void SBC(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            var value = this.memoryMap.GetMemoryValue(address);
            byte chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
                var result = chosenByte - value;
                if (carryFlag)
                {
                    chosenByte--;
                    SetSBCFlags(chosenByte, register1, register2, true);
                }
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
                chosenByte -= (byte)(register2 & 0xFF);
                if (carryFlag)
                {
                    chosenByte--;
                    SetSBCFlags(chosenByte, register1, register2, true);
                    return;
                }

            }
            SetSBCFlags(chosenByte, register1, register2, false);
            registers.registerPC++;
        }

        private void SetSBCFlags(ushort result, ushort register1, ushort register2, bool withCarry)
        {
            zeroFLag = result == 0;
            subtractFlagN = true;
            this.HalfCarryFlag = (result & 0xF) > 0xF;
            carryFlag = register2 > register1;
        }

        /// <summary>
        /// Add the value of the second register to the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        private void ADD(ref ushort register1, ushort register2)
        {
            register1 = (ushort)(register1 + register2);
            registers.registerPC++;
        }

        /// <summary>
        /// Add the byte of a register into the first register
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void ADD(ref ushort register1, ushort register2, bool isUpper)
        {
            byte upperByte = (byte)(register1 >> 8);
            byte lowerByte = (byte)(register1 & 0xFF);

            if (isUpper)
            {
                upperByte += (byte)(register2);
            }
            else
            {
                lowerByte += (byte)(register2);
            }

            register1 = (ushort)((upperByte << 8) | lowerByte);

            registers.registerPC++;
        }

        /// <summary>
        /// Add the value of the second register to the first register plus the carry flag
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        /// <exception cref="Exception"></exception>
        private void ADD(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            byte chosenByte;
            if (isUpper)
            {
                chosenByte = (byte)(register1 >> 8);
            }
            else
            {
                chosenByte = (byte)(register1 & 0xFF);
            }

            if (address == register1)
            {
                this.memoryMap.AddMemoryValue(chosenByte, register2);
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
            registers.registerPC++;
        }

        /// <summary>
        /// Handles the flags for the ADD operation
        /// </summary>
        /// <param name="registerA"></param>
        /// <param name="registerB"></param>
        private void HandleADDFlags(ushort registerA, ushort registerB)
        {
            zeroFLag = registerA == 0;
            subtractFlagN = false;
            carryFlag = registerA + registerB > 0xFF;
            HalfCarryFlag = (registerA & 0xF) + (registerB & 0xF) > 0xF;
        }

        /// <summary>
        /// Add the value of the second register to the first register plus the carry flag
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void ADC(ref ushort register1, ushort register2, bool isUpper)
        {
            if (isUpper)
            {
                byte upperByte = (byte)(register1 >> 8);
                upperByte += (byte)(register2 >> 8);
                if (carryFlag)
                {
                    upperByte++;
                }
                register1 = (ushort)((register1 & 0x00FF) | upperByte);
            }
            else
            {
                byte lowerByte = (byte)(register1 & 0xFF);
                lowerByte += (byte)(register2 & 0xFF);
                if (carryFlag)
                {
                    lowerByte++;
                }
                register1 = (ushort)((register1 & 0xFF00) | lowerByte);
            }
            registers.registerPC++;
        }

        /// <summary>
        /// Add the value of the second register to the first register plus the carry flag
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void ADC(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            ushort chosenByte;
            if (isUpper)
                chosenByte = (byte)(register1 >> 8);
            else
                chosenByte = (byte)(register1 & 0xFF);

            chosenByte += memoryMap.GetMemoryValue(address);
            if (carryFlag)
            {
                chosenByte++;
            }

            register1 = chosenByte;
            registers.registerPC++;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        private void ADC(ref ushort register1, byte register2, ushort address, bool isUpper)
        {
            ushort chosenByte;
            if (isUpper)
                chosenByte = (byte)(register1 >> 8);
            else
                chosenByte = (byte)(register1 & 0xFF);

            chosenByte += memoryMap.GetMemoryValue(address);
            if (carryFlag)
            {
                chosenByte++;
            }
            HandleADCFlags(register1, chosenByte);
            register1 = chosenByte;
            registers.registerPC++;
        }

        /// <summary>
        /// This might be broken
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        private void HandleADCFlags(ushort register1, ushort register2)
        {
            zeroFLag = register1 == 0;
            subtractFlagN = false;
            carryFlag = register1 + register2 > 0xFF;
            HalfCarryFlag = (register1 & 0xF) + (register2 & 0xF) > 0xF;
        }

        /// <summary>
        /// Rotate register A left. aka bitwise left shift where we move the carry to the right most bit
        /// </summary>
        private void RLCA()
        {
            byte a = (byte)(registers.registerAF >> 8);
            carryFlag = (a & 0x80) != 0;

            a = (byte)(a << 1);

            if (carryFlag)
            {
                a++;
            }

            zeroFLag = false;
            subtractFlagN = false;
            HalfCarryFlag = false;

            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
            registers.registerPC++;
        }

        /// <summary>
        /// Rotate register A left through carry
        /// </summary>
        private void RLA()
        {
            byte a = (byte)(registers.registerAF >> 8);
            bool tempCarryFlag = carryFlag;
            carryFlag = (a & 0x80) != 0;

            a = (byte)(a << 1);

            if (tempCarryFlag)
            {
                a++;
            }

            zeroFLag = false;
            subtractFlagN = false;
            HalfCarryFlag = false;

            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
            registers.registerPC++;
        }

        /// <summary>
        /// Rotate register A right through carry
        /// </summary>
        private void RRA()
        {
            byte a = (byte)(registers.registerAF >> 8);
            bool tempCarryFlag = carryFlag;
            carryFlag = (a & 0x1) != 0;

            a = (byte)(a >> 1);

            if (tempCarryFlag)
            {
                a = (byte)(a | 0x80);
            }

            zeroFLag = false;
            subtractFlagN = false;
            HalfCarryFlag = false;

            registers.registerAF = (ushort)((registers.registerAF & (0x00FF)) | a);
            registers.registerPC++;
        }

        private void STOP()
        {
            registers.registerPC += 2;
            //from pandocs
            /*
            * Enter CPU very low power mode. Also used to switch between GBC double speed and normal speed CPU modes.
            * The exact behavior of this instruction is fragile and may interpret its second byte as a separate instruction (see the Pan Docs), 
            * which is why rgbasm(1) allows explicitly specifying the second byte (STOP n8) to override the default of $00 (a NOP instruction).
            */
        }

        /// <summary>
        /// Rotate register A right
        /// </summary>
        private void RRCA()
        {
            byte a = (byte)(registers.registerAF >> 8);
            carryFlag = (a & 0x1) != 0;

            a = (byte)(a >> 1);

            if (carryFlag)
            {
                a = (byte)(a | 0x80);
            }

            zeroFLag = false;
            subtractFlagN = false;
            HalfCarryFlag = false;

            registers.registerAF = (ushort)((registers.registerAF & (0x00FF)) | a);
            registers.registerPC++;
        }

        /// <summary>
        /// loads the second register into the first register, this is used for when only 8bit is being loaded
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="isUpper"></param>
        private void LD(ref ushort register1, byte register2, bool isUpper)
        {
            //FIX ALL OCCURENCES OF THIS
            if (isUpper)
                register1 = (ushort)((register1 & 0x00FF) | register2);
            else
                register1 = (ushort)((register1 & 0xFF00) | register2);
            registers.registerPC++;
        }

        private void LDWithHCIncrement(ref ushort register1, ushort register2, ushort address, bool hasUpper)
        {
            ushort value;
            if (hasUpper)
            {
                value = (ushort)(register1 >> 8);
            }
            else
            {
                value = register1;
            }

            if (address == register1)
            {
                this.memoryMap.SetMemoryValue(address, register2);
                register1++;
            }
            else if (address == register2)
            {
                var outcome = this.memoryMap.GetMemoryValue(address);
                register1 = (ushort)((value & 0x00FF) | outcome);
                register2++;
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
        }

        private void LDWithHCDecrement(ref ushort register1, ushort register2, ushort address, bool hasUpper)
        {
            ushort value;
            if (hasUpper)
            {
                value = (ushort)(register1 >> 8);
            }
            else
            {
                value = register1;
            }

            if (address == register1)
            {
                this.memoryMap.SetMemoryValue(address, register2);
                register1++;
            }
            else if (address == register2)
            {
                var outcome = this.memoryMap.GetMemoryValue(address);
                register1 = (ushort)((value & 0x00FF) | outcome);
                register2++;
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
        }


        /// <summary>
        /// Loads data from register 2 into the data at the memory location register 1 points too
        /// or loads data from the memory location register 2 points too into register 1
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <param name="isUpper"></param>
        /// <exception cref="Exception"></exception>
        private void LD(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            if (address == register1)
            {
                if (isUpper)
                    this.memoryMap.SetMemoryValue(address, (byte)(register2 >> 8));
                else
                    this.memoryMap.SetMemoryValue(address, (byte)(register2 & 0xFF));
                // GO TO MEMORY
            }
            else if (address == register2)
            {
                var outcome = this.memoryMap.GetMemoryValue(address);

                if (isUpper)
                    register1 = (ushort)((register1 & 0x00FF) | outcome);
                else
                    register1 = (ushort)((register1 & 0xFF00) | outcome);
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
            registers.registerPC++;
        }

        /// <summary>
        /// Loads the data from register2 into register1
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        private void LD(ref ushort register1, ushort register2)
        {
            register1 = register2;
            registers.registerPC++;
        }

        /// <summary>
        /// Loads the data from register2 into register1
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <exception cref="Exception"></exception>
        private void LD(ref ushort register1, ushort register2, ushort address)
        {
            if (register1 == address)
            {
                this.memoryMap.SetMemoryValue(address, register1);
            }
            else if (register2 == address)
            {
                var outcome = this.memoryMap.GetMemoryValue(address);
                register1 = outcome;
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
            registers.registerPC++;
        }

        /// <summary>
        /// Loads the data from register2 into register1
        /// </summary>
        /// <param name="register1"></param>
        /// <param name="register2"></param>
        /// <param name="address"></param>
        /// <exception cref="Exception"></exception>
        private void LD(ref ushort register1, byte register2, ushort address)
        {
            if (register1 == address)
            {
                this.memoryMap.SetMemoryValue(address, register2);
            }
            else if (register2 == address)
            {
                var outcome = this.memoryMap.GetMemoryValue(address);
                register1 = outcome;
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
            registers.registerPC++;
        }

        /// <summary>
        /// Increments an entire register
        /// </summary>
        /// <param name="register"></param>
        private void INC(ref ushort register)
        {
            register++;
            SetINCFlags(register);
            registers.registerPC++;
        }

        /// <summary>
        /// Increments a single byte of a register
        /// </summary>
        /// <param name="register"></param>
        /// <param name="Upper"></param>
        private void INC(ref ushort register, bool Upper)
        {
            byte upperByte = (byte)(register >> 8);
            byte lowerByte = (byte)(register & 0xFF);


            if (Upper)
            {
                upperByte++;
                SetDECFlags(upperByte);
            }
            else
            {
                lowerByte++;
                SetDECFlags(lowerByte);
            }

            register = (ushort)((upperByte << 8) | lowerByte);
            registers.registerPC++;
        }
        
        /// <summary>
        /// Increments the value at the memory location
        /// </summary>
        /// <param name="register"></param>
        private void INCReference(ushort register)
        {
            byte value = memoryMap.GetMemoryValue(register);
            value++;
            memoryMap.SetMemoryValue(register, value);
            SetINCFlags(value);
            registers.registerPC++;
        }

        /// <summary>
        /// Sets the flags for the INC operation
        /// </summary>
        /// <param name="value"></param>
        private void SetINCFlags(ushort value)
        {
            zeroFLag = value == 0;
            subtractFlagN = false;
            HalfCarryFlag = (value & 0xF) == 0;
        }

        /// <summary>
        /// Decrements the an entire register
        /// </summary>
        /// <param name="register"></param>
        private void DEC(ref ushort register)
        {
            register--;
            SetDECFlags(register);
            registers.registerPC++;
        }

        /// <summary>
        /// Decrements the a single byte of a register
        /// </summary>
        /// <param name="register"></param>
        /// <param name="Upper"></param>
        private void DEC(ref ushort register, bool Upper)
        {
            byte upperByte = (byte)(register >> 8);
            byte lowerByte = (byte)(register & 0xFF);


            if (Upper)
            {
                upperByte--;
                SetDECFlags(upperByte);
            }
            else
            {
                lowerByte--;
                SetDECFlags(lowerByte);
            }

            register = (ushort)((upperByte << 8) | lowerByte);
            registers.registerPC++;
        }

        /// <summary>
        /// Decrements the value at the memory location
        /// </summary>
        /// <param name="register"></param>
        private void DECReference(ushort register)
        {
            byte value = memoryMap.GetMemoryValue(register);
            value--;
            memoryMap.SetMemoryValue(register, value);

            SetDECFlags(value);
            registers.registerPC++;
        }

        /// <summary>
        /// Sets the flags for the DEC operation
        /// </summary>
        /// <param name="value"></param>
        private void SetDECFlags(ushort value)
        {
            zeroFLag = value == 0;
            subtractFlagN = true;
            HalfCarryFlag = (value & 0xF) == 0xF;
        }
        #endregion

        #region PREFIXFUNCTIONS

        #endregion

        #region TESTFUNCTIONS

        public void TestInstructionSet()
        {
            TestDEC();
            Console.WriteLine("DEC test passed");
            TestINC();
            Console.WriteLine("INC test passed");
            TestADD();
            Console.WriteLine("ADD test passed");
            TestSUB();
            Console.WriteLine("SUB test passed");
            TestXOR();
            Console.WriteLine("XOR test passed");
            TestAND();
            Console.WriteLine("AND test passed");
            TestOR();
            Console.WriteLine("OR test passed");
        }

        private void TestDEC()
        {
            ushort register = 0xFF;
            DEC(ref register);
            if (register != 0xFE)
            {
                throw new Exception("DEC failed");
            }
            register = 0b1001000000000000;
            DEC(ref register, true);
            if (register != 0b1000111100000000)
            {
                throw new Exception("DEC failed");
            }

            register = 0b0000000000000001;
            DEC(ref register, false);
            if (register != 0x0)
            {
                throw new Exception("DEC failed");
            }

        }

        private void TestINC()
        {
            ushort register = 0x00;
            INC(ref register);
            if (register != 0x01)
            {
                throw new Exception("INC failed" + "  Expected: 0x01  Recieved:" + register);
            }
            register = 0b0000000000000000;
            INC(ref register, true);
            if (register != 0b000000100000000)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("INC failed" + "  Expected: 0x10100000  Recieved:" + binaryConversion);
            }
            register = 0x0;
            INC(ref register, false); 
            if (register != 0x1)
            {
                throw new Exception("INC failed" + "  Expected: 0x02  Recieved:" + register);
            }
        }


        private void TestADD()
        {
            ushort register = 0xFE;
            ADD(ref register, 0x01);
            if (register != 0xFF)
            {
                throw new Exception("ADD failed");
            }
            register = 0x00;
            ADD(ref register, 0x01, true);
            if (register != 0b0000000100000000)
            {
                throw new Exception("ADD failed");
            }
            register = 0x00;
            ADD(ref register, 0x01, false);
            if (register != 0x01)
            {
                throw new Exception("ADD failed");
            }
        }

        private void TestSUB()
        {
            ushort register = 0xFFFF;
            SUB(ref register, 0x01, true);
            if (register != 0b1111111011111111)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("SUB failed expected: 0b1111101111111111 recieved: " + binaryConversion);
            }
            register = 0xFFFF;
            SUB(ref register, 0x01, false);
            if (register != 0b1111111111111110)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("SUB failed expected: 0b1111111111111110 recieved: " + binaryConversion);
            }
        }

        private void TestXOR()
        {
            ushort register = 0xFFFF;
            XOR(ref register, 0x01, true);
            if (register != 0b1111111011111111)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("XOR failed expected: 0b1111101111111111 recieved: " + binaryConversion);
            }
            register = 0xFFFF;
            XOR(ref register, 0x01, false);
            if (register != 0b1111111111111110)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("XOR failed expected: 0b1111111111111110 recieved: " + binaryConversion);
            }
        }

        public void TestAND()
        {
            ushort register = 0xFFFF;
            AND(ref register, 0x01, true);
            if (register != 0b0000000111111111)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("AND failed expected: 0b0000000100000001 recieved: " + binaryConversion);
            }
            register = 0xFFFF;
            AND(ref register, 0x01, false);
            if (register != 0b1111111100000001)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("AND failed expected: 0b1111111100000001 recieved: " + binaryConversion);
            }
            #endregion
        }

        public void TestOR()
        {
            ushort register = 0x0;
            OR(ref register, 0x01, true);
            if (register != 0b0000000100000000)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("OR failed expected: 0b0000000100000000 recieved: " + binaryConversion);
            }
            register = 0x0;
            OR(ref register, 0x01, false);
            if (register != 0b0000000000000001)
            {
                string binaryConversion = Convert.ToString(register, 2).PadLeft(16, '0');
                throw new Exception("OR failed expected: 0b0000000000000001 recieved: " + binaryConversion);
            }
        }
    }
}
