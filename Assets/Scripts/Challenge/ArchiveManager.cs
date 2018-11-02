using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameData;
using Enums;
using UnityEngine.EventSystems;
using UtilityMethods;

public class ArchiveManager : MonoBehaviour {
    List<StageNum> stages = new List<StageNum>();

    public Image background;

    int currentChapter = 1;
	StageNum selectedStage = 0;

    public RectTransform stageBriefingPanel;

    public GameObject stageButtonsInPanel;
    public GameObject stageButtonPrefab;
	Dictionary<StageNum, GameObject> stageButtons = new Dictionary<StageNum, GameObject>();
	Dictionary<StageNum, StageClearRecord> bestRecords = new Dictionary<StageNum, StageClearRecord>();
    public GameObject starPrefab;

    public RectTransform backgroundPanel;
    public GameObject explanationPanel;
    public Image POVImage;
    Vector2 originalBackGroundPanelSize;
    float explanationPanelMaxHeight;
    float explanationPanelMinHeight;

    public GameObject availablePCPortraitPanel;
    public GameObject bestRecordPCPortraitPanel;
    public GameObject allyPortraitPanel;
    public GameObject enemyPortraitPanel;
    public GameObject portraitPrefab;

    public GameObject objectTriggers;
    public GameObject objectTriggerPrefab;
    public GameObject totalScoreText;

    // UI에 마우스오버하면 활성화되는 텍스트들
    public Text bestRecordText;
    public Text availablePCText;
    public Text allyText;
    public Text enemyText;

    public Button startButton;
    void Start () {
        explanationPanelMinHeight = explanationPanel.GetComponent<RectTransform>().sizeDelta.y;
        explanationPanelMaxHeight = explanationPanelMinHeight + 65;
        originalBackGroundPanelSize = backgroundPanel.sizeDelta;
        VolatileData.gameMode = GameMode.Challenge;
        RecordData.progress.Clone(VolatileData.progress);
        foreach (var kv in RecordData.stageClearRecords) {
            if(kv.Value.Any(record => record.star != -1))   // 클리어한 기록이 있다면
				stages.Add(kv.Key);
        }
        stages.Sort();
        CacheBestRecords();
        CreateStageButtons();
        RenewAvailablePCPortraits();
        SelectStage(stages.First());
	}
	
    void CreateStageButtons() {
	    Debug.Log("스테이지 수: " + stages.Count);
	    foreach(Transform child in stageButtonsInPanel.transform)
		    child.gameObject.SetActive(false);
	    
        foreach (var i in stages) {
            stageButtons.Add(i, CreateStageButton(i));
            stageButtons[i].transform.SetParent(stageButtonsInPanel.transform);
            stageButtons[i].GetComponent<Button>().onClick.AddListener(delegate { SelectStage(i); });
            stageButtons[i].SetActive(true);
        }
    }
	GameObject CreateStageButton(StageNum stageNum) {
        if (!RecordData.stageClearRecords.ContainsKey(stageNum))
            return null;
        GameObject stageButton = Instantiate(stageButtonPrefab);
		var chapterNumber = (int) stageNum / 10;
        stageButton.GetComponentInChildren<Text>().text = Language.Select(chapterNumber + "장 ", "Chapter " + chapterNumber) + "." + VolatileData.GetStageData(stageNum, StageInfoType.Title);
        stageButton.transform.Find("FrameImage").gameObject.SetActive(false);
        GameObject stars = stageButton.GetComponentInChildren<HorizontalLayoutGroup>().gameObject;
        for (int j = 0; j < bestRecords[stageNum].star; j++)
            Instantiate(starPrefab).transform.SetParent(stars.transform);
        return stageButton;
    }

    void CacheBestRecords() {
        // 각 스테이지의 최고 기록들을 불러온다.
        foreach(var kv in RecordData.stageClearRecords) {
            StageClearRecord bestRecord = null;
            foreach(var record in kv.Value) {
                if(bestRecord == null || record.IsBetterThan(bestRecord))
                    bestRecord = record;
            }
            bestRecords[kv.Key] = bestRecord;
        }
    }

	void SelectStage(StageNum i) {
        if(selectedStage != 0)
            stageButtons[selectedStage].transform.Find("FrameImage").gameObject.SetActive(false);
        selectedStage = i;
        stageButtons[selectedStage].transform.Find("FrameImage").gameObject.SetActive(true);
        VolatileData.progress.stageNumber = i;
        alreadyHighlighted = false;

        UpdateStageBriefingPanel();

        SceneLoader sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();
        startButton.onClick.AddListener(delegate { sceneLoader.LoadNextBattleScene(i); });
    }

