using Chess.Game;

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
        MoveGenerator generator;

        public int Evaluate(Board board, MoveGenerator generator)
        {
            this.board = board;
            this.generator = generator;
            if (board.draw) return 0;

            int eval = 0;

            eval += CountMaterial();
            eval += CountMoves();

            if (!board.whiteToMove) eval *= -1;
            return eval;
        }

        int CountMaterial()
        {
            int sum = 0;
            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                int pieceVal = 0;
                switch (Piece.Type(piece))
                {
                    case Piece.queen:
                        pieceVal = queenVal;
                        break;
                    case Piece.bishop:
                        pieceVal = bishopVal;
                        break;
                    case Piece.knight:
                        pieceVal = knightVal;
                        break;
                    case Piece.rook:
                        pieceVal = rookVal;
                        break;
                    case Piece.pawn:
                        pieceVal = pawnVal;
                        break;
                }
                if (!Piece.IsWhite(piece)) pieceVal *= -1;
                sum += pieceVal;
            }
            return sum;
        }

        int CountMoves()
        {
            int sum = generator.moves.Count - generator.threat.Count;
            if(!board.whiteToMove) sum *= -1;
            return sum;
        }
    }
}