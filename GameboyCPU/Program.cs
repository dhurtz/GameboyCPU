// See https://aka.ms/new-console-template for more information

using System;
using GameboyCPU;
class MainClass
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        GameBoy gameBoy = new GameBoy();
        gameBoy.Run();
    }
}
