using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Enums;
using GameData;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;
using DG.Tweening;

public class EraManager : MonoBehaviour {
	StageNum lastStageNum;
	int date; //각각 1부의 시작과 끝인 1월 18일이 0, 2월 27일이 40에 해당한다
	readonly float duration = 1; //시간대 이동 연출에 걸릴 시간.
	float remainDuration;
	int dateDifference;
	StageInfo lastStageInfo;

	public GameObject Clock;
	public GameObject Arrow;
	public GameObject Locations;
	public GameObject ChallengeModeButton;
	public RectTransform Timeline;
	public EraPortrait EraPortrait_Location;
	
	public Text currentDifficulty;
	public EraTextManager EraTextManager;
	public GameObject ChapterButtonPrefab;
	readonly List<GameObject> ChapterButtons = new List<GameObject>();
	public List<EraPortrait> LocationPortraits;
	
	public void ElapseTo(StageNum stage){
		var selectedChapter = lastStageInfo;
		var newChapter = Parser.GetParsedData<StageInfo>().Find(chap => chap.stage == stage);
		dateDifference = newChapter.date - selectedChapter.date;

		if (dateDifference == 0) return;
		remainDuration = duration;
	}

	public void StageStart() {
		if (lastStageInfo == null) return;
		FindObjectOfType<SceneLoader>().LoadNextDialogueScene("Scene#" + (int)lastStageInfo.stage/10 + "-1");
	}

	void InstantiateChapterButtons(int nextChapter){
		var table = Parser.GetParsedData<StageInfo>();
		for (int i = 0; i < (int)table.Last().stage / 10; i++){
			var stageInfo = table.Find(info => (int) info.stage / 10 == i + 1);
			var chapterButtonTransform = Instantiate(ChapterButtonPrefab, Timeline).GetComponent<RectTransform>();
			chapterButtonTransform.anchoredPosition = new Vector2(stageInfo.date * 170 + 100, 42 - (int)Utility.PCNameToFaction(stageInfo.POV) * 30);
			ChapterButtons.Add(chapterButtonTransform.gameObject);
			
			var button = chapterButtonTransform.GetComponent<Button>();
			button.interactable = false;
			button.onClick.AddListener(() => SetChapter((int)stageInfo.stage/10));
			UI.SetIllustPosition(chapterButtonTransform.Find("ImageMask").Find("Image").GetComponent<Image>(), stageInfo.POV);
			chapterButtonTransform.Find("ShadowImage").GetComponent<Image>().DOFade(0.9f, 0);
			chapterButtonTransform.Find("Text").GetComponent<Text>().text = i+1 + ". " + table.Find(info => (int) info.stage / 10 == i + 1).Title;

			if (i >= nextChapter) continue;
			button.interactable = true;
			chapterButtonTransform.Find("ShadowImage").GetComponent<Image>().DOFade(i == nextChapter - 1 ? 0 : 0.5f, 0);
		}
	}

	void HighlightButton(int number) {
		for (int i = 0; i < ChapterButtons.Count; i++){
			if (ChapterButtons[i].GetComponent<Button>().IsInteractable() == false) continue;
			ChapterButtons[i].transform.Find("ShadowImage").GetComponent<Image>().DOFade(i == number - 1 ? 0 : 0.5f, 0);
		}
	}

	// RecordData.stageClearRecords.Keys.Max()의 값은 마지막으로 클리어한 가장 오른쪽의 스테이지
	IEnumerator Start(){
		if (RecordData.stageClearRecords.Count < 1) lastStageNum = (StageNum)81;
		else lastStageNum = RecordData.stageClearRecords.Keys.Max();
		lastStageInfo = Parser.GetParsedData<StageInfo>().Find(chap => chap.stage == lastStageNum);

		int nextChapter = (int)lastStageNum/10 + 1;
		if (nextChapter > 20) nextChapter = 20;
		var nextStageInfo = Parser.GetParsedData<StageInfo>().Find(chap => (int)chap.stage == nextChapter * 10 + 1);

		SetTimelineRectTransform(nextStageInfo);
		MoveArrowInTimeline(lastStageInfo, true);
		InitializeEachCharacterOnField();
		MoveEachCharacterInField(lastStageInfo, true);
		InstantiateChapterButtons(nextChapter);

		yield return null;
		
		SetChapter(nextChapter);
		currentDifficulty.text = Language.Select("난이도: ", "Difficulty: ") + _String.FromDifficulty(VolatileData.difficulty);
		
		if(lastStageNum == StageNum.S1_2)
			TutorialManager.Instance.Activate("Era");
		if(lastStageNum == StageNum.S6_1)
			TutorialManager.Instance.Activate("Challenge");
		if(!VolatileData.OpenCheck(Setting.challengeModeOpenStage))
			ChallengeModeButton.SetActive(false);
	}

