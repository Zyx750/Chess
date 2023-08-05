using UnityEngine;

namespace Chess.AI
{
    public class TranspositionTable
    {
        public const byte exact = 0;
        public const byte alpha = 1;
        public const byte beta = 2;

        public const int lookupFailed = int.MinValue;

        Board board;
        readonly ulong size;

        Entry[] entries;

        public TranspositionTable(Board board, ulong sizeMB)
        {
            this.board = board;

            int entrySize = System.Runtime.InteropServices.Marshal.SizeOf<TranspositionTable.Entry>();
            ulong sizeInBytes = sizeMB * 1024 * 1024;
            size = sizeInBytes / (ulong)entrySize;

            entries = new Entry[size];
        }

        ulong Index
        {
            get
            {
                return board.zobristHash % size;
            } 
        }

        public Move GetBestMove()
        {
            return entries[Index].bestMove;
        }

        public int LookupEval(int depth, int alpha, int beta)
        {
            Entry entry = entries[Index];
            if (entry.key == board.zobristHash)
            {
                if (entry.depth >= depth)
                {
                    if (entry.flag == exact)
                    {
                        return entry.eval;
                    }
                    if (entry.flag == alpha && entry.eval <= alpha)
                    {
                        return alpha;
                    }
                    if (entry.flag == beta && entry.eval > beta)
                    {
                        return beta;
                    }
                }
            }

            return lookupFailed;
        }

        public void StorePosition(Move bestMove, int eval, int depth, byte flag)
        {
            Entry entry = new(board.zobristHash, bestMove, eval, (byte)depth, flag);
            entries[Index] = entry;
        }

        struct Entry
        {
            public ulong key;
            public Move bestMove;
            public int eval;
            public byte depth;
            public byte flag;

            public Entry(ulong key, Move bestMove, int eval, byte depth, byte flag)
            {
                this.key = key;
                this.bestMove = bestMove;
                this.eval = eval;
                this.depth = depth;
                this.flag = flag;
            }
        }
    }
}