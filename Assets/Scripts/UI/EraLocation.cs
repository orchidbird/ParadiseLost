using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EraLocation : MonoBehaviour{
	private float scaleX = 15;
	private float scaleY = 18;
	Vector2 totalPos = Vector2.zero;
	private List<EraPortrait> children;

	public IEnumerator PullChildren(float duration = 0){
		children = GetComponentsInChildren<EraPortrait>().ToList();
		if(children.Count != 0)
			Debug.Log(gameObject.name + "의 PullChildren(" + duration + "): " + children.Count);
		
		foreach (var child in children)
			if(child.interactable)
				child.duration = duration;
		
		if(duration > 0)
			yield return new WaitForSeconds(duration);
		
		var rects = children.ConvertAll(item => item.GetComponent<RectTransform>());
		totalPos = Vector2.zero;
		var pos = Vector2.zero;
		for (int i = 0; i < children.Count; i++){ 
			if (i == 1) pos = Vector2.right;
			else if (i == 2) pos = new Vector2(0.5f, 1);
			else if (i == 3) pos = new Vector2(-0.5f, 1);
			else if (i == 4) pos = new Vector2(-1, 0);
			else if (i == 5) pos = new Vector2(-0.5f, -1);
			else if (i == 6) pos = new Vector2(0.5f, -1);
			else if (i == 7) pos = new Vector2(1.5f, 1);
			else if (i == 8) pos = new Vector2(1, 2);
			else if (i == 9) pos = new Vector2(0, 2);
			else if (i == 10) pos = new Vector2(-1, 2);
			else if (i == 11) pos = new Vector2(-1.5f, 1);
			else if (i == 12) pos = new Vector2(-2, 0);
			else if (i == 13) pos = new Vector2(-1.5f, -1);
			else if (i == 14) pos = new Vector2(-1, -2);
			else if (i == 15) pos = new Vector2(0, -2);
			else if (i == 16) pos = new Vector2(1, -2);
			else if (i == 17) pos = new Vector2(1.5f, -1);
			else if (i == 18) pos = new Vector2(2, 0);
			
			else if (i == 19) pos = new Vector2(0, 3);

			pos.x *= scaleX;
			pos.y *= scaleY;
			rects[i].localPosition = pos;
			totalPos += pos;
		}

		var averagePos = totalPos / children.Count;
		foreach (var rect in rects){
			rect.Translate(-averagePos);
		}
	}
}