    public void UpdateStageBriefingPanel() {
        RenewBackgroundImage();
        RenewExplanationText();
        RenewPOVimage();
        RenewBestRecordPCPortraits();
        RenewBestRecordTotalScore();
		CountUnitNum();
        RenewBattleTriggers();
    }
    void RenewAvailablePCPortraits() {
        List<string> PCNames = RecordData.GetUnlockedCharacters();
        GameObject PCPortraits = availablePCPortraitPanel.GetComponentInChildren<HorizontalLayoutGroup>().gameObject;
        foreach (Transform child in PCPortraits.transform)
            Destroy(child.gameObject);
        foreach (var name in PCNames) {
            Sprite unitPortraitSprite = VolatileData.GetSpriteOf(SpriteType.Portrait, name);
            GameObject portrait = Instantiate(portraitPrefab);
			portrait.transform.Find("UnitPortraitMask").Find("PortraitImage").GetComponent<Image>().sprite = unitPortraitSprite;
            portrait.transform.SetParent(PCPortraits.transform);
        }
    }
    string stageBackgroundsData = null;
    void RenewBackgroundImage() {
        background.sprite = VolatileData.GetStageBackground(selectedStage);
    }
    void RenewExplanationText() {
        explanationPanel.GetComponentInChildren<Text>().text = VolatileData.GetStageData(selectedStage, StageInfoType.Intro);
    }
    void RenewPOVimage()
    {
	    POVImage.sprite = VolatileData.GetSpriteOf(SpriteType.Illust,
		    VolatileData.GetStageData(selectedStage, StageInfoType.POV));
        float lowBorder = POVImage.sprite.border.y * POVImage.rectTransform.sizeDelta.y * POVImage.rectTransform.localScale.y / POVImage.sprite.rect.size.y;
        POVImage.rectTransform.anchoredPosition3D = new Vector3(-30, 7 - lowBorder, 0);
    }
    void RenewBestRecordPCPortraits() {
        List<string> PCNames = bestRecords[selectedStage].skillTrees.Keys.ToList();
        GameObject PCPortraits = bestRecordPCPortraitPanel.GetComponentInChildren<HorizontalLayoutGroup>().gameObject;
        foreach (Transform child in PCPortraits.transform)
            Destroy(child.gameObject);
        foreach (var name in PCNames) {
            Sprite unitPortraitSprite = VolatileData.GetSpriteOf(SpriteType.Portrait, name);
            GameObject portrait = Instantiate(portraitPrefab);
			portrait.transform.Find("UnitPortraitMask").Find("PortraitImage").GetComponent<Image>().sprite = unitPortraitSprite;
            portrait.transform.SetParent(PCPortraits.transform);
        }
    }
    void RenewBestRecordTotalScore() {
        Dictionary<string, List<int>> achievedTriggers = bestRecords[selectedStage].achievedTriggers;

        int totalScore = 0;
        foreach(var kv in achievedTriggers)
            totalScore += kv.Value[0] * kv.Value[1];
        totalScoreText.GetComponent<Text>().text = totalScore.ToString();
    }
	Dictionary<string, int> unitNumDict = new Dictionary<string, int>();
	void CountUnitNum() {
		unitNumDict = new Dictionary<string, int>();
		List<string> unitNames = VolatileData.stageData.GetUnitGenInfos().Select(unitInfo => unitInfo.CodeName).ToList();
		foreach(var unitName in unitNames) {
			if (!unitNumDict.ContainsKey(unitName))
				unitNumDict.Add(unitName, 1);
			else unitNumDict[unitName]++;
		}
	}
    
    void RenewBattleTriggers() {
        Dictionary<string, bool> everAchievedTriggers = RecordData.GetTriggersAchievedAtLeastOnce(selectedStage);

        // 해당 스테이지의 모든 battleTrigger들마다, 그 battleTrigger가 한번이라도 클리어된 적이 있는지의 여부를 들고 있는 딕셔너리
        Dictionary<BattleTrigger, bool> battleTriggers = new Dictionary<BattleTrigger, bool>();
        foreach (var battleTrigger in VolatileData.stageData.GetBattleTriggers()) {
            if(battleTrigger.GetName == null)   continue;
            battleTriggers.Add(battleTrigger, everAchievedTriggers.ContainsKey(battleTrigger.korName));
        }

        foreach (Transform child in objectTriggers.transform)
            Destroy(child.gameObject);
        foreach (var kv in battleTriggers) {
            // Key : 트리거
            // Value : 해당 트리거를 클리어한 적이 있는지의 여부
            GameObject achievedTrigger = Instantiate(objectTriggerPrefab);
            Text[] texts = achievedTrigger.GetComponentsInChildren<Text>();
            if (kv.Value || kv.Key.reward < 0) {
                int score = kv.Key.reward;
                texts[0].text = kv.Key.korName;
                if (kv.Key.repeatable) texts[0].text += "(반복)";
                texts[1].text = score.ToString();
            }
            else {
                texts[0].text = "???";
                texts[1].text = "?";
            }
            achievedTrigger.transform.SetParent(objectTriggers.transform);
        }
    }

