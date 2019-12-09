using System.Collections.Generic;

namespace Wadloader
{
    public static class Helpers
    {
        public static string InputPath = "Input/";
        public static string OutputPath = "Output/";

        public static Dictionary<string, string> LumpTypes = new Dictionary<string, string>() {

            // Map lumps
            { "things", "map things" },
            { "linedefs", "map lines" },
            { "sidedefs", "map sides" },
            { "vertexes", "map vertices" },
            { "segs", "map segments" },
            { "ssectors", "map subsectors" },
            { "nodes", "map nodes" },
            { "sectors", "map sectors" },
            { "reject", "map reject table" },
            { "blockmap", "map blockmap" },
            { "scripts", "map acs source code" },
            { "textmap", "UDMF map data" },
            { "behaviour", "compiled acs" },
            { "dialogue", "strife conversation data" },
            { "znodes", "map nodes" },
            { "endmap", "map end marker" },

            // Other standard lumps
            { "playpal", "palette" },
            { "colormap", "colormap" },
            { "endoom", "ending screen" },
            { "demo1", "demo recording" },
            { "demo2", "demo recording" },
            { "demo3", "demo recording" },
            { "demo4", "demo recording" },
        };
    }
}