using System;

namespace Chess
{
    static public class ZobristHash
    {
        static readonly Random rnd = new();
        static readonly ulong[,] pieceBitstrings;
        static readonly ulong blackToMove;
        static readonly ulong[] castlingRights;
        static readonly ulong[] enPassantCol;


        public static ulong Hash(Board board, uint castling)
        {
            ulong hash = 0;

            for(int i = 0; i < 64; i++)
            {
                int piece = PieceToIndex(board[i]);
                if (piece < 0) continue;
                hash ^= pieceBitstrings[i, piece];
            }

            if (!board.whiteToMove) hash ^= blackToMove;

            hash ^= castlingRights[castling];

            hash ^= enPassantCol[Coord.Col(board.enPassantTarget)];
            return hash;
        }

        static ZobristHash()
        {
            pieceBitstrings = new ulong[64, 12];
            for (int square = 0; square < 64; square++)
            {
                for (int piece = 0; piece < 12; piece++)
                {
                    pieceBitstrings[square, piece] = RandomNumber();
                }
            }

            blackToMove = RandomNumber();

            castlingRights = new ulong[16];
            for(int i = 0; i < 16; i++)
            {
                castlingRights[i] = RandomNumber();
            }

            enPassantCol = new ulong[8];
            for (int i = 0; i < 8; i++)
            {
                enPassantCol[i] = RandomNumber();
            }
        }

        static ulong RandomNumber()
        {
            var buffer = new byte[sizeof(ulong)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        static int PieceToIndex(int piece)
        {
            if (piece == 0) return -1;
            piece = Piece.Type(piece) - 1;
            if (!Piece.IsWhite(piece)) piece += 6;
            return piece;
        }
    }
}