	void Update(){
		if(remainDuration < 0) return;
		remainDuration -= Time.deltaTime;
		Clock.transform.Rotate(Vector3.forward * dateDifference * Time.deltaTime * 360 / duration);
	}

	void SetTimelineRectTransform(StageInfo nextStageInfo) {
		var yPos = Timeline.localPosition.y;
		var nextChapter = (int)nextStageInfo.stage / 10;
		var nextStageDate = nextStageInfo.date; 

		if (nextChapter < 7)
			Timeline.localPosition = new Vector3(0, yPos, 0);
		else if (nextChapter >= 20)
			Timeline.localPosition = new Vector3(-5900, yPos, 0);
		else
			Timeline.localPosition = new Vector3(-170 * (nextStageDate-5), yPos, 0);
	}

	public void SetChapter(int chapterNumber){
		HighlightButton(chapterNumber);

		var stageNumber = chapterNumber * 10 + 1;
		var stageInfo = Parser.GetParsedData<StageInfo>().Find(chap => (int)chap.stage == stageNumber);
		EraTextManager.SetEraText(stageInfo);

		ElapseTo(stageInfo.stage);

		lastStageInfo = stageInfo;

		MoveArrowInTimeline(stageInfo);
		MoveEachCharacterInField(stageInfo); 
	}

	void InitializeEachCharacterOnField() {
		foreach (var chapter in StageInfo.Stages){
			if((int)chapter.stage % 10 != 1 || chapter.stage - lastStageNum > 10) continue;

			var locationPortrait = Instantiate(EraPortrait_Location, transform);
			LocationPortraits.Add(locationPortrait);
			locationPortrait.Initialize(chapter);
		}
		
		foreach (var locationImage in Locations.GetComponentsInChildren<Image>()){
			locationImage.enabled = false;
			StartCoroutine(locationImage.GetComponent<EraLocation>().PullChildren());
		}
	}

	void MoveEachCharacterInField(StageInfo stageInfo, bool immediate = false) {
		var locationData = Parser.FindRowDataOf(Resources.Load<TextAsset>("Data/PCLocationAtChapter").text,
			((int)stageInfo.stage / 10).ToString());

		for(int i = 0; i < LocationPortraits.Count; i++){
			var portrait = LocationPortraits[i];
			portrait.POV.enabled = portrait.gameObject.name == stageInfo.POV;
			portrait.interactable = portrait.transform.parent.gameObject.name != locationData[i + 1];
			portrait.transform.SetParent(Locations.transform.Find(locationData[i+1]));
		}
		
		foreach (var location in Locations.GetComponentsInChildren<EraLocation>())
			StartCoroutine(location.PullChildren(immediate? 0 : duration));
	}

	Tween TWArrow;

	void MoveArrowInTimeline(StageInfo stageInfo, bool immediate = false) {
		var date = stageInfo.date;
		if (TWArrow != null) TWArrow.Kill();
		if (immediate) Arrow.GetComponent<RectTransform>().localPosition = new Vector3(100 + 170*date, 3, 0);
		else TWArrow = Arrow.transform.DOLocalMoveX(100 + 170*date, duration);
	}

	public void ChangeDifficulty(int input){
		if (input == 1 && VolatileData.difficulty == Difficulty.Legend)
			return;
		if (input == -1 && VolatileData.difficulty == Difficulty.Intro)
			return;
		
		VolatileData.SetDifficulty((int)VolatileData.difficulty + input);
		currentDifficulty.text = Language.Select("난이도: ", "Difficulty: ") + _String.FromDifficulty(VolatileData.difficulty);
		Debug.Log("난이도 변경: " + _String.FromDifficulty(VolatileData.difficulty));
	}
}
