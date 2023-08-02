using Chess.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess.UI
{
    public class Tile : MonoBehaviour, IDropHandler
    {
        public int index;
        public GameObject piece;

        public void OnDrop(PointerEventData eventData)
        {
            BoardUI boardUI = transform.parent.GetComponent<BoardUI>();
            GameObject piece = eventData.pointerDrag;
            if (!piece.GetComponent<Piece>().IsTarget(index)) return;
            if (this.piece != null )
            {
                if(Pieces.Color(this.piece.GetComponent<Piece>().piece) == Pieces.white)
                {
                    boardUI.whitePieces.Remove(this.piece);
                }
                else
                {
                    boardUI.blackPieces.Remove(this.piece);
                }
                Destroy(this.piece);
            }
            piece.GetComponent<Piece>().tile = gameObject;
            this.piece = piece;
        }
    }
}