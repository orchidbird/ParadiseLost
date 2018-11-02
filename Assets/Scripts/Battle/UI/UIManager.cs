using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
	private static UIManager instance;

	public static UIManager Instance{get{
		return instance ?? (instance = FindObjectOfType<UIManager>());
	}}

	void Awake (){
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	public Warning warningPanel;
	public void Warn(Action action, string text){
		PushUI(warningPanel.gameObject);
		warningPanel.Initialize(action, text);
	}
	public void Warn(int typeNumber){
		PushUI(warningPanel.gameObject);
		warningPanel.Initialize(typeNumber);
	}
	
	public GameObject Notification;
	public void Notify(string text){
		PushUI(Notification.gameObject);
		Notification.GetComponentInChildren<Text>().text = text;
	}
	public void PushUI(GameObject input){
		UIStack.Push(input);
	}
	public GameObject DeactivateTopUI() {
		return UIStack.DeactivateTopUI(caller : this);
	}

	public virtual void Update(){
		if (Input.GetMouseButtonDown(1) && (SceneManager.GetActiveScene().name != "Battle" || !BattleData.rightClickLock))
			DeactivateTopUI();
	}
}
