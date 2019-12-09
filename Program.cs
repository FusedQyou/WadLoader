using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Wadloader.Models;

namespace Wadloader
{
    class Program
    {
        const string InputPath = "Input/";
        const string OutputPath = "Output/";


        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to begin output.");
            Console.ReadKey();
            var wad = LoadWadFileToMemory("Doom2.wad");
            DeserializeToJsonOutput(wad);
        }

        static Wad LoadWadFileToMemory(string file)
        {
            string filePath = InputPath + file;
            string outPutFile = OutputPath + file;

            if (!File.Exists(filePath))                { throw new Exception($"File {filePath} does not exist.");   }
            if (Path.GetExtension(filePath) != ".wad") { throw new Exception($"File {filePath} is not a wadfile."); }

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                string wadName = Path.GetFileNameWithoutExtension(outPutFile);
                string extension = Path.GetExtension(outPutFile).Replace(".", "");

                var wad = new Wad(Path.GetFileName(wadName));
                wad.Extension = extension;
                wad.Size = stream.Length;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] signature = reader.ReadBytes(4);
                    int lumpCount = reader.ReadInt32();
                    int lumpListOffset = reader.ReadInt32();

                    wad.Type = Encoding.UTF8.GetString(signature);

                    reader.BaseStream.Seek(lumpListOffset, SeekOrigin.Begin);

                    // Read all lumps
                    while(lumpCount-- > 0)
                    {
                        // Get lump data
                        int lumpDataAddress = reader.ReadInt32();
                        int lumpDataSize = reader.ReadInt32();
                        byte[] lumpName = reader.ReadBytes(8);

                        // Trim lump name
                        int lastIndex = Array.FindLastIndex(lumpName, b => b != 0);
                        Array.Resize(ref lumpName, lastIndex + 1);

                        // Add the lump
                        wad.Lumps.Add(new Lump(
                            Encoding.UTF8.GetString(lumpName),
                            lumpDataAddress,
                            lumpDataSize
                        ));
                    }
                }

                return wad;
            }
        }

        static void DeserializeToJsonOutput(Wad wad)
        {
            string wadName = Path.GetFileNameWithoutExtension(OutputPath + wad.Name);
            string json = JsonConvert.SerializeObject(wad, Formatting.Indented);
            System.IO.File.WriteAllText($"{OutputPath}{wadName}.json", json);
            Console.WriteLine($"{wadName} has been exported.");
        }
    }
}
