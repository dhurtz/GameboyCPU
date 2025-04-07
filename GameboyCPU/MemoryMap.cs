using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyCPU
{
    public class MemoryMap
    {

        public MemoryMap()
        {
            romFixedBank = new byte[0x4000];
            romSwitchableBank = new byte[0x4000]; // 16 banks of 16KB each, only one can be mapped at a time
            vram = new byte[0x2000];
            externalRam = new byte[0x2000]; // 8KB of external RAM, can be accessed when MBC is enabled
            workRam = new byte[0x100]; // 8KB of internal RAM, can be accessed when MBC is enabled
            workRamCGBOnly = new byte[0x100];
            echoRam = new byte[0x1E];
            oam = new byte[0xA0]; // Object Attribute Memory (Sprite data)
            notUsable = new byte[0x60]; // Not usable memory area (0xFEA0 - 0xFEFF)
            ioPorts = new byte[0x80]; // I/O Ports (0xFF00 - 0xFF7F)
            highRam = new byte[0x7F]; // High RAM (0xFF80 - 0xFFFF), also known as HRAM
        }

        public byte[] romFixedBank;
        public byte[] romSwitchableBank;
        public byte[] vram;
        public byte[] externalRam;
        public byte[] workRam; 
        public byte[] workRamCGBOnly;
        public byte[] echoRam;
        public byte[] oam; // Object Attribute Memory (Sprite data)
        public byte[] notUsable; // Not usable memory area (0xFEA0 - 0xFEFF)
        public byte[] ioPorts; // I/O Ports (0xFF00 - 0xFF7F)
        public byte[] highRam; // High RAM (0xFF80 - 0xFFFF), also known as HRAM

        byte inerruptEnableRegister = 0; // Interrupt Enable Register (IE) at 0xFFFF

        public byte StackPointer;

        public void PushToStack(byte value)
        {
            highRam[StackPointer] = value;
            StackPointer++;
        }

        public void PushToStack(ushort value)
        {
            highRam[StackPointer] = (byte)(value & 0x00FF);
            StackPointer++;
            highRam[StackPointer] = (byte)((value & 0xFF00) >> 8);
            StackPointer++;
        }
       
        public ushort PopFromStack16Bit()
        {
            byte upperByte = highRam[StackPointer];
            StackPointer--;
            byte lowerByte = highRam[StackPointer];
            StackPointer--;
            return (ushort)((upperByte << 8) | lowerByte);
        }

        public byte PopFromStack()
        {
            var value = highRam[StackPointer];
            StackPointer--;
            return value;
        }

        public byte GetMemoryValue(ushort address)
        {
            switch(address)
            { 
                case  <= 0x3FFF:
                    return romFixedBank[address];

                case <= 0x7FFF:
                    return romSwitchableBank[address - 0x4000];

                case <= 0x9FFF:
                    return vram[address - 0x8000];

                case <= 0xBFFF:
                    return externalRam[address - 0xA000];
            
                case <= 0xCFFF:
                    return workRam[address - 0xC000];

                case <= 0xDFFF:
                    return workRamCGBOnly[address - 0xD000];

                case <= 0xFDFF:
                    return echoRam[address - 0xE000];

                case <= 0xFE9F:
                    return oam[address - 0xFE00];

                case <= 0xFEFF:
                    return notUsable[address - 0xFEA0];

                case <= 0xFF7F:
                    return ioPorts[address - 0xFF00];

                case <= 0xFFFF:
                    return highRam[address - 0xFF80];
            }
        }

        public ref byte GetMemoryValueAsRefrence(ushort address)
        {
            switch (address)
            {
                case <= 0x3FFF:
                    return ref romFixedBank[address];

                case <= 0x7FFF:
                    return ref romSwitchableBank[address - 0x4000];

                case <= 0x9FFF:
                    return ref vram[address - 0x8000];

                case <= 0xBFFF:
                    return ref externalRam[address - 0xA000];

                case <= 0xCFFF:
                    return ref workRam[address - 0xC000];

                case <= 0xDFFF:
                    return ref workRamCGBOnly[address - 0xD000];

                case <= 0xFDFF:
                    return ref echoRam[address - 0xE000];

                case <= 0xFE9F:
                    return ref oam[address - 0xFE00];

                case <= 0xFEFF:
                    return ref notUsable[address - 0xFEA0];

                case <= 0xFF7F:
                    return ref ioPorts[address - 0xFF00];

                case <= 0xFFFF:
                    return ref highRam[address - 0xFF80];
            }
        }

        public void SetMemoryValue(ushort address, byte value)
        {
            switch (address)
            {
                case <= 0x3FFF:
                    romFixedBank[address] = value;
                    break;
                case <= 0x7FFF:
                    romSwitchableBank[address - 0x4000] = value;
                    break;
                case <= 0x9FFF:
                    vram[address - 0x8000] = value;
                    break;
                case <= 0xBFFF:
                    externalRam[address - 0xA000] = value;
                    break;
                case <= 0xCFFF:
                    workRam[address - 0xC000] = value;
                    break;
                case <= 0xDFFF:
                    workRamCGBOnly[address - 0xD000] = value;
                    break;
                case <= 0xFDFF:
                    echoRam[address - 0xE000] = value;
                    break;
                case <= 0xFE9F:
                    oam[address - 0xFE00] = value;
                    break;
                case <= 0xFEFF:
                    notUsable[address - 0xFEA0] = value;
                    break;
                case <= 0xFF7F:
                    ioPorts[address - 0xFF00] = value;
                    break;
                case <= 0xFFFF:
                    highRam[address - 0xFF80] = value;
                    break;
            }
        }

        public void SetMemoryValue(ushort address, ushort value)
        {
            byte lowerByte = (byte)(value & 0x00FF);
            byte upperByte = (byte)((value & 0xFF00) >> 8);
            switch (address)
            {
                case <= 0x3FFF:
                    romFixedBank[address] = lowerByte;
                    romFixedBank[address + 1] = upperByte;
                    break;
                case <= 0x7FFF:
                    romSwitchableBank[address - 0x4000] = lowerByte;
                    romSwitchableBank[address - 0x4000 + 1] = upperByte;
                    break;
                case <= 0x9FFF:
                    vram[address - 0x8000] = lowerByte;
                    vram[address - 0x8000 + 1] = upperByte;
                    break;
                case <= 0xBFFF:
                    externalRam[address - 0xA000] = lowerByte;
                    externalRam[address - 0xA000 + 1] = upperByte;
                    break;
                case <= 0xCFFF:
                    workRam[address - 0xC000] = lowerByte;
                    workRam[address - 0xC000 + 1] = upperByte;
                    break;
                case <= 0xDFFF:
                    workRamCGBOnly[address - 0xD000] = lowerByte;
                    workRamCGBOnly[address - 0xD000 + 1] = upperByte;
                    break;
                case <= 0xFDFF:
                    echoRam[address - 0xE000] = lowerByte;
                    echoRam[address - 0xE000 + 1] = upperByte;
                    break;
                case <= 0xFE9F:
                    oam[address - 0xFE00] = lowerByte;
                    oam[address - 0xFE00 + 1] = upperByte;
                    break;
                case <= 0xFEFF:
                    notUsable[address - 0xFEA0] = lowerByte;
                    notUsable[address - 0xFEA0 + 1] = upperByte;
                    break;
                case <= 0xFF7F:
                    ioPorts[address - 0xFF00] = lowerByte;
                    ioPorts[address - 0xFF00 + 1] = upperByte;
                    break;
                case <= 0xFFFF:
                    highRam[address - 0xFF80] = lowerByte;
                    highRam[address - 0xFF80 + 1] = upperByte;
                    break;
            }
        }
    
        public void AddMemoryValue(ushort address, byte value)
        {
            switch (address)
            {
                case <= 0x3FFF:
                    romFixedBank[address] += value;
                    break;
                case <= 0x7FFF:
                    romSwitchableBank[address - 0x4000] += value;
                    break;
                case <= 0x9FFF:
                    vram[address - 0x8000] += value;
                    break;
                case <= 0xBFFF:
                    externalRam[address - 0xA000] += value;
                    break;
                case <= 0xCFFF:
                    workRam[address - 0xC000] += value;
                    break;
                case <= 0xDFFF:
                    workRamCGBOnly[address - 0xD000] += value;
                    break;
                case <= 0xFDFF:
                    echoRam[address - 0xE000] += value;
                    break;
                case <= 0xFE9F:
                    oam[address - 0xFE00] += value;
                    break;
                case <= 0xFEFF:
                    notUsable[address - 0xFEA0] += value;
                    break;
                case <= 0xFF7F:
                    ioPorts[address - 0xFF00] += value;
                    break;
                case <= 0xFFFF:
                    highRam[address - 0xFF80] += value;
                    break;
            }
        }

        public void AddMemoryValue(ushort address, ushort value)
        {
            byte lowerByte = (byte)(value & 0x00FF);
            byte upperByte = (byte)((value & 0xFF00) >> 8);
            switch (address)
            {
                case <= 0x3FFF:
                    romFixedBank[address] += lowerByte;
                    romFixedBank[address + 1] += upperByte;
                    break;
                case <= 0x7FFF:
                    romSwitchableBank[address - 0x4000] += lowerByte;
                    romSwitchableBank[address - 0x4000 + 1] += upperByte;
                    break;
                case <= 0x9FFF:
                    vram[address - 0x8000] += lowerByte;
                    vram[address - 0x8000 + 1] += upperByte;
                    break;
                case <= 0xBFFF:
                    externalRam[address - 0xA000] += lowerByte;
                    externalRam[address - 0xA000 + 1] += upperByte;
                    break;
                case <= 0xCFFF:
                    workRam[address - 0xC000] += lowerByte;
                    workRam[address - 0xC000 + 1] += upperByte;
                    break;
                case <= 0xDFFF:
                    workRamCGBOnly[address - 0xD000] += lowerByte;
                    workRamCGBOnly[address - 0xD000 + 1] += upperByte;
                    break;
                case <= 0xFDFF:
                    echoRam[address - 0xE000] += lowerByte;
                    echoRam[address - 0xE000 + 1] += upperByte;
                    break;
                case <= 0xFE9F:
                    oam[address - 0xFE00] += lowerByte;
                    oam[address - 0xFE00 + 1] += upperByte;
                    break;
                case <= 0xFEFF:
                    notUsable[address - 0xFEA0] += lowerByte;
                    notUsable[address - 0xFEA0 + 1] += upperByte;
                    break;
                case <= 0xFF7F:
                    ioPorts[address - 0xFF00] += lowerByte;
                    ioPorts[address - 0xFF00 + 1] += upperByte;
                    break;
                case <= 0xFFFF:
                    highRam[address - 0xFF80] += lowerByte;
                    highRam[address - 0xFF80 + 1] += upperByte;
                    break;
            }
        }
    }
}
