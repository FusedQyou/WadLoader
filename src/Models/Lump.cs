using Newtonsoft.Json;

namespace Wadloader
{
    public class Lump
    {
        public Lump(string name, int address, int size)
        {
            Name = name;
            Adress = address;
            Size = size;
        }

        /// <summary> The name of the lump. </summary>
        public string Name { get; set; }

        /// <summary> The type of the lump. </summary>
        public string Type { get; set; }

        /// <summary> The adress in memory where the lump is located. </summary>
        public int Adress { get; set; }

        /// <summary> The size of the lump in bytes. </summary>
        public int Size { get; set; }

        /// <summary> The content of the lump. </summary>
        [JsonIgnore]
        public byte[] Content { get; set; }
    }
}