    bool alreadyHighlighted = false;
    bool highLigtFixed = false;
    public void OnBestRecordClicked() {
        highLigtFixed = !highLigtFixed;
    }
    void OnBestRecordPointer(bool on) {
        if(!on) {
            if (!highLigtFixed && alreadyHighlighted) {
                RenewBattleTriggers();
                alreadyHighlighted = true;
            }
            return;
        }
        alreadyHighlighted = true;
        Dictionary<string, List<int>> achievedTriggers = bestRecords[selectedStage].achievedTriggers;
        foreach (Transform child in objectTriggers.transform) {
            Text[] texts = child.gameObject.GetComponentsInChildren<Text>();
            
            bool repeatable = false;
            if (texts[0].text.EndsWith("(반복)")) {
                texts[0].text = texts[0].text.Replace("(반복)", "");
                repeatable = true;
            }
            
            string triggerName = texts[0].text;
            if(achievedTriggers.ContainsKey(triggerName)) {
                Image image = child.gameObject.GetComponent<Image>();
                Color color = image.color;
                color.a = 0.5f;
                image.color = color;
                int times = achievedTriggers[triggerName][0];
                if(repeatable)
                    texts[0].text += " X" + times;
            }
        }
    }

    Coroutine backgroundZoomCoroutine = null;

    public IEnumerator OnMagnifierMouseOver() {
        if(backgroundZoomCoroutine != null)
            StopCoroutine(backgroundZoomCoroutine);
        POVImage.sprite = VolatileData.GetIcon(IconSprites.Transparent);
        backgroundZoomCoroutine = StartCoroutine(Utility.Resize(backgroundPanel, 
                stageBriefingPanel.sizeDelta - new Vector2(30, 20), Setting.cameraZoomDuration));
        yield return backgroundZoomCoroutine;
        backgroundZoomCoroutine = null;
        MapBriefingManager MBM = FindObjectOfType<MapBriefingManager>();
        MBM.GenerateUnitAndTileImages(stageBriefingPanel, backgroundPanel.gameObject);
    }
    public IEnumerator OnMagnifierMouseExit() {
        MapBriefingManager MBM = FindObjectOfType<MapBriefingManager>();
        MBM.EraseAllTileImages();
        MBM.EraseAllUnitImages();
        if (backgroundZoomCoroutine != null)
            StopCoroutine(backgroundZoomCoroutine);
        backgroundZoomCoroutine = StartCoroutine(Utility.Resize(backgroundPanel,
                originalBackGroundPanelSize, Setting.cameraZoomDuration));
        yield return backgroundZoomCoroutine;
        RenewPOVimage();
        backgroundZoomCoroutine = null;
    }

    void AdjustExplanationPanelSize(bool expand) {
        RectTransform rect = explanationPanel.GetComponent<RectTransform>();
        Vector2 size = rect.sizeDelta;
        if(expand)  size.y = explanationPanelMaxHeight;
        else        size.y = explanationPanelMinHeight;
        rect.sizeDelta = size;
    }

    void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f)) {
            GameObject hitObject = hit.collider.gameObject;
            allyText.gameObject.SetActive(hitObject == allyPortraitPanel);
            enemyText.gameObject.SetActive(hitObject == enemyPortraitPanel);
            bestRecordText.gameObject.SetActive(hitObject == bestRecordPCPortraitPanel);
            availablePCText.gameObject.SetActive(hitObject == availablePCPortraitPanel);
            OnBestRecordPointer(hitObject == totalScoreText);
        }
        else {
            allyText.gameObject.SetActive(false);
            enemyText.gameObject.SetActive(false);
            bestRecordText.gameObject.SetActive(false);
            availablePCText.gameObject.SetActive(false);
            OnBestRecordPointer(false);
        }
    }
}
