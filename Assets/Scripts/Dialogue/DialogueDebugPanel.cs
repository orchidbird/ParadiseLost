using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueDebugPanel : MonoBehaviour{
	public DialogueManager Manager;
	public Text textUI;

	void Update(){
		if(Manager.isLeftUnitOld){
			textUI.text = "LEFT";
		}else{
			textUI.text = "RIGHT";
		}
	}
}