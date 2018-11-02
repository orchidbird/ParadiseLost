using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UtilityMethods;

public class TogglePosition : MonoBehaviour, IPointerDownHandler{
	public bool moving;
	public bool opened;
	
	void IPointerDownHandler.OnPointerDown(PointerEventData input){
		StartCoroutine(Move());
	}

	public IEnumerator Move(){
		if (moving) yield break;

		moving = true;
		var rect = GetComponent<RectTransform>();
		yield return UI.SlideRect(rect,
			rect.anchoredPosition3D + new Vector3(0, (rect.sizeDelta.y - 130) * (opened ? 1 : -1), 0), 0.5f);
		
		opened = !opened;
		moving = false;
	}
}
