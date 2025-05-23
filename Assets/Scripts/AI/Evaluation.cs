using Chess.Game;
using Chess.UI;
using System.Collections.Generic;

namespace Chess.AI
{
    public class Evaluation
    {
        public static readonly int queenVal = 900;
        public static readonly int bishopVal = 300;
        public static readonly int knightVal = 300;
        public static readonly int rookVal = 500;
        public static readonly int pawnVal = 100;

        Board board;

        bool endgame;

        public int Evaluate(Board board)
        {
            this.board = board;
            if (board.draw) return 0;

            int eval = 0;
            endgame = false;

            int whitePieces = CountMaterial(true);
            int blackPieces = CountMaterial(false);
            eval += whitePieces - blackPieces;
            if (whitePieces < 1600 && blackPieces < 1600) endgame = true;

            int whitePiecePosition = AddPieceSquareTableValues(true);
            int blackPiecePosition = AddPieceSquareTableValues(false);
            eval += whitePiecePosition - blackPiecePosition;

            if (!board.whiteToMove) eval *= -1;
            return eval;
        }

        int CountMaterial(bool white)
        {
            int sum = 0;
            List<int> queens = (white) ? board.whiteQueens : board.blackQueens;
            List<int> rooks = (white) ? board.whiteRooks : board.blackRooks;
            List<int> bishops = (white) ? board.whiteBishops : board.blackBishops;
            List<int> knights = (white) ? board.whiteKnights : board.blackKnights;
            List<int> pawns = (white) ? board.whitePawns : board.blackPawns;

            sum += queenVal * queens.Count;
            sum += rookVal * rooks.Count;
            sum += bishopVal * bishops.Count;
            sum += knightVal * knights.Count;
            sum += pawnVal * pawns.Count;

            return sum;
        }

        int AddPieceSquareTableValues(bool white)
        {
            int sum = 0;

            List<int> queens = (white) ? board.whiteQueens : board.blackQueens;
            List<int> rooks = (white) ? board.whiteRooks : board.blackRooks;
            List<int> bishops = (white) ? board.whiteBishops : board.blackBishops;
            List<int> knights = (white) ? board.whiteKnights : board.blackKnights;
            List<int> pawns = (white) ? board.whitePawns : board.blackPawns;
            int king = (white) ? board.whiteKing : board.blackKing;

            foreach (int piece in queens)
            {
                sum += PieceSquareTables.ReadFromTable(PieceSquareTables.queenTable, piece, white);
            }
            foreach (int piece in rooks)
            {
                sum += PieceSquareTables.ReadFromTable(PieceSquareTables.queenTable, piece, white);
            }
            foreach (int piece in bishops)
            {
                sum += PieceSquareTables.ReadFromTable(PieceSquareTables.queenTable, piece, white);
            }
            foreach (int piece in knights)
            {
                sum += PieceSquareTables.ReadFromTable(PieceSquareTables.queenTable, piece, white);
            }
            foreach (int piece in pawns)
            {
                sum += PieceSquareTables.ReadFromTable(PieceSquareTables.queenTable, piece, white);
            }

            if (endgame) sum += PieceSquareTables.ReadFromTable(PieceSquareTables.kingEndgameTable, king, white);
            else sum += PieceSquareTables.ReadFromTable(PieceSquareTables.kingTable, king, white);

            return sum;
        }
    }
}