using System.Collections.Generic;

namespace Wadloader
{
    public class Wad
    {
        public Wad(string fileName, string extension)
        {
            Name = fileName;
            Extension = extension;
        }

        /// <summary> The file name of the wad. </summary>
        public string Name { get; set; }

        /// <summary> The extension of the wad. </summary>
        public string Extension { get; set; }

        /// <summary> The size of the wad in bytes. </summary>
        public long Size { get; set; }

        /// <summary> The type of the wad (IWAD, PWAD). </summary>
        public string Type { get; set; }

        /// <summary> The lumps inside the wad. </summary>
        public List<Lump> Lumps { get; set; } = new List<Lump>();
    }
}