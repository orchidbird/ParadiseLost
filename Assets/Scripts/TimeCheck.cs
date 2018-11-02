using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCheck : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Debug.Log("Start at : " + Time.time);
	}
	void OnDestroy(){
		Debug.Log("Destroy at : " + Time.time);
	}
}