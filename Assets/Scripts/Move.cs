using System;
using static Chess.Move;

namespace Chess
{
    public class Move
    {
        readonly ushort moveValue;

        //flags
        public readonly struct Flag
        {
            public const int none = 0;
            public const int enPassant = 1;
            public const int doubleMove = 2;
            public const int castling = 3;
            public const int promoteQ = 4;
            public const int promoteB = 5;
            public const int promoteN = 6;
            public const int promoteR = 7;
        }
        

        public const ushort flagMask = 0b1111000000000000;
        public const ushort toMask = 0b0000111111000000;
        public const ushort fromMask = 0b0000000000111111;

        public int MoveFlag
        {
            get
            {
                return (moveValue & flagMask) >> 12;
            }
        }
        public int Target
        {
            get
            {
                return (moveValue & toMask) >> 6;
            }
        }
        public int Start
        {
            get
            {
                return moveValue & fromMask;
            }
        }

        public Move(ushort moveValue)
        {
            this.moveValue = moveValue;
        }

        public Move(int from, int to, int flag = Flag.none)
        {
            this.moveValue = (ushort)(from | (to << 6) | (flag << 12));
        }

        public Move(Move move, int flag)
        {
            this.moveValue = (ushort)(move.Start | (move.Target << 6) | (flag << 12));
        }

        public static string MoveString(Move move)
        {
            return Coord.TileString(move.Start) + " => " + Coord.TileString(move.Target) + " flag: " + move.MoveFlag;
        }
    }
}