using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameboyCPU
{
    public unsafe class GameCartridge
    {
        public unsafe struct GameHeader
        {
            fixed byte EntryPoint[4];
            fixed ushort NintendoLogo[0x30];
            fixed byte Title[0xF];
            ushort LicenseCode;
            byte SGBFlag;
            byte CartridgeType;
            byte RomSize;
            byte RamSize;
            byte DestinationCode;
            byte OldLicenseeCode;
            byte ROMVersionNumber;
            byte HeaderCheckSum;
            ulong GlobalCheckSum;
        }

        public unsafe struct  GameHeader_Context
        {
            fixed char FileName[24];
            ushort Rom_size;
            byte* Rom_data;
            GameHeader* Header;
        }

        private MemoryMap memoryMap;

        public GameCartridge(string pathToCart, MemoryMap memoryMap)
        {
            CartLoad(pathToCart); // Load the cartridge data into memory (stub for now, implement actual loading logic)
            this.memoryMap = memoryMap;
            InitializeCartridgeTypes();
            InitializeRomTypes();
            InitializeOldLicenseeTypes();
        }

        public string[] Rom_Types = new string[0xA5];

        public string[] Cartridge_Type = new string[0xFF];

        public string[] OldLicenseeTypes = new string[0xFF];

        public void InitializeCartridgeTypes()
        {
            Cartridge_Type[0x0] = "ROM_ONLY";
            Cartridge_Type[0x1] = "MBC1";
            Cartridge_Type[0x2] = "MBC1+RAM";
            Cartridge_Type[0x3] = "MBC1+RAM+BATTERY";
            Cartridge_Type[0x5] = "MBC2";
            Cartridge_Type[0x6] = "MBC2+BATTERY";
            Cartridge_Type[0x8] = "ROM+RAM";
            Cartridge_Type[0x9] = "ROM+RAM+BATTERY";
            Cartridge_Type[0xB] = "MMM01";
            Cartridge_Type[0xC] = "MMM01+RAM";
            Cartridge_Type[0xD] = "MMM01+RAM+BATTERY";
            Cartridge_Type[0xF] = "MBC3+TIMER+BATTERY";
            Cartridge_Type[0x10] = "MBC3+TIMER+RAM+BATTERY";
            Cartridge_Type[0x11] = "MBC3";
            Cartridge_Type[0x12] = "MBC3+RAM";
            Cartridge_Type[0x13] = "MBC3+RAM+BATTERY";
            Cartridge_Type[0x19] = "MBC5";
            Cartridge_Type[0x1A] = "MBC5+RAM";
            Cartridge_Type[0x1B] = "MBC5+RAM+BATTERY";
            Cartridge_Type[0x1C] = "MBC5+RUMBLE";
            Cartridge_Type[0x1D] = "MBC5+RUMBLE+RAM";
            Cartridge_Type[0x1E] = "MBC5+RUMBLE+RAM+BATTERY";
            Cartridge_Type[0x20] = "MBC6";
            Cartridge_Type[0x22] = "MBC7+SENSOR+RUMBLE+RAM+BATTERY";
            Cartridge_Type[0xFC] = "POCKET CAMERA";
            Cartridge_Type[0xFD] = "BANDAI TAMA5";
            Cartridge_Type[0xFE] = "HuC3";
            Cartridge_Type[0xFF] = "HuC1+RAM+BATTERY";
        }
        
        public void InitializeRomTypes()
        {
            Rom_Types[0x0C] = "Elite Systems";
            Rom_Types[0x13] = "EA (Electronic Arts)";
            Rom_Types[0x18] = "Hudson Soft";
            Rom_Types[0x19] = "ITC Entertainment";
            Rom_Types[0x1A] = "Yanoman";
            Rom_Types[0x1D] = "Japan Clary";
            Rom_Types[0x1F] = "Virgin Games Ltd.";
            Rom_Types[0x24] = "PCM Complete";
            Rom_Types[0x25] = "San-X";
            Rom_Types[0x28] = "Kemco";
            Rom_Types[0x29] = "SETA Corporation";
            Rom_Types[0x30] = "Infogrames";
            Rom_Types[0x31] = "Nintendo";
            Rom_Types[0x32] = "Bandai";
            Rom_Types[0x33] = "Indicates that the New licensee code should be used instead.";
            Rom_Types[0x34] = "Konami";
            Rom_Types[0x35] = "HectorSoft";
            Rom_Types[0x38] = "Capcom";
            Rom_Types[0x39] = "Banpresto";
            Rom_Types[0x3C] = "Entertainment Interactive (stub)";
            Rom_Types[0x3E] = "Gremlin";
            Rom_Types[0x41] = "Ubi Soft";
            Rom_Types[0x42] = "Atlus";
            Rom_Types[0x44] = "Malibu Interactive";
            Rom_Types[0x46] = "Angel";
            Rom_Types[0x47] = "Spectrum HoloByte";
            Rom_Types[0x49] = "Irem";
            Rom_Types[0x4A] = "Virgin Games Ltd.";
            Rom_Types[0x4D] = "Malibu Interactive";
            Rom_Types[0x4F] = "U.S. Gold";
            Rom_Types[0x50] = "Absolute";
            Rom_Types[0x51] = "Acclaim Entertainment";
            Rom_Types[0x52] = "Activision";
            Rom_Types[0x53] = "Sammy USA Corporation";
            Rom_Types[0x54] = "GameTek";
            Rom_Types[0x55] = "Park Place";
            Rom_Types[0x56] = "LJN";
            Rom_Types[0x57] = "Matchbox";
            Rom_Types[0x59] = "Milton Bradley Company";
            Rom_Types[0x5A] = "Mindscape";
            Rom_Types[0x5B] = "Romstar";
            Rom_Types[0x5C] = "Naxat Soft";
            Rom_Types[0x5D] = "Tradewest";
            Rom_Types[0x60] = "Titus Interactive";
            Rom_Types[0x61] = "Virgin Games Ltd.";
            Rom_Types[0x67] = "Ocean Software";
            Rom_Types[0x69] = "EA (Electronic Arts)";
            Rom_Types[0x6E] = "Elite Systems";
            Rom_Types[0x6F] = "Electro Brain";
            Rom_Types[0x70] = "Infogrames";
            Rom_Types[0x71] = "Interplay Entertainment";
            Rom_Types[0x72] = "Broderbund";
            Rom_Types[0x73] = "Sculptured Software";
            Rom_Types[0x75] = "The Sales Curve Limited";
            Rom_Types[0x78] = "THQ";
            Rom_Types[0x79] = "Accolade";
            Rom_Types[0x7A] = "Triffix Entertainment";
            Rom_Types[0x7C] = "MicroProse";
            Rom_Types[0x7F] = "Kemco";
            Rom_Types[0x80] = "Misawa Entertainment";
            Rom_Types[0x83] = "LOZC G.";
            Rom_Types[0x86] = "Tokuma Shoten";
            Rom_Types[0x8B] = "Bullet-Proof Software";
            Rom_Types[0x8C] = "Vic Tokai Corp.";
            Rom_Types[0x8E] = "Ape Inc.";
            Rom_Types[0x8F] = "I’Max";
            Rom_Types[0x91] = "Chunsoft Co.";
            Rom_Types[0x92] = "Video System";
            Rom_Types[0x93] = "Tsubaraya Productions";
            Rom_Types[0x95] = "Varie";
            Rom_Types[0x96] = "Yonezawa/S’Pal";
            Rom_Types[0x97] = "Kemco";
            Rom_Types[0x99] = "Arc";
            Rom_Types[0x9A] = "Nihon Bussan";
            Rom_Types[0x9B] = "Tecmo";
            Rom_Types[0x9C] = "Imagineer";
            Rom_Types[0x9D] = "Banpresto";
            Rom_Types[0x9F] = "Nova";
            Rom_Types[0xA1] = "Hori Electric";
            Rom_Types[0xA2] = "Bandai";
            Rom_Types[0xA4] = "Konami";
            Rom_Types[0xA6] = "Kawada";
            Rom_Types[0xA7] = "Takara";
            Rom_Types[0xA9] = "Technos Japan";
            Rom_Types[0xAA] = "Broderbund";
            Rom_Types[0xAC] = "Toei Animation";
            Rom_Types[0xAD] = "Toho";
            Rom_Types[0xAF] = "Namco";
            Rom_Types[0xB0] = "Acclaim Entertainment";
            Rom_Types[0xB1] = "ASCII Corporation or Nexsoft";
            Rom_Types[0xB2] = "Bandai";
            Rom_Types[0xB4] = "Square Enix";
            Rom_Types[0xB6] = "HAL Laboratory";
            Rom_Types[0xB7] = "SNK";
            Rom_Types[0xB9] = "Pony Canyon";
            Rom_Types[0xBA] = "Culture Brain";
            Rom_Types[0xBB] = "Sunsoft";
            Rom_Types[0xBD] = "Sony Imagesoft";
            Rom_Types[0xBF] = "Sammy Corporation";
            Rom_Types[0xC0] = "Taito";
            Rom_Types[0xC2] = "Kemco";
            Rom_Types[0xC3] = "Square";
            Rom_Types[0xC4] = "Tokuma Shoten";
            Rom_Types[0xC5] = "Data East";
            Rom_Types[0xC6] = "Tonkin House";
            Rom_Types[0xC8] = "Koei";
            Rom_Types[0xC9] = "UFL";
            Rom_Types[0xCA] = "Ultra Games";
            Rom_Types[0xCB] = "VAP, Inc.";
            Rom_Types[0xCC] = "Use Corporation";
            Rom_Types[0xCD] = "Meldac";
            Rom_Types[0xCE] = "Pony Canyon";
            Rom_Types[0xCF] = "Angel";
            Rom_Types[0xD0] = "Taito";
            Rom_Types[0xD1] = "SOFEL (Software Engineering Lab)";
            Rom_Types[0xD2] = "Quest";
            Rom_Types[0xD3] = "Sigma Enterprises";
            Rom_Types[0xD4] = "ASK Kodansha Co.";
            Rom_Types[0xD6] = "Naxat Soft";
            Rom_Types[0xD7] = "Copya System";
            Rom_Types[0xD9] = "Banpresto";
            Rom_Types[0xDA] = "Tomy";
            Rom_Types[0xDB] = "LJN";
            Rom_Types[0xDD] = "Nippon Computer Systems";
            Rom_Types[0xDE] = "Human Ent.";
            Rom_Types[0xDF] = "Altron";
            Rom_Types[0xE0] = "Jaleco";
            Rom_Types[0xE1] = "Towa Chiki";
            Rom_Types[0xE2] = "Yutaka";
            Rom_Types[0xE3] = "Varie";
            Rom_Types[0xE5] = "Epoch";
            Rom_Types[0xE7] = "Athena";
            Rom_Types[0xE8] = "Asmik Ace Entertainment";
            Rom_Types[0xE9] = "Natsume";
            Rom_Types[0xEA] = "King Records";
            Rom_Types[0xEB] = "Atlus";
            Rom_Types[0xEC] = "Epic/Sony Records";
            Rom_Types[0xEE] = "IGS";
            Rom_Types[0xF0] = "A Wave";
            Rom_Types[0xF3] = "Extreme Entertainment";
            Rom_Types[0xFF] = "LJN";
        }
        
        public void InitializeOldLicenseeTypes()
        {
            OldLicenseeTypes[0x00] = "None";
            OldLicenseeTypes[0x01] = "Nintendo";
            OldLicenseeTypes[0x08] = "Capcom";
            OldLicenseeTypes[0x09] = "HOT-B";
            OldLicenseeTypes[0x0A] = "Jaleco";
            OldLicenseeTypes[0x0B] = "Coconuts Japan";
            OldLicenseeTypes[0x0C] = "Elite Systems";
            OldLicenseeTypes[0x13] = "EA (Electronic Arts)";
            OldLicenseeTypes[0x18] = "Hudson Soft";
            OldLicenseeTypes[0x19] = "ITC Entertainment";
            OldLicenseeTypes[0x1A] = "Yanoman";
            OldLicenseeTypes[0x1D] = "Japan Clary";
            OldLicenseeTypes[0x1F] = "Virgin Games Ltd.";
            OldLicenseeTypes[0x24] = "PCM Complete";
            OldLicenseeTypes[0x25] = "San-X";
            OldLicenseeTypes[0x28] = "Kemco";
            OldLicenseeTypes[0x29] = "SETA Corporation";
            OldLicenseeTypes[0x30] = "Infogrames";
            OldLicenseeTypes[0x31] = "Nintendo";
            OldLicenseeTypes[0x32] = "Bandai";
            OldLicenseeTypes[0x33] = "Indicates that the New licensee code should be used instead.";
            OldLicenseeTypes[0x34] = "Konami";
            OldLicenseeTypes[0x35] = "HectorSoft";
            OldLicenseeTypes[0x38] = "Capcom";
            OldLicenseeTypes[0x39] = "Banpresto";
            OldLicenseeTypes[0x3C] = "Entertainment Interactive (stub)";
            OldLicenseeTypes[0x3E] = "Gremlin";
            OldLicenseeTypes[0x41] = "Ubi Soft";
            OldLicenseeTypes[0x42] = "Atlus";
            OldLicenseeTypes[0x44] = "Malibu Interactive";
            OldLicenseeTypes[0x46] = "Angel";
            OldLicenseeTypes[0x47] = "Spectrum HoloByte";
            OldLicenseeTypes[0x49] = "Irem";
            OldLicenseeTypes[0x4A] = "Virgin Games Ltd.";
            OldLicenseeTypes[0x4D] = "Malibu Interactive";
            OldLicenseeTypes[0x4F] = "U.S. Gold";
            OldLicenseeTypes[0x50] = "Absolute";
            OldLicenseeTypes[0x51] = "Acclaim Entertainment";
            OldLicenseeTypes[0x52] = "Activision";
            OldLicenseeTypes[0x53] = "Sammy USA Corporation";
            OldLicenseeTypes[0x54] = "GameTek";
            OldLicenseeTypes[0x55] = "Park Place";
            OldLicenseeTypes[0x56] = "LJN";
            OldLicenseeTypes[0x57] = "Matchbox";
            OldLicenseeTypes[0x59] = "Milton Bradley Company";
            OldLicenseeTypes[0x5A] = "Mindscape";
            OldLicenseeTypes[0x5B] = "Romstar";
            OldLicenseeTypes[0x5C] = "Naxat Soft";
            OldLicenseeTypes[0x5D] = "Tradewest";
            OldLicenseeTypes[0x60] = "Titus Interactive";
            OldLicenseeTypes[0x61] = "Virgin Games Ltd.";
            OldLicenseeTypes[0x67] = "Ocean Software";
            OldLicenseeTypes[0x69] = "EA (Electronic Arts)";
            OldLicenseeTypes[0x6E] = "Elite Systems";
            OldLicenseeTypes[0x6F] = "Electro Brain";
            OldLicenseeTypes[0x70] = "Infogrames";
            OldLicenseeTypes[0x71] = "Interplay Entertainment";
            OldLicenseeTypes[0x72] = "Broderbund";
            OldLicenseeTypes[0x73] = "Sculptured Software";
            OldLicenseeTypes[0x75] = "The Sales Curve Limited";
            OldLicenseeTypes[0x78] = "THQ";
            OldLicenseeTypes[0x79] = "Accolade";
            OldLicenseeTypes[0x7A] = "Triffix Entertainment";
            OldLicenseeTypes[0x7C] = "MicroProse";
            OldLicenseeTypes[0x7F] = "Kemco";
            OldLicenseeTypes[0x80] = "Misawa Entertainment";
            OldLicenseeTypes[0x83] = "LOZC G.";
            OldLicenseeTypes[0x86] = "Tokuma Shoten";
            OldLicenseeTypes[0x8B] = "Bullet-Proof Software";
            OldLicenseeTypes[0x8C] = "Vic Tokai Corp.";
            OldLicenseeTypes[0x8E] = "Ape Inc.";
            OldLicenseeTypes[0x8F] = "I’Max";
            OldLicenseeTypes[0x91] = "Chunsoft Co.";
            OldLicenseeTypes[0x92] = "Video System";
            OldLicenseeTypes[0x93] = "Tsubaraya Productions";
            OldLicenseeTypes[0x95] = "Varie";
            OldLicenseeTypes[0x96] = "Yonezawa/S’Pal";
            OldLicenseeTypes[0x97] = "Kemco";
            OldLicenseeTypes[0x99] = "Arc";
            OldLicenseeTypes[0x9A] = "Nihon Bussan";
            OldLicenseeTypes[0x9B] = "Tecmo";
            OldLicenseeTypes[0x9C] = "Imagineer";
            OldLicenseeTypes[0x9D] = "Banpresto";
            OldLicenseeTypes[0x9F] = "Nova";
            OldLicenseeTypes[0xA1] = "Hori Electric";
            OldLicenseeTypes[0xA2] = "Bandai";
            OldLicenseeTypes[0xA4] = "Konami";
            OldLicenseeTypes[0xA6] = "Kawada";
            OldLicenseeTypes[0xA7] = "Takara";
            OldLicenseeTypes[0xA9] = "Technos Japan";
            OldLicenseeTypes[0xAA] = "Broderbund";
            OldLicenseeTypes[0xAC] = "Toei Animation";
            OldLicenseeTypes[0xAD] = "Toho";
            OldLicenseeTypes[0xAF] = "Namco";
            OldLicenseeTypes[0xB0] = "Acclaim Entertainment";
            OldLicenseeTypes[0xB1] = "ASCII Corporation or Nexsoft";
            OldLicenseeTypes[0xB2] = "Bandai";
            OldLicenseeTypes[0xB4] = "Square Enix";
            OldLicenseeTypes[0xB6] = "HAL Laboratory";
            OldLicenseeTypes[0xB7] = "SNK";
            OldLicenseeTypes[0xB9] = "Pony Canyon";
            OldLicenseeTypes[0xBA] = "Culture Brain";
            OldLicenseeTypes[0xBB] = "Sunsoft";
            OldLicenseeTypes[0xBD] = "Sony Imagesoft";
            OldLicenseeTypes[0xBF] = "Sammy Corporation";
            OldLicenseeTypes[0xC0] = "Taito";
            OldLicenseeTypes[0xC2] = "Kemco";
            OldLicenseeTypes[0xC3] = "Square";
            OldLicenseeTypes[0xC4] = "Tokuma Shoten";
            OldLicenseeTypes[0xC5] = "Data East";
            OldLicenseeTypes[0xC6] = "Tonkin House";
            OldLicenseeTypes[0xC8] = "Koei";
            OldLicenseeTypes[0xC9] = "UFL";
            OldLicenseeTypes[0xCA] = "Ultra Games";
            OldLicenseeTypes[0xCB] = "VAP, Inc.";
            OldLicenseeTypes[0xCC] = "Use Corporation";
            OldLicenseeTypes[0xCD] = "Meldac";
            OldLicenseeTypes[0xCE] = "Pony Canyon";
            OldLicenseeTypes[0xCF] = "Angel";
            OldLicenseeTypes[0xD0] = "Taito";
            OldLicenseeTypes[0xD1] = "SOFEL";
            OldLicenseeTypes[0xD2] = "Quest";
            OldLicenseeTypes[0xD3] = "Sigma Enterprises";
            OldLicenseeTypes[0xD4] = "ASK Kodansha Co.";
            OldLicenseeTypes[0xD6] = "Naxat Soft";
            OldLicenseeTypes[0xD7] = "Copya System";
            OldLicenseeTypes[0xD9] = "Banpresto";
            OldLicenseeTypes[0xDA] = "Tomy";
            OldLicenseeTypes[0xDB] = "LJN";
            OldLicenseeTypes[0xDD] = "Nippon Computer Systems";
            OldLicenseeTypes[0xDE] = "Human Ent.";
            OldLicenseeTypes[0xDF] = "Altron";
            OldLicenseeTypes[0xE0] = "Jaleco";
            OldLicenseeTypes[0xFF] = "LJN";
        }

        public static string[] LicenseeCodes = new string[0xA5];

        public static unsafe bool CartLoad(string cart)
        {
            return true;
        }
    }
}
