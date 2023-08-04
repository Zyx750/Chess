using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
{
    public class MoveGenerator
    {
        public List<Move> moves;
        public HashSet<int> threat;
        Board board;

        static readonly int[][] squaresToEdge;
        static readonly int[] directions = {-8, 8, -1, 1, -9, 9, -7, 7};
        static readonly byte[][] knightMoves;
        static readonly int[] pawnCaptures = { -1, 1 };
        static readonly int[] kingVectors = {-9, -8, -7, -1, 1, 7, 8, 9};

        public bool KingInCheck = false;
        bool DoubleCheck = false;
        HashSet<int> blockCheckSquares;
        Dictionary<int, int> pins; //key = piece; direction = direction (index) in which the piece can move (piece can also move in the opposite direction)
        int epPin;

        public List<Move> GenerateMoves(Board board)
        {
            this.board = board;
            moves = new List<Move>();
            GenerateThreat();
            if(!DoubleCheck)
            {
                GenerateSlidingMoves();
                GenerateKnightMoves();
                GeneratePawnMoves();
                ExcludePseudoLegalMoves();
            }
            GenerateKingMoves();
            //Debug.Log(moves.Count);
            //DebugPrintMoves();
            return moves;
        }

        void GenerateSlidingMoves()
        {
            List<int> queens = (board.whiteToMove) ? board.whiteQueens : board.blackQueens;
            List<int> rooks = (board.whiteToMove) ? board.whiteRooks : board.blackRooks;
            List<int> bishops = (board.whiteToMove) ? board.whiteBishops : board.blackBishops;

            foreach (int index in queens)
            {
                int piece = board[index];
                GenerateSlidingPieceMoves(0, 8, index);
            }
            foreach (int index in rooks)
            {
                int piece = board[index];
                GenerateSlidingPieceMoves(0, 4, index);
            }
            foreach (int index in bishops)
            {
                int piece = board[index];
                GenerateSlidingPieceMoves(4, 8, index);
            }
        }

        void GenerateSlidingPieceMoves(int startIndex, int endIndex, int index)
        {
            int friendly = (board.whiteToMove) ? Piece.white : Piece.black;
            if(pins.ContainsKey(index))
            {
                if (pins[index] < startIndex) return;
                if (pins[index] >= endIndex) return;
                if (directions[pins[index]] < 0)
                {
                    startIndex = pins[index];
                    endIndex = pins[index] + 2;
                }
                else
                {
                    startIndex = pins[index] - 1;
                    endIndex = pins[index] + 1;
                }
            }
            for (int dir = startIndex; dir < endIndex; dir++)
            {
                for(int dist = 1; dist <= squaresToEdge[index][dir]; dist++)
                {
                    int target = index + dist * directions[dir];
                    int targetPiece = board[target];
                    if (Piece.Color(targetPiece) == friendly) break;
                    Move move = new(index, target);
                    moves.Add(move);
                    if (targetPiece != Piece.none) break;
                }
            }
        }

        void GenerateKnightMoves()
        {
            List<int> pieces = (board.whiteToMove) ? board.whiteKnights : board.blackKnights;
            int friendly = (board.whiteToMove) ? Piece.white : Piece.black;
            foreach (int index in pieces)
            {
                if (pins.ContainsKey(index)) continue;
                foreach (int target in knightMoves[index])
                {
                    if (Piece.Color(board[target]) != friendly)
                    {
                        Move move = new(index, target);
                        moves.Add(move);
                    }
                }
            }
        }

        void GeneratePawnMoves()
        {
            List<int> pieces;
            int enemy;
            int dir;
            int startRow;
            if(board.whiteToMove)
            {
                pieces = board.whitePawns;
                enemy = Piece.black;
                dir = -1;
                startRow = 1;
            }
            else
            {
                pieces = board.blackPawns;
                enemy = Piece.white;
                dir = 1;
                startRow = 6;
            }

            foreach (int index in pieces)
            {
                int target = index + 8*dir;
                if (!pins.ContainsKey(index) || pins[index] < 2)
                {
                    if (board[target] == Piece.none)
                    {
                        AddPawnMove(index, target);

                        if (Coord.Row(index) == startRow)
                        {
                            if (board[target+8*dir] == Piece.none)
                            {
                                Move move = new(index, target + 8 * dir, Move.Flag.doubleMove);
                                moves.Add(move);
                            }
                        }
                    }
                }

                foreach (int x in pawnCaptures)
                {
                    int capTarget = target + x;
                    if (pins.ContainsKey(index) && (Math.Abs(directions[pins[index]]) != Math.Abs(capTarget - index))) continue;
                    if (capTarget < 0 || capTarget > 63) continue;
                    if (Coord.KingDistance(index, capTarget) > 1) continue;
                    if (Piece.Color(board[capTarget]) == enemy)
                    {
                        AddPawnMove(index, capTarget);
                    }
                    else if (capTarget == board.enPassantTarget && board[capTarget] == Piece.none)
                    {
                        if (epPin == index) continue;
                        Move move = new(index, capTarget, Move.Flag.enPassant);
                        moves.Add(move);
                    }
                }
            }
        }

        void AddPawnMove(int index, int target)
        {
            if(Coord.Row(target) == 0 || Coord.Row(target) == 7)
            {
                for(int flag = 4; flag < 8; flag++)
                {
                    Move move = new(index, target, flag);
                    moves.Add(move);
                }
            }
            else
            {
                Move move = new(index, target);
                moves.Add(move);
            }
        }

        void GenerateKingMoves()
        {
            int index = (board.whiteToMove) ? board.whiteKing : board.blackKing;
            int friendly = (board.whiteToMove) ? Piece.white : Piece.black;
            foreach (int dir in kingVectors)
            {
                int target = index + dir;
                if (target < 0 || target > 63 || Coord.KingDistance(index, target) > 1 || threat.Contains(target)) continue;
                int targetPiece = board[target];
                if (Piece.Color(targetPiece) == friendly) continue;
                Move move = new(index, target);
                moves.Add(move);
            }

            if (KingInCheck) return;
            if((board.whiteToMove && board.whiteCanCastleKing) || (!board.whiteToMove && board.blackCanCastleKing))
            {
                int target = index + 2;
                if (board[target - 1] == 0 && board[target] == 0 && !threat.Contains(target - 1) && !threat.Contains(target))
                {
                    Move move = new(index, target, Move.Flag.castling);
                    moves.Add(move);
                }
            }

            if ((board.whiteToMove && board.whiteCanCastleQueen) || (!board.whiteToMove && board.blackCanCastleQueen))
            {
                int target = index - 2;
                if (board[target + 1] == 0 && board[target] == 0 && board[target - 1] == 0 && !threat.Contains(target) && !threat.Contains(target + 1))
                {
                    Move move = new(index, target, Move.Flag.castling);
                    moves.Add(move);
                }
            }
        }

        void ExcludePseudoLegalMoves()
        {
            if(KingInCheck)
            {
                List<Move> legalMoves = new();
                foreach (Move move in moves)
                {
                    if (blockCheckSquares.Contains(move.Target)) legalMoves.Add(move);
                    else if(blockCheckSquares.Count == 1 && move.MoveFlag == Move.Flag.enPassant)
                    {
                        int pawn = move.Target;
                        pawn += (board.whiteToMove) ? 8 : -8;
                        if(blockCheckSquares.Contains(pawn)) legalMoves.Add(move);
                    }
                }
                moves = legalMoves;
            }
        }

        public MoveGenerator(Board board)
        {
            this.board = board;
        }

        public MoveGenerator() {}

        static MoveGenerator()
        {
            int[] knightVectors = { -15, -6, 10, 17, 15, 6, -10, -17 };

            squaresToEdge = new int[64][];
            knightMoves = new byte[64][];
            for(int row = 0; row < 8; row++)
            {
                for(int col = 0; col < 8; col++)
                {
                    int index = 8 * row + col;

                    int up = row;
                    int down = 7 - row;
                    int left = col;
                    int right = 7 - col;

                    squaresToEdge[index] = new int[8]{
                        up,
                        down,
                        left,
                        right,
                        Math.Min(up, left),
                        Math.Min(down, right),
                        Math.Min(up, right),
                        Math.Min(down, left)
                    };
                    
                    List<byte> knightMovesForSquare = new();
                    foreach(int v in knightVectors)
                    {
                        int target = index + v;
                        int xDist = Math.Abs(Coord.Col(index) - Coord.Col(target));
                        int yDist = Math.Abs(Coord.Row(index) - Coord.Row(target));
                        if (xDist < 3 && yDist < 3) knightMovesForSquare.Add((byte)target);
                    }
                    knightMoves[index] = knightMovesForSquare.ToArray();
                }
            }
        }

        void DebugPrintMoves()
        {
            Debug.Log("Moves:");
            foreach (Move move in moves)
            {
                Debug.Log(Coord.TileString(move.Start) + " => " + Coord.TileString(move.Target));
            }
        }


        //Threat generation (squares the enemy can attack)
        HashSet<int> GenerateThreat()
        {
            threat = new HashSet<int>();
            pins = new Dictionary<int, int>();
            epPin = -1;
            blockCheckSquares = new HashSet<int>();
            KingInCheck = false;
            DoubleCheck = false;
            GenerateSlidingThreat();
            GenerateKnightThreat();
            GeneratePawnThreat();
            GenerateKingThreat();
            return threat;
        }

        void GenerateSlidingThreat()
        {
            List<int> queens = (!board.whiteToMove) ? board.whiteQueens : board.blackQueens;
            List<int> rooks = (!board.whiteToMove) ? board.whiteRooks : board.blackRooks;
            List<int> bishops = (!board.whiteToMove) ? board.whiteBishops : board.blackBishops;

            foreach (int index in queens)
            {
                int piece = board[index];
                GenerateSlidingPieceThreat(0, 8, index);
            }
            foreach (int index in rooks)
            {
                int piece = board[index];
                GenerateSlidingPieceThreat(0, 4, index);
            }
            foreach (int index in bishops)
            {
                int piece = board[index];
                GenerateSlidingPieceThreat(4, 8, index);
            }
        }

        void GenerateSlidingPieceThreat(int startIndex, int endIndex, int index)
        {
            int friendly = (board.whiteToMove) ? Piece.white : Piece.black;
            int enemy = (!board.whiteToMove) ? Piece.white : Piece.black;
            int king = (board.whiteToMove) ? board.whiteKing : board.blackKing;
            for (int dir = startIndex; dir < endIndex; dir++)
            {
                int pinned = -1;
                HashSet<int> passedSquares = new() { index };
                for (int dist = 1; dist <= squaresToEdge[index][dir]; dist++)
                {
                    int target = index + dist * directions[dir];
                    int targetPiece = board[target];
                    if (Piece.Color(targetPiece) == friendly)
                    {
                        if(pinned == -1)
                        {
                            if (target == king)
                            {
                                if(!KingInCheck)
                                {
                                    KingInCheck = true;
                                    threat.Add(target);
                                    blockCheckSquares = passedSquares;
                                }
                                else
                                {
                                    DoubleCheck = true;
                                }
                                for (int d = dist + 1; d <= squaresToEdge[index][dir]; d++)
                                {
                                    target = index + d * directions[dir];
                                    threat.Add(target);
                                    if (board[target] != 0) break;
                                }
                                break;
                            }
                            else
                            {
                                pinned = target;
                                threat.Add(target);
                            }
                        }
                        else
                        {
                            if (target == king)
                            {
                                pins[pinned] = dir;
                            }
                            break;
                        }
                    }
                    passedSquares.Add(target);
                    if (pinned == -1) threat.Add(target);
                    if(Piece.Color(targetPiece) == enemy)
                    {
                        if(Piece.Type(targetPiece) == Piece.pawn && (target - 8 == board.enPassantTarget || target + 8 == board.enPassantTarget))
                        {
                            int targetPawn;
                            if (pinned == -1)
                            {
                                targetPawn = target + directions[dir];
                                dist++;
                            }
                            else
                            {
                                targetPawn = target - directions[dir];
                            }
                            if (Piece.Type(board[targetPawn]) != Piece.pawn || Piece.Color(board[targetPawn]) != friendly) break;
                            for (int d = dist+1; d <= squaresToEdge[index][dir]; d++)
                            {
                                target = index + d * directions[dir];
                                targetPiece = board[target];
                                if(targetPiece != 0)
                                {
                                    if (target == king) epPin = targetPawn;
                                    else break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        void GenerateKnightThreat()
        {
            List<int> pieces = (!board.whiteToMove) ? board.whiteKnights : board.blackKnights;
            int king = (board.whiteToMove) ? board.whiteKing : board.blackKing;
            foreach (int index in pieces)
            {
                foreach (int target in knightMoves[index])
                {
                    threat.Add(target);
                    if (target == king)
                    {
                        if(KingInCheck)
                        {
                            DoubleCheck = true;
                        }
                        else
                        {
                            blockCheckSquares = new() { index };
                            KingInCheck = true;
                        }
                    }
                }
            }
        }

        void GeneratePawnThreat()
        {
            List<int> pieces;
            int dir;
            int king = (board.whiteToMove) ? board.whiteKing : board.blackKing;
            if (!board.whiteToMove)
            {
                pieces = board.whitePawns;
                dir = -1;
            }
            else
            {
                pieces = board.blackPawns;
                dir = 1;
            }

            foreach (int index in pieces)
            {
                int target = index + 8 * dir;
                foreach (int x in pawnCaptures)
                {
                    int capTarget = target + x;
                    if (capTarget < 0 || capTarget > 63) continue;
                    if (Coord.KingDistance(index, capTarget) > 1) continue;
                    threat.Add(capTarget);
                    if (capTarget == king)
                    {
                        if (KingInCheck)
                        {
                            DoubleCheck = true;
                        }
                        else
                        {
                            blockCheckSquares = new HashSet<int> { index };
                            KingInCheck = true;
                        }
                    }
                }
            }
        }

        void GenerateKingThreat()
        {
            int index = (!board.whiteToMove) ? board.whiteKing : board.blackKing;
            foreach (int dir in kingVectors)
            {
                int target = index + dir;
                if (target < 0 || target > 63 || Coord.KingDistance(index, target) > 1) continue;
                threat.Add(target);
            }
        }
    }
}