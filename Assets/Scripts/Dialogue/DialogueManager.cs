using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameData;
using Enums;
using UtilityMethods;

public class DialogueManager : MonoBehaviour{
	public string directScene; //값을 넣으면 무조건 해당 스크립트 파일만 열어줌. 테스트용. "TestAll"을 넣으면 1~20까지 오류 확인
    static DialogueManager instance;
    public static DialogueManager Instance { get { return instance; } }
	public Sprite transparent;
	
    public GameObject dialogueUICanvas;
    public GameObject dialogueUI;
    public GameObject buttons;
    public GameObject menuPanel;
	public GameObject dialogueLogScrollView;
	public GameObject glosNotification;
	public Image Tutorial;

	public Image leftPortrait;
	public Image rightPortrait;
	public bool isLeftUnitOld;
	Image GetPortrait(string input){
		if(input == "left" || leftPortrait.sprite.name.StartsWith(input)) return leftPortrait;
		if(input == "right" || rightPortrait.sprite.name.StartsWith(input)) return rightPortrait;
		return null;
	}
	Image OldPortrait{get{return isLeftUnitOld ? leftPortrait : rightPortrait;}}

	public Image namePanel;
    public Image textPanel;

	public Text nameText;
	public Text dialogueText;
	public Text dialogueLogText;

    public SpriteRenderer background;
	
	List<DialogueData> dialogueDataList;
	public int line;
	int endLine;
	bool isWaitingMouseInput;
	public bool gray;
	
	public SceneLoader sceneLoader;
	public GameObject AskingUI;
    
    public GameObject exclamationMarkPrefab;

    GameObject[] objects;

	bool isQuivering;

	public void SkipDialogue(){
		InactiveSkipQuestionUI();

		int newLine = line;
		for (int i = newLine; i < endLine; i++){
			DialogueData data = dialogueDataList[i];
			if(data.Command == DialogueData.CommandType.Glos){
				SetGlossaryLevel(data);
			}
			if(HandleSceneChange(dialogueDataList[i])) {return;}
		}
	}

	bool HandleSceneChange (DialogueData Data){
		//뭔가 명령이 인식됐으면 true, 아무것도 하지 않았으면 false
		if (Data.Command == DialogueData.CommandType.Script){
			string nextScriptName = Data.GetCommandSubType ();
			sceneLoader.LoadNextDialogueScene (nextScriptName);
			return true;
		}if (Data.Command == DialogueData.CommandType.Battle){
			Debug.Log("Loading Battle : Stage " + Data.GetCommandSubType());
			VolatileData.progress.stageNumber = (StageNum)int.Parse(Data.GetCommandSubType());
			sceneLoader.LoadNextBattleScene();
			return true;
		}if(Data.Command == DialogueData.CommandType.Title){
			GoTitle();
			return true;	
		}if(Data.Command == DialogueData.CommandType.Return) {
            line = endLine;
            isWaitingMouseInput = false;
            dialogueUICanvas.SetActive(false);
            return true;
        }if (Data.Command == DialogueData.CommandType.Era){
			StartCoroutine(sceneLoader.LoadScene("Era", true));
			return true;
		}
		return false;
	}

	public void GoTitle(){
		if(VolatileData.progress.dialogueName.Contains("Lose"))
			Ask("Retry", () => sceneLoader.LoadNextBattleScene(), () => sceneLoader.GoToTitle());
		else
			sceneLoader.GoToTitle();
	}

	private Action YesAction;
	private Action NoAction;
	public void Ask(string keyword, Action yes, Action no = null){
		UIManager.Instance.PushUI(AskingUI);
		YesAction = yes;
		NoAction = no;
		AskingUI.transform.Find("Text").GetComponent<Text>().text = Language.Find("Ask_" + keyword);
	}

	public void AskSkip(){
		Ask("SkipDialogue", SkipDialogue);
	}
	public void DoYes(){
		YesAction();
	}
	public void DoNo(){
		if (NoAction == null)
			UIManager.Instance.DeactivateTopUI();
		else
			NoAction();
	}

