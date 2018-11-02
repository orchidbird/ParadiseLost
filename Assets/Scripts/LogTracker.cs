using UnityEngine;
using UnityEngine.SceneManagement;

public class LogTracker : MonoBehaviour {
	private void OnEnable(){
		Debug.Log(gameObject.name + " Enabled(" + SceneManager.GetActiveScene().name + ")");
	}

	private void OnDisable(){
		Debug.Log(gameObject.name + " Disabled(" + SceneManager.GetActiveScene().name + ")");
	}
}
