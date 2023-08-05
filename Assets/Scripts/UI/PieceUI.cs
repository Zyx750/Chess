using Chess.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chess.UI
{
    public class PieceUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public GameObject tile;
        public GameObject promoteSelect;
        public int piece;
        GameObject startTile;
        Vector2 startPos;
        Camera cam;
        List<Move> moveTargets;
        int startIndex;

        public void OnBeginDrag(PointerEventData eventData)
        {
            startTile = tile;
            startPos = transform.position;
            cam = Camera.main;
            GetComponent<Image>().raycastTarget = false;
            transform.SetSiblingIndex(transform.parent.childCount-1);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 pos = cam.ScreenToWorldPoint(eventData.position);
            transform.position = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GetComponent<Image>().raycastTarget = true;
            Move move = TargetMove(tile.GetComponent<Tile>().index);
            if (move != null)
            {
                startTile.GetComponent<Tile>().piece = null;
                transform.position = tile.transform.position;
                MakeMove(move);
            }
            else
            {
                transform.position = startPos;
            }
        }

        public Move TargetMove(int index)
        {
            foreach(Move move in moveTargets)
            {
                if (move.Target == index) return move;
            }
            return null;
        }

        public bool IsTarget(int index)
        {
            foreach (Move move in moveTargets)
            {
                if (move.Target == index) return true;
            }
            return false;
        }

        public void MakeMove(Move move)
        {
            BoardUI boardUI = transform.parent.GetComponent<BoardUI>();
            if (move.MoveFlag == Move.Flag.enPassant)
            {
                int index = tile.GetComponent<Tile>().index;
                if (Coord.Row(move.Target) == 2) index -= 8;
                else index += 8;
                boardUI.DeletePiece(index);
            }
            else if(move.MoveFlag > 3)
            {
                Vector2 pos = (Coord.Row(move.Target) == 0) ? (Vector2)transform.position + new Vector2(0,1) : (Vector2)transform.position - new Vector2(0, 1);
                GameObject selector = Instantiate(promoteSelect, pos, Quaternion.identity, transform);
                selector.GetComponent<PromoteSelect>().ShowPieces(Coord.Row(move.Target) == 7, move);
                boardUI.DisableOrEnableRaycasts(false, false);
                return;
            }
            else if(move.MoveFlag == Move.Flag.castling)
            {
                if (move.Target > move.Start)
                {
                    int rook = move.Target + 1;
                    boardUI.MovePiece(rook, move.Target - 1);
                }
                else
                {
                    int rook = move.Target - 2;
                    boardUI.MovePiece(rook, move.Target + 1);
                }
            }
            GameObject manager = GameObject.Find("GameManager");
            manager.GetComponent<GameManager>().MakeMove(move);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            startIndex = tile.GetComponent<Tile>().index;
            BoardUI boardUI = transform.parent.GetComponent<BoardUI>();
            moveTargets = new List<Move>();
            if (boardUI.moveDict.ContainsKey(startIndex))
            {
                moveTargets = boardUI.moveDict[startIndex];
                boardUI.HighlightMoveSquares(startIndex);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            BoardUI boardUI = transform.parent.GetComponent<BoardUI>();
            boardUI.ResetSquareColors();
        }

        public void ChangeType(int type)
        {
            BoardUI boardUI = transform.parent.GetComponent<BoardUI>();

            piece = Piece.Color(piece) | type;
            Image image = GetComponent<Image>();

            int spriteID = Piece.Type(piece) - 1;
            if (!Piece.IsWhite(piece)) spriteID += 6;
            image.sprite = boardUI.pieces[spriteID];
        }
    }
}