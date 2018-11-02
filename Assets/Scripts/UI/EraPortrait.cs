using GameData;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EraPortrait : MonoBehaviour, IPointerDownHandler{
	public Image POV;
	public float duration;
	public bool interactable;

	public void Initialize(StageInfo stage){
		gameObject.name = stage.POV; 
		GetComponent<Image>().sprite = Resources.Load<Sprite>("Portrait/" + stage.POV);
	}

	void Update(){
		if(duration <= 0) return;
		
		var positionDifference = transform.parent.GetComponent<RectTransform>().position - GetComponent<RectTransform>().position;
		GetComponent<RectTransform>().Translate(positionDifference * Time.deltaTime / duration);
		duration -= Time.deltaTime;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData data){
		if(!interactable) return;
		FindObjectOfType<EraManager>().ElapseTo(StageInfo.stages.Find(chapter => chapter.POV == gameObject.name).stage);
	}
}
