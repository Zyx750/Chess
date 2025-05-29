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
        private static bool nextWhitePlayerHuman = true;
        private static bool nextBlackPlayerHuman = false;

        public static float nextAiTimeout = 5;

        ChessAI bot;

        Stack<Move> moveHistory;

        [SerializeField]
        private InGameUI inGameUi;

        public static void PushNextSettings(bool whitePlayerHuman, bool blackPlayerHuman)
        {
            nextWhitePlayerHuman = whitePlayerHuman;
            nextBlackPlayerHuman = blackPlayerHuman;
        }

        void Start()
        {
            whitePlayerHuman = nextWhitePlayerHuman;
            blackPlayerHuman = nextBlackPlayerHuman;

            board = new Board();
            generator = new MoveGenerator();
            moveHistory = new Stack<Move>();

            boardUI = boardUIObject.GetComponent<BoardUI>();
            boardUI.DrawBoard();
            boardUI.DrawPieces(board);

            if (!whitePlayerHuman || !blackPlayerHuman)
            {
                bot = gameObject.AddComponent(typeof(ChessAI)) as ChessAI;
                bot.Init(board, MakeMove, (int)(nextAiTimeout * 1000));
            }

            List<Move> moves = generator.GenerateMoves(board);
            PrepareForMove(moves);
        }

        public void MakeMove(Move move)
        {
            board.MakeMove(move);
            moveHistory.Push(move);
            if ((!board.whiteToMove && !whitePlayerHuman) || (board.whiteToMove && !blackPlayerHuman))
            {
                boardUI.RedrawPieces(board);
            }
            if (board.draw)
            {
                EndGame(0);
                return;
            }
            List<Move> moves = generator.GenerateMoves(board);
            //boardUI.HighlightThreatSquares(generator.threat);
            if (moves.Count == 0)
            {
                board.DebugDisplayBoard();
                if (generator.KingInCheck) EndGame((board.whiteToMove) ? -1 : 1);
                else EndGame(0);
                return;
            }
            PrepareForMove(moves);
        }

        void PrepareForMove(List<Move> moves)
        {
            boardUI.GetPossibleMoves(moves);
            boardUI.DisableOrEnableRaycasts(board.whiteToMove && whitePlayerHuman, !board.whiteToMove && blackPlayerHuman);
            if (whitePlayerHuman && blackPlayerHuman)
            {
                boardUI.RotateView(board.whiteToMove && whitePlayerHuman || !blackPlayerHuman);
            }
            else
            {
                boardUI.RotateViewAndCamera(whitePlayerHuman);
            }

            if ((board.whiteToMove && !whitePlayerHuman) || (!board.whiteToMove && !blackPlayerHuman))
            {
                bot.ChooseMove(true);
            }
        }

        void EndGame(int result) //-1 = black win, 0 = draw, 1 = white win
        {
            boardUI.DisableOrEnableRaycasts(false, false);
            Debug.Log("result = " + result);

            StartCoroutine(ShowGameOverAfterDelay(result, 0.5f));
        }

        System.Collections.IEnumerator ShowGameOverAfterDelay(int result, float delay)
        {
            yield return new WaitForSeconds(delay);
            inGameUi.ShowGameOver(result);
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


