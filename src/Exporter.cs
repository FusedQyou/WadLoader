using System;
using System.IO;
using Newtonsoft.Json;

namespace Wadloader
{
    public class Exporter
    {
        public static void DeserializeToJsonOutput(Wad wad)
        {
            string wadName = Path.GetFileNameWithoutExtension(Helpers.OutputPath + wad.Name);
            string json = JsonConvert.SerializeObject(wad, Formatting.Indented);
            System.IO.File.WriteAllText($"{Helpers.OutputPath}{wadName}.json", json);
            Console.WriteLine($"{wadName} has been exported.");
        }
    }
}