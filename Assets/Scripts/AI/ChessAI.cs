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
        MoveOrdering sort;
        TranspositionTable tTable;

        System.Diagnostics.Stopwatch timer;

        const int inf = int.MaxValue;
        const ulong transpositionTableSize = 64000;

        Move bestMoveInSearch;
        int bestEvalInSearch;

        bool searchComplete;
        int lookups;

        public void ChooseMove(bool debugInfo = false)
        {
            searchComplete = false;
            if (debugInfo) timer.Restart();

            Thread thread = new(new ThreadStart(StartSearch));
            thread.Start();
        }

        void StartSearch()
        {
            lookups = 0;
            bestMoveInSearch = null;
            bestEvalInSearch = -inf;

            Search(5, 0, -inf, inf);

            SearchComplete();
        }

        int Search(int depth, int distance, int alpha, int beta)
        {
            if(distance > 0)
            {
                if (board.hashHistory.Contains(board.zobristHash))
                {
                    return 0;
                }
            }

            int ttEval = tTable.LookupEval(depth, alpha, beta);
            if(ttEval != TranspositionTable.lookupFailed)
            {
                lookups++;
                if (distance == 0)
                {
                    bestEvalInSearch = ttEval;
                    bestMoveInSearch = tTable.GetBestMove();
                }
                return ttEval;
            }

            if (depth == 0)
            {
                int eval = evaluation.Evaluate(board, generator);
                return eval;
            }

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

            byte flag = TranspositionTable.alpha;

            sort.OrderMoves(board, moves);
            foreach(Move move in moves)
            {
                board.MakeMove(move, true);
                int eval = -Search(depth - 1, distance + 1, -beta, -alpha);
                board.UnMakeMove(move, true);
                if (eval >= beta)
                {
                    tTable.StorePosition(move, beta, depth, TranspositionTable.beta);
                    return beta;
                }

                if (eval > alpha)
                {
                    alpha = eval;
                    bestMove = move;

                    flag = TranspositionTable.exact;

                    if (distance == 0)
                    {
                        bestEvalInSearch = eval;
                        bestMoveInSearch = move;
                    }
                }
            }

            tTable.StorePosition(bestMove, alpha, depth, flag);

            return alpha;
        }

        void SearchComplete()
        {
            Debug.Log("Lookups: " + lookups);
            if (timer.IsRunning)
            {
                timer.Stop();
                Debug.Log("Best move: " + Move.MoveString(bestMoveInSearch) + '\n' + "Evaluation: " + (float)bestEvalInSearch/100 + '\n' + "Time taken: " + timer.Elapsed);
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
            sort = new();
            tTable = new(board, transpositionTableSize);
        }
    }
}