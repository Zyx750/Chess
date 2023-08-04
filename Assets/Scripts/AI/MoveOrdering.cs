using System.Collections.Generic;
using UnityEngine;

namespace Chess.AI
{
    public class MoveOrdering
    {
        int[] moveScore;
        const int captureMultiplier = 10;

        public void OrderMoves(Board board, List<Move> moves)
        {
            moveScore = new int[moves.Count];

            for(int i = 0; i < moves.Count; i++)
            {
                moveScore[i] = 0;
                int movedPiece = Piece.Type(board[moves[i].Start]);
                int cappedPiece = Piece.Type(board[moves[i].Target]);

                if (cappedPiece != 0)
                {
                    moveScore[i] += captureMultiplier * PieceValue(cappedPiece) - PieceValue(movedPiece);
                }
                else if (moves[i].MoveFlag == Move.Flag.enPassant) moveScore[i] += captureMultiplier * Evaluation.pawnVal;

                if (moves[i].MoveFlag > 3)
                {
                    switch(moves[i].MoveFlag)
                    {
                        case Move.Flag.promoteQ:
                            moveScore[i] += Evaluation.queenVal;
                            break;
                        case Move.Flag.promoteB:
                            moveScore[i] += Evaluation.bishopVal;
                            break;
                        case Move.Flag.promoteN:
                            moveScore[i] += Evaluation.knightVal;
                            break;
                        case Move.Flag.promoteR:
                            moveScore[i] += Evaluation.rookVal;
                            break;
                    }
                }
            }

            Sort(moves);
        }

        int PieceValue(int type)
        {
            return type switch
            {
                Piece.queen => Evaluation.queenVal,
                Piece.bishop => Evaluation.bishopVal,
                Piece.knight => Evaluation.knightVal,
                Piece.rook => Evaluation.rookVal,
                Piece.pawn => Evaluation.pawnVal,
                _ => 0
            };
        }

        void Sort(List<Move> moves)
        {
            for(int i = 1; i < moves.Count; i++)
            {
                for(int j = i; j > 0; j--)
                {
                    if (moveScore[j] > moveScore[j - 1])
                    {
                        (moves[j], moves[j-1]) = (moves[j-1], moves[j]);
                        (moveScore[j], moveScore[j - 1]) = (moveScore[j - 1], moveScore[j]);
                    }
                    else break;
                }
            }
        }
    }
}