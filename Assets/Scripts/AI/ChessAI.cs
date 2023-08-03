using Chess.Game;
using System;
using System.Threading;
using UnityEngine;

namespace Chess.AI
{
    public class ChessAI : MonoBehaviour
    {
        Action<Move> onMoveMade;

        Board board;
        MoveGenerator generator;
        Evaluation evaluation;

        System.Diagnostics.Stopwatch timer;

        const int inf = int.MaxValue;

        Move bestMoveInSearch;
        int bestEvalInSearch;

        bool searchComplete;

        public void ChooseMove(bool debugInfo = false)
        {
            searchComplete = false;
            if (debugInfo) timer.Restart();

            Thread thread = new Thread(new ThreadStart(StartSearch));
            thread.Start();
        }

        void StartSearch()
        {
            bestMoveInSearch = null;
            bestEvalInSearch = -inf;

            Search(4);

            SearchComplete();
        }

        int Search(int depth, int distance = 0)
        {
            if (depth == 0) return evaluation.Evaluate(board);

            int bestEval = -inf;
            Move bestMove = null;

            var moves = generator.GenerateMoves(board);
            if(moves.Count == 0)
            {
                if(generator.KingInCheck)
                {
                    return -inf + distance;
                }
                else
                {
                    return 0;
                }
            }

            foreach(Move move in moves)
            {
                board.MakeMove(move);
                int eval = -Search(depth - 1, distance + 1);
                board.UnMakeMove(move);

                if(eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }

            if(distance == 0 && bestEval > bestEvalInSearch)
            {
                bestEvalInSearch = bestEval;
                bestMoveInSearch = bestMove;
            } 

            return bestEval;
        }

        void SearchComplete()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                Debug.Log("Best move: " + Move.MoveString(bestMoveInSearch) + '\n' + "Evaluation: " + bestEvalInSearch + '\n' + "Time taken: " + timer.Elapsed);
            }

            searchComplete = true;
        }

        void Update()
        {
            if(searchComplete)
            {
                searchComplete = false;
                onMoveMade(bestMoveInSearch);
            }
        }

        public void Init(Board board, Action<Move> onMoveMade)
        {
            generator = new();
            this.board = board;
            evaluation = new();
            timer = new();
            this.onMoveMade = onMoveMade;
        }
    }
}