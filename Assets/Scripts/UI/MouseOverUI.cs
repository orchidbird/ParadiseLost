using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
	public GameObject HighlightMode;
	public GameObject NormalMode;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		HighlightMode.SetActive(true);
		NormalMode.SetActive(false);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		HighlightMode.SetActive(false);
		NormalMode.SetActive(true);
	}
}
