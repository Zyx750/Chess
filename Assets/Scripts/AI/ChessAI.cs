using Chess.Game;
using System;
using System.Collections.Generic;

namespace Chess.AI
{
    public class ChessAI
    {
        Random rnd;
        MoveGenerator generator;

        public Move ChooseMove(Board board)
        {
            List<Move> moves = generator.GenerateMoves(board);
            return moves[rnd.Next(0, moves.Count)];
        }

        public ChessAI()
        {
            rnd = new Random();
            generator = new MoveGenerator();
        }
    }
}