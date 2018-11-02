using UnityEngine;

public class TimeStopper : MonoBehaviour {
	void OnEnable(){
		Time.timeScale = 0;
	}

	void OnDisable(){
		Time.timeScale = 1;
	}
}
