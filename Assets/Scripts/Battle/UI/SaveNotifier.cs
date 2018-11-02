using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveNotifier : MonoBehaviour {
	public GameObject Text;
	public float lifespan;
	public IEnumerator Notice(){
		Debug.Log("Notice Save.");
		Text.SetActive(true);
		yield return new WaitForSeconds(lifespan);
		Text.SetActive(false);
	}
}