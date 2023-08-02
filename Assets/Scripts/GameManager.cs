using Chess.AI;
using Chess.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
{
    public class GameManager : MonoBehaviour
    {
        Board board;
        public GameObject boardUIObject;
        BoardUI boardUI;
        MoveGenerator generator;

        public bool whitePlayerHuman;
        public bool blackPlayerHuman;

        ChessAI bot;

        Stack<Move> moveHistory;

        void Start()
        {
            //board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            board = new Board("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/P1N2Q1p/1PPBBPPP/R3K2R b KQkq - 0 1");
            generator = new MoveGenerator();
            moveHistory = new Stack<Move>();

            boardUI = boardUIObject.GetComponent<BoardUI>();
            boardUI.DrawBoard();
            boardUI.DrawPieces(board);

            if(!whitePlayerHuman || !blackPlayerHuman) bot = new ChessAI();

            List<Move> moves = generator.GenerateMoves(board);
            PrepareForMove(moves);
        }
        
        public void MakeMove(Move move)
        {
            board.MakeMove(move);
            moveHistory.Push(move);
            if (board.halfMovesSinceCaptureOrPawnMove >= 100)
            {
                EndGame(0);
                return;
            }
            List<Move> moves = generator.GenerateMoves(board);
            if (moves.Count == 0)
            {
                if (generator.KingInCheck) EndGame((board.whiteToMove) ? -1 : 1);
                else EndGame(0);
                return;
            }
            if ((!board.whiteToMove && !whitePlayerHuman) || (board.whiteToMove && !blackPlayerHuman))
            {
                boardUI.RedrawPieces(board);
            }
            board.DebugDisplayBoard();
            PrepareForMove(moves);
        }

        void PrepareForMove(List<Move> moves)
        {
            boardUI.GetPossibleMoves(moves);
            boardUI.DisableOrEnableRaycasts(board.whiteToMove && whitePlayerHuman, !board.whiteToMove && blackPlayerHuman);
            //boardUI.HighlightThreatSquares(generator.threat);

            if((board.whiteToMove && !whitePlayerHuman) || (!board.whiteToMove && !blackPlayerHuman))
            {
                Move move = bot.ChooseMove(board);
                MakeMove(move);
            }
        }

        void EndGame(int result) //-1 = black win, 0 = draw, 1 = white win
        {
            boardUI.DisableOrEnableRaycasts(false, false);
            Debug.Log("result = " + result);
        }

        [ContextMenu("Draw Board")]
        void DrawBoard()
        {
            Board board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            boardUI = boardUIObject.GetComponent<BoardUI>();
            boardUI.DrawBoard();
            boardUI.DrawPieces(board);
        }

        [ContextMenu("Undo Last Move")]
        void UndoMove()
        {
            Move move = moveHistory.Pop();
            board.UnMakeMove(move);
            board.DebugDisplayBoard();
            boardUI.RedrawPieces(board);
            List<Move> moves = generator.GenerateMoves(board);
            PrepareForMove(moves);
        }
    }
}


