using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameboyCPU
{

    public unsafe class GameBoyCPU
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

        public unsafe struct Instruction
        {
            public byte operationCode;
            public ushort* instruction;
        }

        public unsafe Registers registers = new Registers();

        public unsafe Dictionary<byte, Action> instructionSet = new Dictionary<byte, Action>();

        public unsafe void InitializeInstructionSet()
        {
            instructionSet[0x00] = NOP;
            instructionSet[0x01] = () => LD(ref registers.registerBC, FetchParameters16Bit());
            instructionSet[0x02] = () => LD(registers.registerBC, registers.registerAF, registers.registerBC);
            instructionSet[0x03] = () => INC(ref registers.registerBC);
            instructionSet[0x04] = () => INC(ref registers.registerBC, true);
            instructionSet[0x05] = () => DEC(ref registers.registerBC, true);
            instructionSet[0X06] = () => LD(ref registers.registerBC, FetchParameters8Bit(), true);
            instructionSet[0x07] = () => RLCA();
            instructionSet[0x08] = () => LD(registers.registerAF, registers.registerSP, registers.registerAF);
            instructionSet[0x09] = () => ADD(registers.registerHL, registers.registerBC, ref registers.registerHL);
            instructionSet[0x0A] = () => LD(registers.registerAF, registers.registerBC, registers.registerBC);
            instructionSet[0x0B] = () => DEC(ref registers.registerBC);
            instructionSet[0x0C] = () => INC(ref registers.registerBC, false);
            instructionSet[0x0D] = () => DEC(ref registers.registerBC, false);
            instructionSet[0x0E] = () => LD(ref registers.registerBC, FetchParameters8Bit(), false);
            instructionSet[0x0F] = () => RRCA();
            instructionSet[0x10] = () => STOP();
            instructionSet[0x11] = () => LD(ref registers.registerDE, FetchParameters16Bit());
            instructionSet[0x12] = () => LD(registers.registerDE, (byte) (registers.registerAF >> 8), registers.registerDE);
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
            instructionSet[0x22] = () => LD()
            instructionSet[0x23] = () => INC(ref registers.registerHL);
            instructionSet[0x24] = () => INC(ref registers.registerHL, true);
            instructionSet[0x25] = () => DEC(ref registers.registerHL, true);
            instructionSet[0x26] = () => LD(ref registers.registerHL, FetchParameters8Bit(), true);
            instructionSet[0x27] = () => DAA();
            instructionSet[0x28] = () => JR(registers.registerBC, false);
            instructionSet[0x29] = () => ADD(ref registers.registerHL, registers.registerHL);
            instructionSet[0x3A] = () => LD(ref registers.registerAF, );
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
            instructionSet[0x6A] = () => LD(ref registers.registerHL, (byte)registers.)
        }


        public unsafe void ReadInstruction(Instruction *instruction)
        {

        }

        private ushort FetchParameters16Bit()
        {
            return 0;
        }

        private byte FetchParameters8Bit()
        {
            return 0;
        }

        #region INSTRUCTION_FUNCTIONS

        private void NOP()
        {
            return;
        }

        /// <summary>
        /// provides bitwise not on A register
        /// </summary>
        private void CPL()
        {
            byte a = (byte)(registers.registerAF >> 8);

            a = (byte)~a;

            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
        }

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
        }

        private void JR(ushort parameter, bool isUpper)
        {
            // jumps to the address provided in memory
        }

        private void JRWithNC()
        {
            // jumps to the address provided in memory from NC bits which are the two first bits of the F register
        }

        private void ADD(ref ushort register1, ushort register2)
        {
            register1 = (ushort)(register1 + register2);
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
            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
        }

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
            registers.registerAF = (ushort)((registers.registerAF & (0xFF00)) | a);
        }

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
            registers.registerAF = (ushort)((registers.registerAF & (0x00FF)) | a);
        }

        private void STOP()
        {
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
            registers.registerAF = (ushort)((registers.registerAF & (0x00FF)) | a);
        }

        private void ADD(ushort registerA, ushort registerB, ref ushort registerReference)
        {
            registerReference = (ushort)(registerA + registerB);
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
        }

        private void LD(ref ushort register1, ushort register2, ushort address, bool isUpper)
        {
            if (address == register1)
            {
                // GO TO MEMORY
            }
            else if (address == register2)
            {
                // GO TO MEMORY
            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }

            if (isUpper)
                register1 = (ushort)((register1 & 0x00FF) | register2);
            else
                register1 = (ushort)((register1 & 0xFF00) | register2);
        }

        private void LD(ref ushort register1, ushort register2)
        {
            register1 = register2;
        }

        private void LD(ushort register1, ushort register2, ushort Address)
        {
            if (register1 == Address)
            {
                // access memory at this address and load register 2
            }
            else if (register2 == Address)
            {

            }
            else
            {
                throw new Exception("Error: No register matches with address");
            }
        }

        private void LD(ushort register1, byte register2, ushort address)
        {

        }

        private void INC(ref ushort register)
        {
            register++;
        }

        private void INC(ref ushort register, bool Upper)
        {
            if (Upper)
            {
                byte upperByte = (byte)(register >> 8);
                upperByte++;

                register = (ushort)((register & (0x00FF)) | upperByte);
            }
            else
            {
                byte low = (byte)(register & 0xFF);
                low++;

                register = (ushort)((register & 0xFF00) | low);
            }
        }

        private void DEC(ref ushort register)
        {
            register--;
        }

        private void DEC(ref ushort register, bool Upper)
        {
            if (Upper)
            {
                byte upperByte = (byte)(register >> 8);
                upperByte--;

                register = (ushort)((register & (0x00FF)) | upperByte);
            }
            else
            {
                byte lowerByte = (byte)(register & 0xFF);
                lowerByte--;

                register = (ushort)((register & 0xFF00) | lowerByte);
            }
        }

        #endregion
    }
}
