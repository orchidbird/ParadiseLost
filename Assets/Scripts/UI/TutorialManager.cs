using System.Collections.Generic;
using Enums;
using GameData;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour{
	static readonly bool allowInteractiveTutorialOnTest = false;
	static readonly bool allowPassiveTutorialOnTest = false;
	public bool AllowInteractiveTutorial{
		get { return allowInteractiveTutorialOnTest || VolatileData.gameMode != GameMode.Test; }
	}
	public bool AllowPassiveTutorial{
		get { return BattleData.TutoState != TutorialState.Active && (allowPassiveTutorialOnTest || VolatileData.gameMode != GameMode.Test); }
	}
	
	private static TutorialManager instance;
	public static TutorialManager Instance{ get { return instance; } }
	void Awake (){
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	public static Queue<string> subjectQueue = new Queue<string>();
	
	public TutorialController Tutorial;
	public List<GameObject> chapters;
	int page;

	void OnEnable(){
		for (int i = 0; i < chapters.Count; i++)
			chapters[i].gameObject.SetActive(i < (int)VolatileData.progress.stageNumber/10);
	}

	public bool AllowTutorial(bool interactive){
		return false;
		return BattleData.currentPhase > 0 && 
			   (interactive && AllowInteractiveTutorial || !interactive && AllowPassiveTutorial);
	}

	public void DequeueTutorial(){
		if(subjectQueue.Count > 0)
			Activate(subjectQueue.Dequeue(), true);
	}
	public void ReserveTutorial(string title){
		if (!subjectQueue.Contains(title) && !RecordData.alreadyReadTutorials.Contains(title) && AllowTutorial(false))
			subjectQueue.Enqueue(title);
		//PrintAllInQueue();
	}

	void PrintAllInQueue(){
		var result = "";
		foreach (var item in subjectQueue)
			result += item + " / ";
		Debug.Log("Tutorial Queue: " + result);
	}
	//UI.Button에서 참조
	public void Activate(string title, bool queued = false, bool review = false){//Queue 기능을 통해 호출될 경우.
		return;
		if (SceneManager.GetActiveScene().name == "Battle" && !AllowTutorial(false)
		    || !review && RecordData.alreadyReadTutorials.Contains(title)
		    || subjectQueue.Contains(title))
			return;
		if (BattleData.TutoState == TutorialState.Passive && !queued){
			subjectQueue.Enqueue(title);
			return;
		}

		SoundManager.Instance.PlaySE("Tutorial");
		Time.timeScale = 0;
		BattleData.TutoState = TutorialState.Passive;
		if(!RecordData.alreadyReadTutorials.Contains(title))
			RecordData.alreadyReadTutorials.Add(title);
		Tutorial.gameObject.SetActive(true);
		Tutorial.screenImage.enabled = true;
		Tutorial.SetControl(false);
		Tutorial.subjectTitle = title;
		Tutorial.index = 0;
		Tutorial.ToNextStep();
	}
}
