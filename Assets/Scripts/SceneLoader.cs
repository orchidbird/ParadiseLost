using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using GameData;
using Save;
using Enums;

public class SceneLoader : LogTracker{
	public GameObject fadeoutScreenObject;
	public LoadingScreen loadingScreenPrefab;

	public void GoToTitle() {
		StartCoroutine(FadeoutAndLoadDialogueScene("Title"));
	}

	public void ChangeLanguage(){
		if (VolatileData.language == Lang.Kor)
			VolatileData.language = Lang.Eng;
		else if (VolatileData.language == Lang.Eng)
			VolatileData.language = Lang.Kor;
		
		if(SceneManager.GetActiveScene().name == "Title")
			FindObjectOfType<TitleManager>().SetNameImage();
	}
	public void QuitGame(){Application.Quit();}

	public void LoadNextBattleScene(StageNum stage = 0) {
		if (stage > 0) VolatileData.progress.stageNumber = stage;

		Debug.Log("Load Battle: " + VolatileData.progress.stageNumber);
		if (SceneManager.GetActiveScene().name == "BattleReady") {
			StartCoroutine(FadeoutAndLoadBattleScene());
			return;
		}

		VolatileData.progress.isDialogue = false;
		switch (VolatileData.gameMode) {
		case GameMode.Test:
			CheckStageAndLoadBattle();
			break;
		case GameMode.Challenge:
			StartCoroutine(FadeoutAndLoadBattleReadyScene());
			break;	
		case GameMode.AllStageTest:
			CheckStageAndLoadBattle();
			break;
		case GameMode.Story:
			RecordData.progress.Clone(VolatileData.progress);
			GameDataManager.Save();
			CheckStageAndLoadBattle();
			break;
		}
	}

	void CheckStageAndLoadBattle(){
		StartCoroutine(!VolatileData.OpenCheck(Setting.readySceneOpenStage)
			? FadeoutAndLoadBattleScene()
			: FadeoutAndLoadBattleReadyScene());
	}

	public void LoadBattleSceneFromBattleReadyScene() {
		GameDataManager.Save();
		StartCoroutine(FadeoutAndLoadBattleScene());
	}
	public void LoadNextDialogueScene(string nextSceneName){
		if (nextSceneName != "Title") {
			VolatileData.progress.dialogueName = nextSceneName;
			VolatileData.progress.isDialogue = true;
			if (VolatileData.gameMode == GameMode.Story || VolatileData.gameMode == GameMode.Test) {
				if (!nextSceneName.Contains("Lose"))
					RecordData.progress.Clone(VolatileData.progress);
				GameDataManager.Save();
			}
			StartCoroutine(FadeoutAndLoadDialogueScene(nextSceneName));
		}else
			DialogueManager.Instance.GoTitle();
	}

	public void StartChallengeMode(){ //UI.Button으로 참조
		StartCoroutine(LoadArchiveScene());
	}
	public void EndChallengeMode(){
		VolatileData.gameMode = GameMode.Story;
		StartCoroutine(LoadScene("Era", true));
	}
	public IEnumerator LoadArchiveScene() {
		GameDataManager.RecordPlayLog(new SpentTimePlayLog(SceneManager.GetActiveScene().name, spentTime));
		spentTime = 0;
		GameDataManager.Save();
		VolatileData.gameMode = GameMode.Challenge;
		VolatileData.progress.Clone(RecordData.progress);
		yield return Fade(true);
		SceneManager.LoadScene("Archive");
	}

	public IEnumerator LoadScene(string sceneName, bool save){
		GameDataManager.RecordPlayLog(new SpentTimePlayLog(SceneManager.GetActiveScene().name, spentTime));
		spentTime = 0;
		if(save) GameDataManager.Save();
		yield return Fade(true);
		SceneManager.LoadScene(sceneName);
	}

	void Awake() {
		Application.backgroundLoadingPriority = ThreadPriority.Low;
	}

	float spentTime = 0;
	float noInputTime = 0;

