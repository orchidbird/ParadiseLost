using UnityEngine;
using UnityEngine.UI;

public class EnterToClickButton : MonoBehaviour{
	void Update (){
		if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			GetComponent<Button>().onClick.Invoke();
	}
}
