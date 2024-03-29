using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Wadloader
{
    public static class Loader
    {
        public static Wad Load(string file)
        {
            string filePath = Helpers.InputPath + file;
            if (!File.Exists(filePath))                { throw new Exception($"File {file} does not exist.");   }
            if (Path.GetExtension(filePath) != ".wad") { throw new Exception($"File {file} is not a wadfile."); }
            if (file.Length < 4)                       { throw new Exception($"Filesize ({file.Length}) must be more than 4 bytes."); }

            string outPutFile = Helpers.OutputPath + file;
            string wadName = Path.GetFileNameWithoutExtension(outPutFile);
            string extension = Path.GetExtension(outPutFile).Replace(".", "");

            var wad = new Wad(wadName, extension);

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                {

                    // Get signature and verify we're dealing with a wadfile.
                    string signature = Encoding.UTF8.GetString(reader.ReadBytes(4)).ToLower();
                    if (signature != "iwad" && signature != "pwad") {
                        throw new Exception($"File {file} is not a wadfile.");
                    }

                    int lumpCount = reader.ReadInt32();
                    int lumpListOffset = reader.ReadInt32();

                    wad.Type = signature;
                    wad.Size = stream.Length;

                    reader.BaseStream.Seek(lumpListOffset, SeekOrigin.Begin);

                    // Read basic lump data
                    {
                        while(lumpCount-- > 0)
                        {
                            // Get lump data
                            int lumpDataAddress = reader.ReadInt32();
                            int lumpDataSize = reader.ReadInt32();
                            string lumpName = Encoding.UTF8.GetString(reader.ReadBytes(8)).TrimEnd('\0');

                            // Add the lump
                            wad.Lumps.Add(new Lump(
                                lumpName,
                                lumpDataAddress,
                                lumpDataSize
                            ));
                        }
                    }

                    Console.WriteLine($"Loaded {wad.Lumps.Count()} lumps.");

                    // Load lump types between marker positions
                    wad.SetTypeBetweenMarkers("S", "sprite");
                    wad.SetTypeBetweenMarkers("SS", "sprite");
                    wad.SetTypeBetweenMarkers("P", "patch");
                    wad.SetTypeBetweenMarkers("F", "flat");
                    
                    // Load map markers
                    {
                        wad.Lumps.Where(lump => lump.Size == 0).ToList().ForEach(lump => {

                            // Map01-Map32 marker
                            for (int mapnum = 1; mapnum <= 32; ++mapnum) {
                                if (lump.Name.ToLower() == $"map{mapnum}") {
                                    lump.Type = "map marker";
                                    break;
                                }
                            }

                            // E1M1-E4M9 marker
                            for (int mapnum = 1; mapnum <= 9; ++mapnum) {
                                for (int episode = 1; episode <= 4; ++episode) {
                                    if (lump.Name.ToLower() == $"E{mapnum}M{episode}") {
                                        lump.Type = "map marker";
                                        break;
                                    }
                                }
                            }
                        });
                    }

                    // Load standard lump types
                    {
                        foreach(Lump lump in wad.Lumps)
                        {
                            string lumpType;

                            // Try and load standard lump types from the dictionary.
                            if (!Helpers.LumpTypes.TryGetValue(lump.Name.ToLower(), out lumpType)) {

                                // If the lump isn't a marker, and we don't have a duplicate, just ignore.
                                if (!string.IsNullOrEmpty(lump.Type)) {
                                    continue;
                                }

                                // If address is 0, assume it is a marker we are dealing with.
                                if (lump.IsMarker()) {
                                    lumpType = "marker";
                                }
                                else {
                                    lumpType = "unknown";
                                }
                            }
                            else 
                            {

                                // Lump has a type already, but we received a standard lumptype from our list of types, causing a duplicate. Notify the user.
                                if (!string.IsNullOrEmpty(lump.Type)) {
                                    Console.WriteLine($"Warning, found type \"{lumpType}\" for lump \"{lump.Name}\" but it already has type \"{lump.Type}\". Ignoring.");
                                    continue;
                                }
                            }

                            lump.Type = lumpType;
                        }
                    }

                    Console.WriteLine($"Loaded lump types.");

                    // Load lump content
                    {
                        foreach(Lump lump in wad.Lumps)
                        {
                            // Create a byte object to store all binary data
                            lump.Content = new byte[lump.Size];
                            stream.Seek(lump.Adress, SeekOrigin.Begin);
                            stream.Read(lump.Content, 0, lump.Size);
                        }
                    }

                    // Load types of unknown lumps
                    {
                        foreach(Lump lump in wad.Lumps.Where(lump => lump.Type == "unknown"))
                        {
                            // Double check it's unknown, because this loop could have changed it.
                            if (lump.Type != "unknown") {
                                continue;
                            }

                            // MAPINFO
                            if (lump.Name.ToLower() == "mapinfo")
                            {
                                lump.Type = "map information";

                                // Read the lump content
                                string mapinfo = Encoding.UTF8.GetString(lump.Content);

                                // Filter out all map names, and give them a map marker type
                                while(true) {
                                    int mapNameStartIndex = mapinfo.ToLower().IndexOf("map ");
                                    if (mapNameStartIndex == -1) {
                                        break;
                                    }

                                    // After getting the map name, cut out part of the content so we will check for a different map key next time.
                                    mapNameStartIndex += 4;
                                    int mapNameEndIndex = mapinfo.IndexOf(" ", mapNameStartIndex);
                                    string lumpName = mapinfo.Substring(mapNameStartIndex, mapNameEndIndex - mapNameStartIndex).ToLower();
                                    mapinfo = mapinfo.Substring(mapNameEndIndex, mapinfo.Length - mapNameEndIndex);

                                    // Try and set a lump to the map marker, if it exists.
                                    var mapMarkerLump = wad.Lumps.Where(lump => lump.Type == "unknown").FirstOrDefault(lump => lump.Name.ToLower() == lumpName);
                                    if (mapMarkerLump == null) {
                                        continue;
                                    }

                                    mapMarkerLump.Type = "map marker";
                                }
                            }

                            // DECORATE
                            if (lump.Name.ToLower() == "decorate")
                            {
                                lump.Type = "actor decoration";

                                // Read the lump content
                                string decorate = Encoding.UTF8.GetString(lump.Content);

                                // Filter out all includes, and give the associated lumps a proper tag
                                while(true) {
                                    int includeStartIndex = decorate.ToLower().IndexOf("#include ");
                                    if (includeStartIndex == -1) {
                                        break;
                                    }

                                    // After getting the inclusion, cut out part of the content so we will check for a different inclusion next time.
                                    includeStartIndex += 9;
                                    int includeEndIndex = decorate.IndexOf("\n", includeStartIndex)-1;
                                    string lumpName = decorate.Substring(includeStartIndex, includeEndIndex - includeStartIndex).ToLower();
                                    decorate = decorate.Substring(includeEndIndex, decorate.Length - includeEndIndex);

                                    // Try and set a lump to the map marker, if it exists.
                                    var mapMarkerLump = wad.Lumps.Where(lump => lump.Type == "unknown").FirstOrDefault(lump => lump.Name.ToLower() == lumpName);
                                    if (mapMarkerLump == null) {
                                        continue;
                                    }

                                    mapMarkerLump.Type = "actor decoration";
                                }
                            }
                        }
                    }

                    Console.WriteLine("Loaded lump content.");
                }

                Console.WriteLine($"Loaded {wad.Name}.{wad.Extension} ({wad.Lumps.Sum(lump => lump.Size)} bytes)");
                return wad;
            }
        }

        static void SetTypeBetweenMarkers(this Wad wad, string markerPrefix, string type)
        {
            int startIndex = wad.Lumps.FindIndex(lump => lump.Name.ToLower() == $"{markerPrefix.ToLower()}_start");
            int endIndex = wad.Lumps.FindIndex(lump => lump.Name.ToLower() == $"{markerPrefix.ToLower()}_end");

            if (startIndex == -1 || endIndex == -1) {
                Console.WriteLine($"{markerPrefix.ToUpper()}_START and {markerPrefix.ToUpper()}_END markers not found. Skipping.");
                return;
            }

            // Update types
            wad.Lumps.Skip(startIndex).Take(endIndex - startIndex).Select(lump => {

                // Wads like Doom 2 have extra markers within markers. Ignore these extra markers.
                if (lump.IsMarker()) {
                    return lump;
                }

                lump.Type = type;
                return lump;
            }).ToList();
            return;
        }
    }
}