using System;

namespace RO.Databases
{
    public static class MapDb
    {
        public enum MapIds : int
        {
            gl_knt01 = 0,
            glast_01,
            izlude,
            new_zone01,
            prontera,
            valkyrie,

            Last
        }

        public readonly static string[] MapNames = Enum.GetNames(typeof(MapIds));

        public readonly static string[] MapBgms;

        static MapDb()
        {
            MapBgms = new string[(int)MapIds.Last];

            //Map id -> bgm name in client -> bgm number in ro
            MapBgms[(int)MapIds.gl_knt01] = "gl_knt"; // 44
            MapBgms[(int)MapIds.glast_01] = "glast_01"; //42
            MapBgms[(int)MapIds.new_zone01] = "new_zone"; // 22
            MapBgms[(int)MapIds.izlude] = "izlude"; // 26
            MapBgms[(int)MapIds.prontera] = "prontera"; // 8
            MapBgms[(int)MapIds.valkyrie] = "valkyrie"; // 9
        }
    }
}
