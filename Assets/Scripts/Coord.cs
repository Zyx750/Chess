using System;

namespace Chess
{
    public static class Coord
    {
        public const int colMask = 0b000111;
        public const int rowMask = 0b111000;

        public static int Col(int index)
        {
            return index & colMask;
        }

        public static int Row(int index)
        {
            return 7 - (index & rowMask)/8;
        }

        public static string TileString(int index)
        {
            if (index < 0 || 63 < index) return "none";
            char col = (char)('a' + Col(index));
            return col + (Row(index) + 1).ToString();
        }
        public static int CoordFromString(string tile)
        {
            int row = tile[1] - '1';
            int col = tile[0] - 'a';
            return Combine(7-row, col);
        }

        public static int Combine(int row, int col)
        {
            return 8 * row + col;
        }

        public static bool IsWhite(int index)
        {
            return (Col(index) + Row(index)) % 2 == 0;
        }

        public static int KingDistance(int from, int to)
        {
            return Math.Max(Math.Abs(Row(from) - Row(to)), Math.Abs(Col(from) - Col(to)));
        }
    }
}