using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyCPU
{
    public class GameBoy
    {
        public GameBoy()
        {

        }

        public void Run()
        {
            MemoryMap memoryMap = new MemoryMap();
            GameBoyCPU cpu = new GameBoyCPU(memoryMap);
            GameCartridge header = new GameCartridge("C:\\Users\\Dustin\\source\\repos\\GameboyCPU\\GameboyCPU\\cpu_instrs.gb", memoryMap);
            cpu.CPUMainLoop();
        }

        public void RunCPUTests()
        {
            MemoryMap memoryMap = new MemoryMap();
            GameBoyCPU cpu = new GameBoyCPU(memoryMap);
            cpu.TestInstructionSet();
        }
    }
}
