//this script allows the button to switch between two screens for the demos :)
//nothing fancy here

using UnityEngine;
using System.Collections;

public class GoToSceneScript : MonoBehaviour {

	public void GoToScene(int SceneNum)
	{
		Application.LoadLevel(SceneNum);
	}
}