	IEnumerator HandleCommand(){
		DialogueData data = dialogueDataList[line];
		DialogueData.CommandType Command = data.Command;
		string subType = data.GetCommandSubType();

		if(HandleSceneChange(data)) yield break;
			
		if(Command == DialogueData.CommandType.App){
			Sprite loadedSprite = VolatileData.GetSpriteOf(SpriteType.Illust, data.GetNameInCode());
			if(loadedSprite != null){
				GetPortrait(subType).sprite = loadedSprite;
				isLeftUnitOld = (subType == "right");
			}
		}else if (Command == DialogueData.CommandType.Disapp){
			if(subType == "All") ResetPortraits();
			else{
				isLeftUnitOld = (GetPortrait(subType) == leftPortrait);
				GetPortrait(subType).sprite = transparent;
			}
		}else if(Command == DialogueData.CommandType.BGM){
			SoundManager.Instance.PlayBGM (subType);		
		}else if(Command == DialogueData.CommandType.BG){
			Sprite bgSprite = Resources.Load<Sprite>("Background/" + subType);
            if(background != null)
			    background.sprite = bgSprite;
		}else if(Command == DialogueData.CommandType.SE){
			SoundManager.Instance.PlaySE (subType);
			Debug.Log("Play SE : " + subType);
		}else if(Command == DialogueData.CommandType.MovingFade){
			yield return MovingFade(float.Parse(subType));
		}else if(Command == DialogueData.CommandType.FO){
			yield return sceneLoader.Fade(true, string.IsNullOrEmpty(subType) ? 1 : float.Parse(subType));
		}else if(Command == DialogueData.CommandType.FI){
			ResetPortraits();
			Camera.main.orthographicSize = 360;
			SetUIActive(true);
			var peekLine = line;
			while (dialogueDataList[peekLine].IsEffect)
				peekLine += 1;
			HandleDialogue(peekLine);
			yield return sceneLoader.Fade(false, string.IsNullOrEmpty(subType) ? 1 : float.Parse(subType));
		}else if(Command == DialogueData.CommandType.Gray){
			SetGrayScale(true);
		}else if(Command == DialogueData.CommandType.Colorful){
			SetGrayScale(false);
		}else if(Command == DialogueData.CommandType.Glos){
			SetGlossaryLevel(data);
		}else if(Command == DialogueData.CommandType.Quake){
			StartCoroutine(Quake());
		}else if(Command==DialogueData.CommandType.Quiver){
			isQuivering = subType == "On";
			if(isQuivering) StartCoroutine(Quiver());
		}else if(Command == DialogueData.CommandType.Tutorial){
			Tutorial.enabled = true;
			Tutorial.sprite = Resources.Load<Sprite>("Tutorial/"+subType);
		}else Debug.LogError("Undefined effectType : " + dialogueDataList[line].Command);

		if (!dialogueDataList[line + 1].IsEffect) yield break;
		line += 1;
		yield return HandleCommand();
	}

	void SetGrayScale(bool on){
		gray = on;
		SetMaterial(on ? VolatileData.GetGrayScale() : null);
	}

	void SetMaterial(Material mat){
		if (background != null)
			background.material = mat == null? new Material(Shader.Find("Sprites/Default")) : mat;
			
		leftPortrait.material = mat;
		rightPortrait.material = mat;
	}

	IEnumerator Quake(){
		GameObject clickBlock = GameObject.Find("ClickBlockImage");
		clickBlock.GetComponent<Image>().enabled=true;
		Vector3 originalBGPosition = new Vector3();
        if(background != null)
            originalBGPosition = background.transform.position;
		Vector3 originalLeftPosition = leftPortrait.transform.position;
		Vector3 originalRightPosition = rightPortrait.transform.position;

		float quakeTime = 2f;
		float magnitude = 10f;
		float rate = 1f;

		while (rate > 0){
			Vector3 addingVector = UnityEngine.Random.insideUnitCircle * magnitude * rate;
            if(background != null)
			    background.transform.position = originalBGPosition + addingVector;
			leftPortrait.transform.position = originalLeftPosition + addingVector;
			rightPortrait.transform.position = originalRightPosition + addingVector;

			rate -= Time.deltaTime / quakeTime;

			yield return null;
		}
		rightPortrait.transform.position = originalRightPosition;
		leftPortrait.transform.position = originalLeftPosition;
        if(background != null)
		    background.transform.position= originalBGPosition;
		clickBlock.GetComponent<Image>().enabled = false;
	}

	IEnumerator Quiver(){
		Vector3 originalTextPosition = textPanel.transform.position;
		Vector3 originalNamePosition = namePanel.transform.position;

		float magnitude = 2f;

		while(isQuivering){
			Vector3 addingVector = UnityEngine.Random.insideUnitCircle * magnitude;

			textPanel.transform.position = originalTextPosition + addingVector;
			namePanel.transform.position = originalNamePosition + addingVector;
			yield return null;
		}
		textPanel.transform.position = originalTextPosition;
		namePanel.transform.position = originalNamePosition;
	}

