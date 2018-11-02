using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleImage : MonoBehaviour, IPointerDownHandler
{
	public Image image;
	void IPointerDownHandler.OnPointerDown(PointerEventData input)
	{
		image.color = image.color.a > 0 ?
			new Color(image.color.r, image.color.g, image.color.b, 0)
			: new Color(image.color.r, image.color.g, image.color.b, 111 / 255f);
	}
}
