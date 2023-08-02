using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
{
    public class Board
    {
        readonly int[] board;

        public bool whiteToMove = false;

        public bool whiteCanCastleKing = false;
        public bool whiteCanCastleQueen = false;
        public bool blackCanCastleKing = false;
        public bool blackCanCastleQueen = false;

        public int enPassantTarget = -1;
        public int halfMovesSinceCaptureOrPawnMove = 0;
        public int fullMoves = 0;

        public List<int> whiteSlidingPieces;
        public List<int> whitePawns;
        public List<int> whiteKnights;
        public List<int> blackSlidingPieces;
        public List<int> blackPawns;
        public List<int> blackKnights;

        public int whiteKing = -1;
        public int blackKing = -1;

        //bits 0-3 - castling rights; bits 4-9 - EP target (1-64, 0 = no target); bits 10-16 - 50 move clock; bits 17-22 - captured piece
        public uint currentGameState;
        Stack<uint> stateHistory;

        public Board(string fen)
        {
            board = new int[64];

            whiteSlidingPieces = new List<int>();
            whitePawns = new List<int>();
            whiteKnights = new List<int>();
            blackSlidingPieces = new List<int>();
            blackPawns = new List<int>();
            blackKnights = new List<int>();

            BoardFromFEN(fen);
        }

        public void MakeMove(Move move)
        {
            uint oldcastling = currentGameState & 0b1111;
            currentGameState = 0;   

            //Count half-moves for fifty move rule
            if (board[move.Target] != 0 || Pieces.Type(board[move.Start]) == Pieces.pawn || move.MoveFlag == Move.Flag.enPassant)
            {
                halfMovesSinceCaptureOrPawnMove = 0;
            }
            else
            {
                halfMovesSinceCaptureOrPawnMove++;
            }
            currentGameState |= (uint)halfMovesSinceCaptureOrPawnMove << 10;

            //Remove taken piece from lists
            if (move.MoveFlag != Move.Flag.enPassant)
            {
                currentGameState |= (uint)board[move.Target] << 17;
                RemovePiece(move.Target);
            }
            else
            {
                int cappedPawn = (whiteToMove) ? move.Target + 8 : move.Target - 8;
                currentGameState |= (uint)board[cappedPawn] << 17;
                RemovePiece(cappedPawn);
                board[cappedPawn] = 0;
            }

            //Move piece or move and promote pawn
            if (move.MoveFlag > 3)
            {
                int color = Pieces.Color(board[move.Start]);
                RemovePiece(move.Start);
                board[move.Start] = 0;

                board[move.Target] = (move.MoveFlag - 2) | color;
                AddPiece(move.Target);
            }
            else
            {
                MovePiece(move.Start, move.Target);
            }

            //If pawn double move set en passant target to passed square
            if (move.MoveFlag == Move.Flag.doubleMove)
            {
                int target = (whiteToMove) ? move.Target + 8 : move.Target - 8;
                enPassantTarget = target;
            }
            else
            {
                enPassantTarget = -1;
            }
            currentGameState |= (uint)(enPassantTarget + 1) << 4;

            //Handle castling
            if (move.MoveFlag == Move.Flag.castling)
            {
                if (whiteToMove)
                {
                    whiteCanCastleKing = false;
                    whiteCanCastleQueen = false;
                }
                else
                {
                    blackCanCastleKing = false;
                    blackCanCastleQueen = false;
                }

                if (move.Target > move.Start)
                {
                    int rook = move.Target + 1;
                    MovePiece(rook, move.Target - 1);
                }
                else
                {
                    int rook = move.Target - 2;
                    MovePiece(rook, move.Target + 1);
                }
            }
            else if (oldcastling != 0) {
                if (move.Target == whiteKing)
                {
                    whiteCanCastleKing = false;
                    whiteCanCastleQueen = false;
                }
                else if (move.Target == blackKing)
                {
                    blackCanCastleKing = false;
                    blackCanCastleQueen = false;
                }
                else
                {
                    switch (move.Start)
                    {
                        case 0:
                            blackCanCastleQueen = false;
                            break;
                        case 7:
                            blackCanCastleKing = false;
                            break;
                        case 56:
                            whiteCanCastleQueen = false;
                            break;
                        case 63:
                            whiteCanCastleKing = false;
                            break;
                    }

                    switch (move.Target)
                    {
                        case 0:
                            blackCanCastleQueen = false;
                            break;
                        case 7:
                            blackCanCastleKing = false;
                            break;
                        case 56:
                            whiteCanCastleQueen = false;
                            break;
                        case 63:
                            whiteCanCastleKing = false;
                            break;
                    }
                }
            }

            uint castling = 0;
            if (whiteCanCastleKing) castling |= 0b1;
            if (whiteCanCastleQueen) castling |= 0b10;
            if (blackCanCastleKing) castling |= 0b100;
            if (blackCanCastleQueen) castling |= 0b1000;
            currentGameState |= castling;

            //Increment total game moves and change player to move
            if (!whiteToMove) fullMoves++;
            whiteToMove = !whiteToMove;

            stateHistory.Push(currentGameState);
        }

        public void UnMakeMove(Move move)
        {
            whiteToMove = !whiteToMove;
            int color = (whiteToMove) ? Pieces.white : Pieces.black;
            if (!whiteToMove) fullMoves--;

            if(move.MoveFlag > 3) //if move was a promotion, undo promotion and move pawn back
            {
                board[move.Start] = Pieces.pawn | color;
                AddPiece(move.Start);

                RemovePiece(move.Target);
                board[move.Target] = 0;
            }
            else //otherwise move piece
            {
                MovePiece(move.Target, move.Start);
            }

            //readd captured piece
            int cappedPiece = (int)(currentGameState >> 17);
            if (cappedPiece != 0)
            {
                if (move.MoveFlag != Move.Flag.enPassant)
                {
                    board[move.Target] = cappedPiece;
                    AddPiece(move.Target);
                }
                else
                {
                    int target = (whiteToMove) ? move.Target + 8 : move.Target - 8;
                    board[target] = cappedPiece;
                    AddPiece(target);
                }
            }

            //move rook if move was a castle
            if(move.MoveFlag == Move.Flag.castling)
            {
                if (move.Target > move.Start)
                {
                    int rook = move.Target - 1;
                    MovePiece(rook, move.Target + 1);
                }
                else
                {
                    int rook = move.Target + 1;
                    MovePiece(rook, move.Target - 2);
                }
            }

            //get old game state from stack and get data from it
            stateHistory.Pop();
            currentGameState = stateHistory.Peek();

            if ((currentGameState & 0b1) != 0) whiteCanCastleKing = true;
            if ((currentGameState & 0b10) != 0) whiteCanCastleQueen = true;
            if ((currentGameState & 0b100) != 0) blackCanCastleKing = true;
            if ((currentGameState & 0b1000) != 0) blackCanCastleQueen = true;

            enPassantTarget = (int)((currentGameState & 0b1111110000) >> 4) - 1;

            halfMovesSinceCaptureOrPawnMove = (int)((currentGameState & 0b11111110000000000) >> 10);
        }

        void MovePiece(int from, int to)
        {
            switch (board[from])
            {
                case Pieces.queen | Pieces.white:
                case Pieces.bishop | Pieces.white:
                case Pieces.rook | Pieces.white:
                    whiteSlidingPieces.Remove(from);
                    whiteSlidingPieces.Add(to);
                    break;
                case Pieces.knight | Pieces.white:
                    whiteKnights.Remove(from);
                    whiteKnights.Add(to);
                    break;
                case Pieces.pawn | Pieces.white:
                    whitePawns.Remove(from);
                    whitePawns.Add(to);
                    break;
                case Pieces.king | Pieces.white:
                    whiteKing = to;
                    break;
                case Pieces.queen | Pieces.black:
                case Pieces.bishop | Pieces.black:
                case Pieces.rook | Pieces.black:
                    blackSlidingPieces.Remove(from);
                    blackSlidingPieces.Add(to);
                    break;
                case Pieces.knight | Pieces.black:
                    blackKnights.Remove(from);
                    blackKnights.Add(to);
                    break;
                case Pieces.pawn | Pieces.black:
                    blackPawns.Remove(from);
                    blackPawns.Add(to);
                    break;
                case Pieces.king | Pieces.black:
                    blackKing = to;
                    break;
            }

            board[to] = board[from];
            board[from] = 0;
        }

        void RemovePiece(int index)
        {
            if (board[index] == 0) return;
            switch(board[index])
            {
                case Pieces.queen | Pieces.white:
                case Pieces.bishop | Pieces.white:
                case Pieces.rook | Pieces.white:
                    whiteSlidingPieces.Remove(index);
                    return;
                case Pieces.knight | Pieces.white:
                    whiteKnights.Remove(index);
                    return;
                case Pieces.pawn | Pieces.white:
                    whitePawns.Remove(index);
                    return;
                case Pieces.queen | Pieces.black:
                case Pieces.bishop | Pieces.black:
                case Pieces.rook | Pieces.black:
                    blackSlidingPieces.Remove(index);
                    return;
                case Pieces.knight | Pieces.black:
                    blackKnights.Remove(index);
                    return;
                case Pieces.pawn | Pieces.black:
                    blackPawns.Remove(index);
                    return;
            }
        }

        void AddPiece(int index)
        {
            if (board[index] == 0) return;
            switch (board[index])
            {
                case Pieces.queen | Pieces.white:
                case Pieces.bishop | Pieces.white:
                case Pieces.rook | Pieces.white:
                    whiteSlidingPieces.Add(index);
                    return;
                case Pieces.knight | Pieces.white:
                    whiteKnights.Add(index);
                    return;
                case Pieces.pawn | Pieces.white:
                    whitePawns.Add(index);
                    return;
                case Pieces.queen | Pieces.black:
                case Pieces.bishop | Pieces.black:
                case Pieces.rook | Pieces.black:
                    blackSlidingPieces.Add(index);
                    return;
                case Pieces.knight | Pieces.black:
                    blackKnights.Add(index);
                    return;
                case Pieces.pawn | Pieces.black:
                    blackPawns.Add(index);
                    return;
            }
        }

        void BoardFromFEN(string fen)
        {
            stateHistory = new Stack<uint>();
            int row = 0;
            int col = 0;

            string[] fields = fen.Split(' ');
            currentGameState = 0;

            foreach (char c in fields[0])
            {
                if (c == '/')
                {
                    col = 0;
                    row++;
                }
                else if (c - '0' < 10)
                {
                    col += c - '0';
                }
                else
                {
                    int coord = Coord.Combine(row, col);
                    switch (c)
                    {
                        case 'P':
                            board[coord] = Pieces.pawn | Pieces.white;
                            whitePawns.Add(coord);
                            break;
                        case 'K':
                            board[coord] = Pieces.king | Pieces.white;
                            whiteKing = coord;
                            break;
                        case 'N':
                            board[coord] = Pieces.knight | Pieces.white;
                            whiteKnights.Add(coord);
                            break;
                        case 'B':
                            board[coord] = Pieces.bishop | Pieces.white;
                            whiteSlidingPieces.Add(coord);
                            break;
                        case 'R':
                            board[coord] = Pieces.rook | Pieces.white;
                            whiteSlidingPieces.Add(coord);
                            break;
                        case 'Q':
                            board[coord] = Pieces.queen | Pieces.white;
                            whiteSlidingPieces.Add(coord);
                            break;
                        case 'p':
                            board[coord] = Pieces.pawn | Pieces.black;
                            blackPawns.Add(coord);
                            break;
                        case 'k':
                            board[coord] = Pieces.king | Pieces.black;
                            blackKing = coord;
                            break;
                        case 'n':
                            board[coord] = Pieces.knight | Pieces.black;
                            blackKnights.Add(coord);
                            break;
                        case 'b':
                            board[coord] = Pieces.bishop | Pieces.black;
                            blackSlidingPieces.Add(coord);
                            break;
                        case 'r':
                            board[coord] = Pieces.rook | Pieces.black;
                            blackSlidingPieces.Add(coord);
                            break;
                        case 'q':
                            board[coord] = Pieces.queen | Pieces.black;
                            blackSlidingPieces.Add(coord);
                            break;
                    }
                    col++;
                }
            }

            if (fields[1] == "w") whiteToMove = true;

            foreach (char c in fields[2])
            {
                switch (c)
                {
                    case 'K':
                        whiteCanCastleKing = true;
                        currentGameState |= 0b1;
                        break;
                    case 'Q':
                        whiteCanCastleQueen = true;
                        currentGameState |= 0b10;
                        break;
                    case 'k':
                        blackCanCastleKing = true;
                        currentGameState |= 0b100;
                        break;
                    case 'q':
                        blackCanCastleQueen = true;
                        currentGameState |= 0b1000;
                        break;
                }
            }


            if (fields[3] != "-") enPassantTarget = Coord.CoordFromString(fields[3]);
            else enPassantTarget = -1;

            if (fields.Length > 4)
                halfMovesSinceCaptureOrPawnMove = Convert.ToInt32(fields[4]);
            else halfMovesSinceCaptureOrPawnMove = 0;

            if(fields.Length > 5)
                fullMoves = Convert.ToInt32(fields[5]);
            else fullMoves = 1;

            currentGameState |= (uint)halfMovesSinceCaptureOrPawnMove << 10;
            currentGameState |= (uint)(enPassantTarget + 1) << 4;

            stateHistory.Push(currentGameState);
        }

        public int this[int index]
        {
            get
            {
                return board[index];
            }
            set
            {
                board[index] = value;
            }
        }
        public void DebugDisplayBoard()
        {
            string log = "\n";
            for (int row = 0; row < 8; row++)
            {
                for(int col = 0; col < 8; col++)
                {
                    int index = Coord.Combine(row, col);
                    log += Pieces.PieceChar(board[index]);
                }
                log += '\n';
            }
            Debug.Log(log);
        }
    }
}


