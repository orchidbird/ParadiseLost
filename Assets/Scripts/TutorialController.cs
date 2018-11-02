using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;
using Enums;
using TMPro;
using UtilityMethods;

public class TutorialController : MonoBehaviour {
	private static TutorialController instance;
	public static TutorialController Instance{ get { return instance; } }
	
	void Awake (){
		if (instance != null && instance != this) {
			Destroy(gameObject);
			return;
		}
		instance = this;
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)) EndTutorial();
		if (scenarioList != null && scenarioList.Last().index == index + 1 && Input.anyKey && BattleData.TutoState == TutorialState.Active)
			EndTutorial();
	}

	public Image screenImage;
	public Image paperImage;
	public Button NextButton;
	string usedSceneName;
	List<TutorialScenario> scenarioList;
	public int index;
	List<AIScenario> AIscenarioList;
	int AIscenarioIndex;
	public TemporaryObject markPrefab;
	public TemporaryObject mark;

	public void OnEnable(){
		if (BattleData.TutoState == TutorialState.Passive)
			return;
		
		TutorialScenario.TutorialController = this;
		usedSceneName = SceneManager.GetActiveScene().name;
		TextAsset searchData = Resources.Load<TextAsset>("Tutorial/" + usedSceneName + (int)VolatileData.progress.stageNumber);

		if(searchData == null || !TutorialManager.Instance.AllowTutorial(true))
			EndTutorial ();
		else{
			scenarioList = Parser.GetParsedData<TutorialScenario>();
			AIscenarioList = Parser.GetParsedData<AIScenario> ();
			AIscenarioIndex = 0;
			BattleData.TutoState = TutorialState.Active;
			BattleData.rightClickLock = true;
			BattleData.detailInfoLock = true;
			ToNextStep();
		}
	}

	private void EndTutorial(){
		Time.timeScale = 1;
		if(CurrentScenario != null)
			CurrentScenario.ResetMissionCondition();
		BattleData.rightClickLock = false;
		BattleData.shortClickLock = false;
		BattleData.TutoState = TutorialState.None;
		BattleData.detailInfoLock = false;
		screenImage.enabled = false;
		paperImage.gameObject.SetActive(false);
		SetControl(true);
		var announce = transform.Find("SkipAnnounce");
		if (announce != null)
			Destroy(announce.gameObject);
		
		//TileManager.SetInstance와 순서 문제로 인해 Instance 호출을 쓰지 않음.
		if (SceneManager.GetActiveScene().name != "Battle") return;
		FindObjectOfType<TileManager>().SetPreselectLock(false);
		if(BattleData.turnUnit != null)
			FindObjectOfType<TileManager>().PreselectTiles(BattleData.turnUnit.MovableTiles);
	}
	
    public void RemoveSpriteAndMark() {
        if(scenarioList == null) return;
        
	    if(BattleData.TutoState == TutorialState.Active)
        	screenImage.sprite = VolatileData.GetIcon(IconSprites.Transparent);
        if (CurrentScenario != null && CurrentScenario.mouseMarkPos != Vector3.zero && mark != null)
            Destroy(mark.gameObject);
    }
	void TryNewSprite(){
		var newSprite = SearchSprite();
		if (newSprite == null){
			screenImage.sprite = VolatileData.GetIcon(IconSprites.Transparent);
			var newString = Language.Select(CurrentScenario.korText, CurrentScenario.engText);
			if (newString != null){
				paperImage.gameObject.SetActive(true);
				paperImage.GetComponentInChildren<TextMeshProUGUI>().text = _String.ColorExplainText(newString);	
			}
		}else{
			screenImage.sprite = newSprite;
			paperImage.gameObject.SetActive(false);
		} 
		Debug.Log("Tutorial Step " + index);
    }
	Sprite SearchSprite(){
		return Resources.Load<Sprite>("Tutorial/"+SceneManager.GetActiveScene().name + (int)VolatileData.progress.stageNumber + "_" + index);
	}

	public void OnClick(){
		if (BattleData.TutoState == TutorialState.Passive || BattleData.isWaitingUserInput)
			ToNextStep();
	}

	IEnumerator DisableButtonForSeconds(float duration){
		NextButton.enabled = false;
		var startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < startTime + duration){
			yield return null;
		}
		NextButton.enabled = true;
	}
	public string subjectTitle;
	public void ToNextStep(){
		if(BattleData.TutoState == TutorialState.Passive){
			index += 1;
			var NewSprite = Resources.Load<Sprite>("Tutorial/" + subjectTitle + "/" + index);
			if (NewSprite != null){
				screenImage.sprite = NewSprite;
				StartCoroutine(DisableButtonForSeconds(1));
			}else if (TutorialManager.subjectQueue.Count > 0)
				TutorialManager.Instance.DequeueTutorial();
			else
				EndTutorial();
			
			return;
		}
		
		TutorialScenario oldScenario = CurrentScenario;
		if (oldScenario != null)
			oldScenario.ResetMissionCondition();

		index++; TryNewSprite();

		TutorialScenario newScenario = CurrentScenario;
		if (newScenario != null){
			if (newScenario.IsEndMission) {EndTutorial ();}
			SetControl(true);
			newScenario.SetMissionCondition ();
			if(newScenario.mouseMarkPos != Vector3.zero)
				mark = Instantiate(markPrefab, newScenario.mouseMarkPos, Quaternion.identity, BattleUIManager.Instance.battleUICanvas.transform);
		}
		else {SetControl(false);}

		if(BattleData.TutoState == TutorialState.Active)
			transform.Find("SkipAnnounce").GetComponent<Text>().text =
				Language.Find("SkipTutorial") + "  " + index + " / " + (scenarioList.Last().index - 1);
	}

	public bool IsScenarioOf(TutorialScenario.Mission mission){return CurrentScenario != null && CurrentScenario.mission == mission;}
	public TutorialScenario CurrentScenario{get{return scenarioList == null ? null : scenarioList.Find(data => data.index == index);}}

	//able이 true이면 통제권을 주고, false이면 빼앗음
	public void SetControl(bool able){
		NextButton.enabled = !able;
		screenImage.raycastTarget = !able;
		Setting.shortcutEnable = able;
	}

	public void SetActive(bool able){
		screenImage.enabled = able;
		SetControl(!able);
	}

	public AIScenario GetNextAIScenario(){
		return AIscenarioList [AIscenarioIndex++];
	}
}