	IEnumerator MovingFade(float stepTime){
		//stepTime = 한 걸음에 들어가는 시간(초)
		SetUIActive(false);
		
		Camera.main.DOOrthoSize(250, stepTime*4).Play();
		var wait1 = new WaitForSeconds(stepTime*0.2f);
		var wait2 = new WaitForSeconds(stepTime*0.8f);
		for (int i = 0; i < 4; i++){
			iTween.MoveBy(Camera.main.gameObject, new Vector3(0, 20, 0), 0.1f);
			yield return wait1;
			iTween.MoveBy(Camera.main.gameObject, new Vector3(0, -20, 0), 0.2f);
			yield return wait2;
		}

		yield return sceneLoader.Fade(true);
	}

	void SetUIActive(bool enable){
		dialogueUI.SetActive(enable);
		buttons.SetActive(enable);
	}
    
    const string exclamationMarkName = "ExclamationMark";
	void SetGlossaryLevel(DialogueData data){
		string info = data.GetCommandSubType();
		string glosItem = info.Substring(0, info.Length-1);
		int glosLevel = Utility.CharToInt(info.Last());
		var glos = GlobalData.GlossaryDataList.Find(x => x.nameKor.ToString() == glosItem);
		Debug.Assert(glos != null, glosItem + " is NOT found!");

        if(glos.level < glosLevel){
            glos.isNew = true;
	        glosNotification.SetActive(true);
	        glosNotification.GetComponentInChildren<Text>().text = Language.Select("항목 갱신: ", "Content renewed: ") + glos.Name;
        }
            
		glos.level = Math.Max(glos.level, glosLevel);
		if (glos.level <= 0) return;
		
		if(RecordData.openedGlossaries[glos.Type].ContainsKey(glos.index))
			RecordData.openedGlossaries[glos.Type][glos.index] = glos.level;
		else RecordData.openedGlossaries[glos.Type].Add(glos.index, glos.level);

		if(GlobalData.GlossaryDataList.All(g => g.Type != GlossaryType.Person || RecordData.openedGlossaries[GlossaryType.Person].ContainsKey(g.index))) {
			SteamAchievementManager.Instance.Achieve("XXMonster...?");
		}
	}

    public void GenerateExclamationMarkAt(GameObject gameObject){
	    if (gameObject.transform.Find(exclamationMarkName) != null) return;
	    
	    GameObject exclamationMark = Instantiate(exclamationMarkPrefab, gameObject.transform);
	    exclamationMark.transform.Translate(10, 0, 0);
	    exclamationMark.name = exclamationMarkName;
    }

    public void RemoveExclamationMarkFrom(GameObject gameObject) {
        Transform exclamationMark = gameObject.transform.Find(exclamationMarkName);
        if(exclamationMark != null) Destroy(exclamationMark.gameObject);
    }


	//유니티 씬에서 쓰는 것이므로 레퍼런스 없더라도 지우지 말 것
	public void InactiveSkipQuestionUI(){
		AskingUI.SetActive(false);
		UIStack.Refresh();
	}

    public void ActiveDialogueUI(){
        dialogueUI.SetActive(true);
	}

	public void SetDifficulty(int difficulty) {
		VolatileData.difficulty = (Difficulty)difficulty;
		UpdateCurrentDifficultyText();
	}

