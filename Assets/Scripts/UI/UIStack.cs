using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 켜진 UI들을 스택에 넣어둔다. 이후 esc를 누르면 스택에서 빼면서 비활성화한다.
public static class UIStack{
	public static Stack<GameObject> myStack = new Stack<GameObject>();
	
	public static void Push(GameObject gameObject){
		gameObject.SetActive(true);
		foreach(var ui in myStack)
			if(ui == gameObject)
				return;
		myStack.Push(gameObject);
	}

	public static GameObject DeactivateTopUI(object caller){
		//Debug.Log("Try deactivating top UI.");
		GameObject topUI;
		try {
			topUI = myStack.Peek();
		} catch {
			return null;
		}
		//Debug.Log("Top UI: " + topUI.gameObject.name);
		if(topUI == null || !topUI.activeSelf){
			myStack.Pop();
			return DeactivateTopUI(caller);
		}

		bool reallyDeactivate = true;
		if (topUI.GetComponent<DetailInfoPanel>() != null && BattleData.deactivateDetailInfoLock){
			reallyDeactivate = false;
			BattleUIManager.Instance.deactivateDetailInfoEvent.Invoke();
		}else if (SceneManager.GetActiveScene().name == "Dialogue" && topUI == DialogueManager.Instance.AskingUI){
			topUI.transform.Find("NoButton").GetComponent<Button>().onClick.Invoke();
		}

		if (reallyDeactivate){
			topUI.SetActive(false);
			myStack.Pop();	
		}
		
		return topUI;
	}
	
	/*public void PrintAll(){
		var result = "Print full stack: ";
		foreach (var ui in myStack){
			result += ui.name + ", ";
		}

		Debug.Log(result);
	}*/

	// 파괴되었거나 deactivate된 UI들을 스택에서 없앤다
	public static void Refresh(){
		Stack<GameObject> tempStack = new Stack<GameObject>();
		for(int i = 0; i < myStack.Count; i++) {
			GameObject ui = myStack.Pop();
			if(ui != null && ui.activeSelf)
				tempStack.Push(ui);
		}
		for(int i = 0; i < tempStack.Count; i++)
			myStack.Push(tempStack.Pop());
	}

	public static bool IsEmpty(){
		Refresh();
		return myStack.Count == 0;
	}
}
