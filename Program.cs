﻿using System;

namespace Wadloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to begin output.");
            Console.ReadKey();
            var wad = Loader.Load("Ze29.wad");
            wad.ExportFull();
            wad.ExportTypeSum();
        }
    }
}
