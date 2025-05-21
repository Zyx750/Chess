using System.Collections;
using System.Collections.Generic;
using Chess;
using Chess.AI;
using Chess.Game;
using UnityEngine;

public class AiTest : MonoBehaviour
{
    ChessAI bot;
    Board board;
    MoveGenerator generator;
    int searchTimeWhite = 1000;
    int searchTimeBlack = 1000;
    int gameResult;
    void Start()
    {
        bot = gameObject.AddComponent(typeof(ChessAI)) as ChessAI;
        generator = new MoveGenerator();

        StartCoroutine(RunGames());
    }

    IEnumerator RunGames()
    {
        int AiTimeMs1 = 100;
        int AiTimeMs2 = 1000;
        int Ai1Wins = 0, Ai2Wins = 0, draws = 0;

        for (int i = 0; i < 10; i++)
        {
            int whiteTime = (i % 2 == 0) ? AiTimeMs1 : AiTimeMs2;
            int blackTime = (i % 2 == 0) ? AiTimeMs2 : AiTimeMs1;
            int result = int.MinValue;

            yield return StartCoroutine(PlayGameCoroutine(whiteTime, blackTime, r => result = r));

            if (i % 2 == 1) result *= -1;

            if (result == 1) Ai1Wins++;
            else if (result == -1) Ai2Wins++;
            else draws++;

            Debug.Log($"Game {i + 1}: {result}");
        }
        Debug.Log($"AI1: {Ai1Wins}, AI2: {Ai2Wins}, Draws: {draws}");
    }

    IEnumerator PlayGameCoroutine(int whiteAiTimeMs, int blackAiTimeMs, System.Action<int> onGameEnd = null)
    {
        board = new Board();
        gameResult = int.MinValue;

        bot.Init(board, MakeMove, whiteAiTimeMs);
        searchTimeWhite = whiteAiTimeMs;
        searchTimeBlack = blackAiTimeMs;

        bot.ChooseMove();

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
            bot.searchTimeMillis = searchTimeWhite;
        }
        else
        {
            bot.searchTimeMillis = searchTimeBlack;
        }
        bot.ChooseMove();
    }
}