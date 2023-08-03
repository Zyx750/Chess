using Chess.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.UI
{
    public class PromoteSelect : MonoBehaviour
    {
        public Sprite[] sprites;
        Move move;

        public void ShowPieces(bool white, Move move)
        {
            int spriteIndex = (white) ? 0 : 4;

            foreach(Transform piece in transform)
            {
                Image image = piece.GetComponent<Image>();
                image.sprite = sprites[spriteIndex];
                spriteIndex++;
            }

            this.move = move;
        }

        public void AddFlag(int flag)
        {
            Move moveWithFlag = new(move, flag);
            PieceUI piece = transform.parent.GetComponent<PieceUI>();
            piece.ChangeType(flag - 2);

            GameObject manager = GameObject.Find("GameManager");
            manager.GetComponent<GameManager>().MakeMove(moveWithFlag);

            Destroy(gameObject);
        }
    }
}