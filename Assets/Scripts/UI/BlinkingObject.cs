using UnityEngine;

public class BlinkingObject : MonoBehaviour{
	float duration;
	const float timeInterval = 0.3f;
	public GameObject showingObject;
	void Start(){
		duration = timeInterval;
	}

	void Update(){
		duration -= Time.deltaTime;
		if (duration >= 0) return;
		showingObject.SetActive(!showingObject.activeSelf);
		duration = timeInterval;
	}
}
