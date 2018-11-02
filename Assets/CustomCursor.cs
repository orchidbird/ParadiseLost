using UnityEngine;

public class CustomCursor : MonoBehaviour{
	float clickedTime;
	
	void Update (){
		transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward;
		if (Input.GetMouseButton(0)){
			clickedTime += Time.deltaTime;
			if (clickedTime > Setting.longClickDurationThreshold)
				clickedTime = 0;
		}else if (Input.GetMouseButtonUp(0))
			clickedTime = 0;
		
		GetComponent<SpriteRenderer>().material.SetFloat("_Threshold", clickedTime / Setting.longClickDurationThreshold);
		transform.localScale = Vector3.one * Camera.main.orthographicSize / 7;
	}
}
