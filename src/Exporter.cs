using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Wadloader
{
    public static class Exporter
    {
        public static void ExportFull(this Wad wad)
        {
            string wadName = Path.GetFileNameWithoutExtension(Helpers.OutputPath + wad.Name);
            string json = JsonConvert.SerializeObject(wad, Formatting.Indented);
            System.IO.File.WriteAllText($"{Helpers.OutputPath}{wadName}_full.json", json);
            Console.WriteLine($"{wadName} has been exported.");
        }

        public static void ExportTypeSum(this Wad wad)
        {
            string wadName = Path.GetFileNameWithoutExtension(Helpers.OutputPath + wad.Name);

            // Get the sum of all types
            var typeSum = new {
                lumps = wad.Lumps.GroupBy(lump => lump.Type).Select(lump => new { type = lump.Key, occurences = lump.Count() })
            };
            string json = JsonConvert.SerializeObject(typeSum, Formatting.Indented);

            System.IO.File.WriteAllText($"{Helpers.OutputPath}{wadName}_typesum.json", json);
            Console.WriteLine($"{wadName} type sums have been exported.");
        }
    }
}