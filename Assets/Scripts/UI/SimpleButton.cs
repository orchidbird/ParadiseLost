using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler{
	Image image;
	public Sprite defaultSprite;
	public Sprite onDownSprite;
	public UnityEvent OnPointerDown;

	void Start(){
		image = GetComponent<Image>();
	}
	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		OnPointerDown.Invoke();
		image.sprite = onDownSprite;
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData){
		image.sprite = defaultSprite;
	}
}
