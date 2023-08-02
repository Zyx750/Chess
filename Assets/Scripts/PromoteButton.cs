using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess.UI
{
    public class PromoteButton : MonoBehaviour, IPointerClickHandler
    {
        public int flag;

        public void OnPointerClick(PointerEventData eventData)
        {
            PromoteSelect selector = transform.parent.GetComponent<PromoteSelect>();
            selector.AddFlag(flag);
        }
    }
}