using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Loader
{
    /// <summary>
    /// Parses the level JSON format used by the source game.
    /// JSON shape: {"XSize":7,"YSize":6,"Arrows":[{"Indices":[...], "color":"EBDE3DFF", ...}]}
    /// </summary>
    public static class JsonLevelParser
    {
        public static LevelDto Parse(string json)
        {
            return JsonUtility.FromJson<LevelDto>(json);
        }

        public static Color ParseHexColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.white;
            // Accept both "EBDE3DFF" and "#EBDE3DFF"
            if (hex[0] == '#') hex = hex.Substring(1);
            if (hex.Length == 6) hex += "FF";
            if (hex.Length != 8) return Color.white;

            byte r = ParseByte(hex, 0);
            byte g = ParseByte(hex, 2);
            byte b = ParseByte(hex, 4);
            byte a = ParseByte(hex, 6);
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        private static byte ParseByte(string s, int offset)
        {
            return (byte)((Hex(s[offset]) << 4) | Hex(s[offset + 1]));
        }

        private static int Hex(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return 0;
        }

        public static Vector2Int IndexToTile(int index, int xSize)
        {
            return new Vector2Int(index % xSize, index / xSize);
        }
    }
}