	void Update() {
		noInputTime += Time.deltaTime;
		if (Input.anyKeyDown || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) {
			noInputTime = 0;
		}
		if (noInputTime <= Setting.noInputTimeToBeConsideredAsAFK)
			spentTime += Time.deltaTime;
	}

	// 로딩 화면이 필요한 씬에 돌입하기 이전에 LoadingScreen을 만들어놓으면, 이 LoadingScreen은 DontDestroyOnLoad에 의해
	// 씬 돌입 이후에 사라지지 않는다. 이후 사용자가 LoadingScreen을 클릭하면 사라지고, loaded가 true가 된다.
	// 로딩 화면이 필요하지 않은 씬의 경우 씬 돌입 이후 LoadingScreen이 없으므로 바로 loaded가 true가 된다.
	public bool loaded;
	IEnumerator Start(){
		Cursor.visible = false;
		var sceneName = SceneManager.GetActiveScene().name;
		var loadingScreen = FindObjectOfType<LoadingScreen>();
		while (true) {
			if (loadingScreen == null)
				break;
			if (loadingScreen.Clicked) {
				Destroy(loadingScreen.gameObject);
				break;
			}
			yield return null;
		}
		loaded = true;
		yield return Fade(false);

		if (sceneName == "Title"){
			FindObjectOfType<SoundManager>().PlayBGM("Monad_Title");
			StartCoroutine(FindObjectOfType<TitleManager>().ShowTitleName());
		}else if(sceneName == "BattleReady")
			FindObjectOfType<SoundManager>().PlayBGM("Epic_Monarch");
	}

	//true면 FO, false면 FI.
	public IEnumerator Fade(bool fadeOut, float time = 1f){
		fadeoutScreenObject.SetActive(true);

		if (!fadeOut){
			var startTime = Time.realtimeSinceStartup;
			while (startTime + Setting.fadeInWaitingTime > Time.realtimeSinceStartup){
				yield return null;
			}
		}
			
		Time.timeScale = 0;
		var img = fadeoutScreenObject.GetComponent<Image>();
		Tweener tween;

		if (fadeOut)
			tween = img.DOColor(Color.black, time).SetUpdate(true);
		else {
			img.color = Color.black;
			tween = img.DOColor(new Color(0, 0, 0, 0), time).SetUpdate(true);
		}

		yield return tween.WaitForCompletion();
		fadeoutScreenObject.SetActive(fadeOut);

		Time.timeScale = 1.0f;
	}
	IEnumerator FadeoutAndLoadBattleScene() {
		GameDataManager.RecordPlayLog(new SpentTimePlayLog(SceneManager.GetActiveScene().name, spentTime));
		spentTime = 0;
		yield return Fade(true);
		var loadingScreen = Instantiate(loadingScreenPrefab);
		loadingScreen.afterSceneName = "Battle";
		loadingScreen.Initialize(VolatileData.progress.stageNumber);
		SceneManager.LoadScene("Battle");
	}
	IEnumerator FadeoutAndLoadBattleReadyScene() {
		GameDataManager.RecordPlayLog(new SpentTimePlayLog(SceneManager.GetActiveScene().name, spentTime));
		spentTime = 0;
		yield return Fade(true);
		SceneManager.LoadScene("BattleReady");
	}

	IEnumerator FadeoutAndLoadDialogueScene(string nextScriptFileName) {
		GameDataManager.RecordPlayLog(new SpentTimePlayLog(SceneManager.GetActiveScene().name, spentTime));
		spentTime = 0;
		yield return Fade(true);
		if (nextScriptFileName == "Title") {
			Time.timeScale = 1.0f;
			SceneManager.LoadScene("Title");
		} else {
			if (nextScriptFileName.Contains("-1")){
				var loadingScreen = Instantiate(loadingScreenPrefab);
				loadingScreen.afterSceneName = "Dialogue";
				var sceneNumber = nextScriptFileName.Substring(6); //예) '13-2'
				var stage = (StageNum)(int.Parse(sceneNumber.Remove(sceneNumber.Length-2)) * 10 + 1);
				loadingScreen.Initialize(stage);
			}
			SceneManager.LoadScene("Dialogue");
		}
	}
}
