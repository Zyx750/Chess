using Chess;
using Chess.Game;

public class Evaluation
{
    const int queenVal = 900;
    const int bishopVal = 300;
    const int knightVal = 300;
    const int rookVal = 500;
    const int pawnVal = 100;

    Board board;

    public int Evaluate(Board board)
    {
        this.board = board;
        if (board.draw) return 0;

        int eval = 0;
        eval += CountMaterial();
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
            switch(Piece.Type(piece))
            {
                case 2:
                    pieceVal = queenVal;
                    break;
                case 3:
                    pieceVal = bishopVal;
                    break;
                case 4:
                    pieceVal = knightVal;
                    break;
                case 5:
                    pieceVal = rookVal;
                    break;
                case 6:
                    pieceVal = pawnVal;
                    break;
            }
            if (!Piece.IsWhite(piece)) pieceVal *= -1;
            sum += pieceVal;
        }
        return sum;
    }
}
