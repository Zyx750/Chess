using Chess.Game;
using System;
using System.Threading;
using System.Threading.Tasks;
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
        CancellationTokenSource searchAbortTimer;

        const int inf = int.MaxValue;
        const ulong transpositionTableSizeMB = 64;
        public int searchTimeMillis = 1000;
        const int maxDepth = 100;

        Move bestMoveInSearch;
        int bestEvalInSearch;
        Move bestMoveThisIteration;
        int bestEvalThisIteration;

        bool searchComplete;
        bool abortSearch;

        uint lookups;
        int searchDepth;

        public void ChooseMove(bool debugInfo = false)
        {
            searchComplete = false;
            abortSearch = false;
            if (debugInfo) timer.Restart();

            Thread thread = new(new ThreadStart(StartSearch));
            thread.Start();

            searchAbortTimer = new();
            Task.Delay(searchTimeMillis, searchAbortTimer.Token).ContinueWith((t) => EndSearch());
        }

        void StartSearch()
        {
            lookups = 0;
            bestMoveInSearch = null;
            bestEvalInSearch = -inf;

            for (int i = 1; i <= maxDepth; i++)
            {
                bestMoveThisIteration = null;
                bestEvalThisIteration = -inf;

                Search(i, 0, -inf, inf);

                if (abortSearch)
                {
                    break;
                }
                else
                {
                    bestMoveInSearch = bestMoveThisIteration;
                    bestEvalInSearch = bestEvalThisIteration;
                    searchDepth = i;
                }
            }

            SearchComplete();
        }

        int Search(int depth, int distance, int alpha, int beta)
        {
            if (distance > 0)
            {
                if (board.hashHistory.Contains(board.zobristHash))
                {
                    return 0;
                }
            }

            int ttEval = tTable.LookupEval(depth, alpha, beta);
            if (ttEval != TranspositionTable.lookupFailed)
            {
                lookups++;
                if (distance == 0)
                {
                    bestEvalThisIteration = ttEval;
                    bestMoveThisIteration = tTable.GetBestMove();
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
            if (moves.Count == 0)
            {
                if (generator.KingInCheck)
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
            foreach (Move move in moves)
            {
                board.MakeMove(move, true);
                int eval = -Search(depth - 1, distance + 1, -beta, -alpha);
                board.UnMakeMove(move, true);

                if (abortSearch) return alpha;

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
                        bestEvalThisIteration = eval;
                        bestMoveThisIteration = move;
                    }
                }
            }

            tTable.StorePosition(bestMove, alpha, depth, flag);

            return alpha;
        }

        void SearchComplete()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
<<<<<<< HEAD
=======
                Debug.Log("Lookups: " + lookups);
>>>>>>> origin/ui
                Debug.Log("Best move: " + Move.MoveString(bestMoveInSearch) + '\n' + "Evaluation: " + (float)bestEvalInSearch / 100 + '\n' + "Time taken: " + timer.Elapsed + '\n' + "Depth: " + searchDepth);
            }

            searchComplete = true;
        }

        void EndSearch()
        {
            abortSearch = true;
        }

        void Update()
        {
            if (searchComplete)
            {
                searchComplete = false;
                onMoveMade(bestMoveInSearch);
            }
        }

        public void Init(Board board, Action<Move> onMoveMade, int searchTimeMillis)
        {
            this.board = board;
            this.onMoveMade = onMoveMade;
            this.searchTimeMillis = searchTimeMillis;
            generator = new();
            evaluation = new();
            timer = new();
            tTable = new(board, transpositionTableSizeMB);
            sort = new(tTable);
        }
    }
}