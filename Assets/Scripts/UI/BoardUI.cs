using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class BoardUI : MonoBehaviour
    {
        public GameObject square;
        public Color whiteSquare;
        public Color blackSquare;
        public Color moveHighlightWhite;
        public Color moveHighlightBlack;
        public Color threatHighlightWhite;
        public Color threatHighlightBlack;
        public GameObject piecePrefab;
        public Sprite[] pieces;

        public GameObject[] tiles;
        public List<GameObject> whitePieces;
        public List<GameObject> blackPieces;

        public Dictionary<int, List<Move>> moveDict;


        public void DrawBoard()
        {
            tiles = new GameObject[64];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = Coord.Combine(7 - row, col);
                    Vector2 pos = new(col + 0.5f, 7 - row + 0.5f);
                    pos += (Vector2)transform.position;
                    tiles[index] = Instantiate(square, pos, Quaternion.identity, transform);
                    tiles[index].name = Coord.TileString(index);
                    Image image = tiles[index].GetComponent<Image>();
                    image.color = (Coord.IsWhite(index)) ? whiteSquare : blackSquare;
                    tiles[index].GetComponent<Tile>().index = index;
                }
            }
        }

        public void DrawPieces(Board board)
        {
            whitePieces = new List<GameObject>();
            blackPieces = new List<GameObject>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = Coord.Combine(7 - row, col);
                    int piece = board[index];
                    if(piece != 0)
                    {
                        Vector2 pos = new(col + 0.5f, 7 - row + 0.5f);
                        pos += (Vector2)transform.position;
                        GameObject newPiece = Instantiate(piecePrefab, pos, Quaternion.identity, transform);
                        newPiece.name = Piece.ColorString(piece) + Piece.TypeString(piece);

                        int spriteID = Piece.Type(piece) - 1;
                        if (!Piece.IsWhite(piece)) spriteID += 6;
                        newPiece.GetComponent<Image>().sprite = pieces[spriteID];

                        newPiece.GetComponent<PieceUI>().tile = tiles[index];
                        tiles[index].GetComponent<Tile>().piece = newPiece;
                        newPiece.GetComponent<PieceUI>().piece = piece;

                        if(Piece.Color(piece) == Piece.white)
                        {
                            whitePieces.Add(newPiece);
                        }
                        else
                        {
                            blackPieces.Add(newPiece);
                        }
                    }
                }
            }
        }

        public void RedrawPieces(Board board)
        {
            foreach(GameObject tile in tiles)
            {
                tile.GetComponent<Tile>().piece = null;
            }
            foreach (GameObject piece in whitePieces)
            {
                Destroy(piece);
            }
            foreach (GameObject piece in blackPieces)
            {
                Destroy(piece);
            }
            DrawPieces(board);
        }

        public void GetPossibleMoves(List<Move> moves)
        {
            moveDict = new Dictionary<int, List<Move>>();
            foreach(Move move in moves)
            {
                if(!moveDict.ContainsKey(move.Start))
                {
                    moveDict[move.Start] = new List<Move>();
                }
                moveDict[move.Start].Add(move);
            }
        }

        public void DisableOrEnableRaycasts(bool white, bool black)
        {
            foreach(GameObject piece in whitePieces)
            {
                piece.GetComponent<Image>().raycastTarget = white;
            }
            foreach(GameObject piece in blackPieces)
            {
                piece.GetComponent<Image>().raycastTarget = black;
            }
        }

        public void HighlightMoveSquares(int from)
        {
            foreach(Move move in moveDict[from])
            {
                Image image = tiles[move.Target].GetComponent<Image>();
                image.color = (Coord.IsWhite(move.Target)) ? moveHighlightWhite : moveHighlightBlack;
            }
        }

        public void HighlightThreatSquares(HashSet<int> threat)
        {
            ResetSquareColors();
            foreach (int square in threat)
            {
                Image image = tiles[square].GetComponent<Image>();
                image.color = (Coord.IsWhite(square)) ? threatHighlightWhite : threatHighlightBlack;
            }
        }

        public void ResetSquareColors()
        {
            for(int index = 0; index < 64; index++)
            {
                Image image = tiles[index].GetComponent<Image>();
                image.color = (Coord.IsWhite(index)) ? whiteSquare : blackSquare;
            }
        }

        public void DeletePiece(int index)
        {
            GameObject piece = tiles[index].GetComponent<Tile>().piece;
            tiles[index].GetComponent<Tile>().piece = null;
            if (Piece.Color(piece.GetComponent<PieceUI>().piece) == Piece.white)
            {
                whitePieces.Remove(piece);
            }
            else
            {
                blackPieces.Remove(piece);
            }
            Destroy(piece);
        }

        public void MovePiece(int from, int to)
        {
            GameObject piece = tiles[from].GetComponent<Tile>().piece;
            tiles[from].GetComponent<Tile>().piece = null;

            tiles[to].GetComponent<Tile>().piece = piece;
            piece.transform.position = tiles[to].transform.position;
            piece.GetComponent<PieceUI>().tile = tiles[to];
        }

        public void RotateView(bool white)
        {
            Camera camera = Camera.main;
            Quaternion rotation = new Quaternion(0, 0, (white) ? 0 : 180, 0);
            // camera.transform.rotation = rotation;

            foreach (GameObject piece in whitePieces)
            {
                piece.transform.rotation = rotation;
            }
            foreach (GameObject piece in blackPieces)
            {
                piece.transform.rotation = rotation;
            }
        }

        public void RotateViewAndCamera(bool white)
        {
            Camera camera = Camera.main;
            Quaternion rotation = new Quaternion(0, 0, (white) ? 0 : 180, 0);
            camera.transform.rotation = rotation;

            RotateView(white);
        }
    }
}
