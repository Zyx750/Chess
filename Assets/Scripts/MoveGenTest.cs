using Chess;
using Chess.Game;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenTest : MonoBehaviour
{
    Board board;
    MoveGenerator generator;
    readonly string[] positions =
    {
        "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
        "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -",
        "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -",
        "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1",
        "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8",
        "r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10"
    };
    void Start()
    {
        /*board = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/PpN2Q1p/1PPBBPPP/R3K2R w KQkq - 0 2");
        generator = new MoveGenerator(board);
        List<Move> moves = generator.GenerateMoves(board);
        int total = 0;
            
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int num = MoveGeneratorTest(2);
            board.UnMakeMove(move);
            Debug.Log(Move.MoveString(move) + ": " + num + " positions");
            total += num;
        }
        Debug.Log("Total: " + total);
        board.DebugDisplayBoard();*/

        for (int p = 0; p < positions.Length; p++)
        {
            Debug.Log("postition " + (p + 1) + ":");
            for (int i = 1; i < 5; i++)
            {
                board = new Board(positions[p]);
                generator = new MoveGenerator();
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                int num = MoveGeneratorTest(i);
                timer.Stop();
                Debug.Log("depth: " + i + "; total positions = " + num + "; time = " + timer.Elapsed);
            }
        }
    }

    int MoveGeneratorTest(int depth)
    {
        if (depth == 0 || board.draw) return 1;
        else if (depth == 1) return generator.GenerateMoves(board).Count;
        int numMoves = 0;

        List<Move> moves = generator.GenerateMoves(board);
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            numMoves += MoveGeneratorTest(depth - 1);
            board.UnMakeMove(move);
        }

        return numMoves;
    }
}