	public void TestAllDialogueData(){
		const int startScene = 1;
		const int endScene = 20;
		int scene = startScene;
		int part = 1;
		for (; scene <= endScene;) {
			while(true) {
				int tempScene = scene;
				int tempPart = part;
				var sceneName = "Scene#" + scene + "-" + part;
				VolatileData.progress.dialogueName = sceneName;
				var dataList = new List<DialogueData> ();
				var basePath = "Data/" + VolatileData.progress.dialogueName;
				var textAsset = Resources.Load<TextAsset> (basePath + Language.Select ("", "-en")) ?? Resources.Load<TextAsset> (basePath);
				if (textAsset == null) {
					Debug.Log (sceneName + " 파일이 존재하지 않음");
					scene++;
					break;
				}
				Debug.Log (sceneName + " 로드");
				string[] rowDataList = textAsset.text.Split (new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				for (var row = 1; row < rowDataList.Length; row++) {
					var data = Parser.CreateParsedObject<DialogueData> (rowDataList [row]);
					dataList.Add (data);
				}
				line = 0;
				foreach (var data in dataList) {
					if (data.IsEffect) {
						DialogueData.CommandType Command = data.Command;
						string subType = data.GetCommandSubType ();
						if (Command == DialogueData.CommandType.App) {
							Sprite loadedSprite = VolatileData.GetSpriteOf (SpriteType.Illust, data.GetNameInCode ());
							if (loadedSprite == null) {
								Debug.Log (sceneName + ": 인물 일러스트 " + subType + " 스프라이트 파일 없음");
							}
						} else if (Command == DialogueData.CommandType.Disapp) {
						} else if (Command == DialogueData.CommandType.BGM) {
							SoundManager.Instance.PlayBGM (subType);
						} else if (Command == DialogueData.CommandType.BG) {
							Sprite bgSprite = Resources.Load<Sprite> ("Background/" + subType);
							if (background == null) {
								Debug.Log (sceneName + ": Background " + subType + " 스프라이트 파일 없음");
							}
						} else if (Command == DialogueData.CommandType.SE) {
							SoundManager.Instance.PlaySE (subType);
						} else if (Command == DialogueData.CommandType.MovingFade) {
						} else if (Command == DialogueData.CommandType.FO) {
						} else if (Command == DialogueData.CommandType.FI) {
							line += 1;
						} else if (Command == DialogueData.CommandType.Gray) {
						} else if (Command == DialogueData.CommandType.Colorful) {
						} else if (Command == DialogueData.CommandType.Glos) {
							SetGlossaryLevel (data);
						} else if (Command == DialogueData.CommandType.Quake) {
						} else if (Command == DialogueData.CommandType.Quiver) {
						} else if (Command == DialogueData.CommandType.Tutorial) {
							Tutorial.sprite = Resources.Load<Sprite> ("Tutorial/" + subType);
							if (Tutorial.sprite == null) {
								Debug.Log (sceneName + ": 튜토리얼 " + subType + " 스프라이트 파일 없음");
							}
						} else if (Command == DialogueData.CommandType.Battle) {
							part++;
						} else if (Command == DialogueData.CommandType.Era) {
							scene++;
							part = 1;
						} else if (Command == DialogueData.CommandType.Title) {
							scene = endScene + 1;
							part = 1;
						} else {
							Debug.Log (sceneName + ": Undefined effectType : " + data.Command);
						}
						line++;
					} else {
						string codeName = data.GetNameInCode ();
						var illust = VolatileData.GetSpriteOf (SpriteType.Illust, codeName + "-" + data.GetEmotion ());
						if (codeName != "blank") {
							if (illust.name == "notfound") {
								//Debug.Log (sceneName+": 인물 일러스트 '" + codeName + "-" + data.GetEmotion () + "' 스프라이트 파일 없음");
							}
							if (UnitInfo.ConvertName (codeName) == "NoName") {
								Debug.Log (sceneName + ": 코드네임 " + codeName + "의 이름 데이터 없음");
							}
							//Debug.Log (UnitInfo.ConvertName (codeName));
						}
						//Debug.Log (data.GetDialogue ());
						line++;
					}
				}
				if (tempScene != scene)
					break;
				if (tempScene == scene && tempPart == part) {
					scene = endScene + 1;
					Debug.Log (sceneName + ": 무한루프 발생. 조사 강제종료");
					break;
				}
			}
		}
	}

	public IEnumerator Initialize(){
        dialogueUICanvas.SetActive(true);
		if (VolatileData.gameMode == GameMode.AllStageTest){
			dialogueUICanvas.SetActive(false);
			yield break;
		}
		
		if (!string.IsNullOrEmpty (directScene)){
			if (directScene == "TestAll")
				TestAllDialogueData ();
			else
				VolatileData.progress.dialogueName = directScene;
		}
		
		dialogueDataList = Parser.GetParsedData<DialogueData>();

		InactiveSkipQuestionUI();

		endLine = dialogueDataList.Count;
		yield return PrintLinesFrom(0);
	}

	void ResetPortraits(){
		leftPortrait.sprite = transparent;
		rightPortrait.sprite = transparent;
		isLeftUnitOld = true;
	}

    IEnumerator PrintLinesFrom(int startLine){
		ResetPortraits(); // Initialize.

		line = startLine;
		while (line < endLine){
			isWaitingMouseInput = true;

			if (dialogueDataList[line].IsEffect){
				yield return HandleCommand();
				line++;
			}else if(sceneLoader.loaded){
				HandleDialogue(line);
				
				if (Tutorial.enabled){
					while (isWaitingMouseInput)
						yield return null;
					
					Tutorial.enabled = false;
					isWaitingMouseInput = true;
					dialogueLogs.Add(new DialogueLog());
					while (isWaitingMouseInput)
						yield return null;
				}else
					dialogueLogs.Add(new DialogueLog());

				while (isWaitingMouseInput)
					yield return null;

				line++;
			}
			yield return null; //없으면 dialogue씬 시작할 때 무한루프 발생
		}
	}

	void HandleDialogue(int inputLine){
		var data = dialogueDataList[inputLine];
		string codeName = data.GetNameInCode();
		nameText.text = UnitInfo.ConvertName(codeName);
		codeName = _String.GeneralName(codeName);
		var illust = VolatileData.GetSpriteOf(SpriteType.Illust, codeName+"-"+data.GetEmotion());
		
		leftPortrait.color = Color.gray;
		rightPortrait.color = Color.gray;
		
		dialogueText.text = data.GetDialogue();
		namePanel.gameObject.SetActive(nameText.text != "-");

		if(illust.name != "notfound"){
			Image newPortrait = GetPortrait(codeName) ?? OldPortrait;
			newPortrait.sprite = illust;
			isLeftUnitOld = newPortrait == GetPortrait("right");
		}
		
		if(namePanel.gameObject.activeSelf && GetPortrait(codeName) != null) 
			GetPortrait(codeName).color = Color.white;
	}

	List<DialogueLog> dialogueLogs = new List<DialogueLog>();
	public void Rewind(){ //Unity Button에 연동되므로 private 전환하지 말 것!
		if(dialogueLogs.Count < 2) return;
		var log = dialogueLogs[dialogueLogs.Count-2];
		dialogueLogs.Remove(dialogueLogs.Last());
		
		leftPortrait.sprite = log.leftImage;
		rightPortrait.sprite = log.rightImage;
		background.sprite = log.backGround;
		isLeftUnitOld = log.isLeftUnitOld;
		leftPortrait.color = log.leftColor;
		rightPortrait.color = log.rightColor;
		SetGrayScale(log.gray);
		nameText.text = log.speakerName;
		namePanel.gameObject.SetActive(nameText.text != "-");
		dialogueText.text = log.content;
		line = log.lineNumber;
	}

	public void OnClickDialogue(){
		//Debug.Log("OnClick Dialogue");
		if(isWaitingMouseInput && UIStack.IsEmpty()){
			isWaitingMouseInput = false;
		}
	}
	void LoadDialogueLogs(){
		dialogueLogText.text = "";
		foreach (DialogueLog log in dialogueLogs){
			if(log.speakerName != "-") dialogueLogText.text += "[" + log.speakerName + "] ";
			dialogueLogText.text += log.content.Replace ("\n", " ") + "\n";
		}
	}

	public void OpenDialogueLog(){
		LoadDialogueLogs ();
		dialogueLogScrollView.transform.GetComponentInChildren<Scrollbar> ().value = 0;
	}
	public void OpenOrCloseDialogueLog(){
		if (!dialogueLogScrollView.activeSelf) {
			OpenDialogueLog ();
		} else {
			dialogueLogScrollView.SetActive (false);
			UIStack.Refresh();
		}
	}
    
    void Awake() {
        instance = this;
    }
    
	void Start(){
		if (SceneManager.GetActiveScene().name == "Battle") return;
		StartCoroutine(Initialize());
		GlobalData.SetGlossaryDataList();
		GameDataManager.Save();
	}

	public void UpdateCurrentDifficultyText(){
		menuPanel.transform.Find("Buttons/Difficulty/Text").GetComponent<Text>().text =
			Language.Select("난이도 변경\n(현재 ", "Change difficulty\n(Now ") + _String.FromDifficulty(VolatileData.difficulty) + ")";
	}

	public void DeactivateTopUI() {
		UIStack.DeactivateTopUI(caller : this);
	}

	int frameWait = 0;
    void Update() {
		if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) {
			OnClickDialogue();
		} else if (Input.GetKey (KeyCode.LeftControl)) {
			frameWait += 1;
			if (frameWait == Setting.fastDialogueFrameLag) {
				frameWait = 0;
				OnClickDialogue();
			}
		} else if (Input.GetKeyDown (KeyCode.Escape) || Input.GetMouseButtonDown(1)) {
			DeactivateTopUI();
		} else if (Input.GetKeyDown (KeyCode.L)){
			OpenOrCloseDialogueLog ();
		}else if(Input.GetKeyDown(KeyCode.Backspace)){
			Rewind();
		}
    }
}
