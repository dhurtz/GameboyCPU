using System;
using System.Collections.Generic;
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
            public byte registerF;
            public ushort registerHL;
            
            public ushort registerSP;
            public ushort registerPC;
        }

        public bool carryFlag = false;

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
            instructionSet[0x01] = () => LD(registers.registerBC, FetchParameters16Bit());
            instructionSet[0x02] = () => LD(registers.registerBC, registers.registerAF, registers.registerBC);
            instructionSet[0x03] = () => INC(ref registers.registerBC);
            instructionSet[0x04] = () => INC(ref registers.registerBC, true);
            instructionSet[0x05] = () => DEC(ref registers.registerBC, true);
            instructionSet[0X06] = () => LD((byte)registers.registerBC, FetchParameters8Bit());
            instructionSet[0x07] = () => RLCA();
            instructionSet[0x08] = () => LD(registers.registerAF, registers.registerSP, registers.registerAF);
            instructionSet[0x09] = () => ADD(registers.registerHL, registers.registerBC, ref registers.registerHL);
            instructionSet[0x0A] = () => LD(registers.registerAF, registers.registerBC, registers.registerBC);
            instructionSet[0x0B] = () => DEC(ref registers.registerBC);
            instructionSet[0x0C] = () => INC(ref registers.registerBC, false);
            instructionSet[0x0D] = () => DEC(ref registers.registerBC, false);
            instructionSet[0x0E] = () => LD((byte)registers.registerBC, FetchParameters8Bit());
            instructionSet[0x0F] = () => RRCA();
            instructionSet[0x10] = () => STOP();
            instructionSet[0x11] = () => LD(registers.registerDE, FetchParameters16Bit());
            instructionSet[0x12] = () => LD(registers.registerDE, (byte) (registers.registerAF >> 8), registers.registerDE);
            instructionSet[0x13] = () => INC(ref registers.registerDE);
            instructionSet[0x14] = () => INC(ref registers.registerDE, true);
            instructionSet[0x15] = () => DEC(ref registers.registerDE, true);
            instructionSet[0x16] = () => LD(ref registers.registerDE, FetchParameters8Bit(), true);
            instructionSet[0x17] = () => RLA();
            instructionSet[0x18] = () => JR(ref registers.registerDE);
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

        private void JR(byte parameter)
        {
            // jumps to the address provided
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

        private void LD(ushort register1, ushort register2)
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
