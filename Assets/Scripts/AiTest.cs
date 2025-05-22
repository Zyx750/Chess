using System.Collections;
using System.Collections.Generic;
using Chess;
using Chess.AI;
using Chess.Game;
using Chess.UI;
using UnityEngine;

public class AiTest : MonoBehaviour
{
    ChessAI bot1;
    ChessAI bot2;
    BoardUI boardUI;
    public GameObject boardUIObject;
    Board board;
    MoveGenerator generator;
    public int AiTimeMs1 = 1000;
    public int AiTimeMs2 = 1000;
    public int numberOfGames = 10;
    int gameResult;
    void Start()
    {
        bot1 = gameObject.AddComponent(typeof(ChessAI)) as ChessAI;
        bot2 = gameObject.AddComponent(typeof(ChessAI)) as ChessAI;
        generator = new MoveGenerator();

        boardUI = boardUIObject.GetComponent<BoardUI>();

        StartCoroutine(RunGames());
    }

    IEnumerator RunGames()
    {

        int Ai1Wins = 0, Ai2Wins = 0, draws = 0;

        for (int i = 0; i < numberOfGames; i++)
        {
            int whiteTime = (i % 2 == 0) ? AiTimeMs1 : AiTimeMs2;
            int blackTime = (i % 2 == 0) ? AiTimeMs2 : AiTimeMs1;
            Debug.Log($"Game {i + 1}: White AI time: {whiteTime}ms, Black AI time: {blackTime}ms");
            int result = int.MinValue;

            yield return StartCoroutine(PlayGameCoroutine(whiteTime, blackTime, r => result = r));

            int winner = result * (i % 2 == 0 ? 1 : -1);

            if (winner == 1) Ai1Wins++;
            else if (winner == -1) Ai2Wins++;
            else draws++;

            Debug.Log($"Game {i + 1}: {result}");
        }
        Debug.Log($"AI1: {Ai1Wins}, AI2: {Ai2Wins}, Draws: {draws}");
    }

    IEnumerator PlayGameCoroutine(int whiteAiTimeMs, int blackAiTimeMs, System.Action<int> onGameEnd = null)
    {
        board = new Board();
        boardUI.DrawBoard();
        boardUI.DrawPieces(board);

        gameResult = int.MinValue;

        bot1.Init(board, MakeMove, whiteAiTimeMs);
        bot2.Init(board, MakeMove, blackAiTimeMs);

        bot1.ChooseMove();

        while (gameResult == int.MinValue)
            yield return null;

        onGameEnd?.Invoke(gameResult);
    }

    public int PlayGame(int whiteAiTimeMs, int blackAiTimeMs)
    {
        StartCoroutine(PlayGameCoroutine(whiteAiTimeMs, blackAiTimeMs));
        return gameResult; // Will not be correct immediately, result comes asynchronously
    }

    void EndGame(int result)
    {
        gameResult = result;
    }

    public void MakeMove(Move move)
    {
        board.MakeMove(move);
        boardUI.RedrawPieces(board);
        if (board.draw)
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
        PrepareForMove();
    }

    void PrepareForMove()
    {
        if (board.whiteToMove)
        {
            bot1.ChooseMove();
        }
        else
        {
            bot2.ChooseMove();
        }
    }

    void DrawBoard()
    {
        Board board = new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        boardUI = boardUIObject.GetComponent<BoardUI>();
        boardUI.DrawBoard();
        boardUI.DrawPieces(board);
    }
}