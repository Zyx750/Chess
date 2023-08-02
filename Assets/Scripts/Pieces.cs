namespace Chess
{
    public static class Pieces
    {
        public const int none = 0;
        public const int king = 1;
        public const int queen = 2;
        public const int bishop = 3;
        public const int knight = 4;
        public const int rook = 5;
        public const int pawn = 6;

        public const int white = 8;
        public const int black = 16;

        public const int colorMask = 0b11000;
        public const int typeMask = 0b00111;

        public static bool IsWhite(int piece)
        {
            return (piece & colorMask) == white;
        }

        public static int Type(int piece)
        {
            return piece & typeMask;
        }
        public static int Color(int piece)
        {
            return piece & colorMask;
        }

        public static string TypeString(int piece)
        {
            piece &= typeMask;
            return piece switch
            {
                1 => "King",
                2 => "Queen",
                3 => "Bishop",
                4 => "Knight",
                5 => "Rook",
                6 => "Pawn",
                _ => "None",
            };
        }
        public static string ColorString(int piece)
        {
            piece &= colorMask;
            return piece switch
            {
                8 => "White",
                16 => "Black",
                _ => "Unknown",
            };
        }
        public static char PieceChar(int piece)
        {
            return piece switch
            {
                king | white => 'K',
                queen | white => 'Q',
                bishop | white => 'B',
                knight | white => 'N',
                rook | white => 'R',
                pawn | white => 'P',
                king | black => 'k',
                queen | black => 'q',
                bishop | black => 'b',
                knight | black => 'n',
                rook | black => 'r',
                pawn | black => 'p',
                _ => '-',
            };
        }
    }
}

