using Enums;
using GameData;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Language = UtilityMethods.Language;

public class LoadingScreen : MonoBehaviour{
	bool clicked;
	public bool Clicked { get { return clicked; } }
	public string afterSceneName;
	public Text loadingText;
	public Text clickAnyWhereText;
	float loadedFinishTime = -1;
	const float DURATION = 0.6f;
	const float PERIOD = 1.5f;
	const float minAlpha = 0.3f;

	public Text ChapterNumber;
	public Text ChapterTitle;
	public Text ChapterText;
	public Text CharacterName;
	public Text Tip;
	public Image Illust;
	public Image IllustBG;
	
    void Awake(){
		DontDestroyOnLoad(this);
    }

	public void Initialize(StageNum stage){
		var info = StageInfo.Stages.Find(x => x.stage == stage);
		var chapter = (int) stage / 10;
		ChapterNumber.text = Language.Select("제" + chapter + "장", "Chapter " + chapter);
		ChapterTitle.text = info.Title;
		ChapterText.text = info.Intro;
		CharacterName.text = UnitInfo.ConvertName(info.POV, true);
		//Illust.sprite = VolatileData.GetSpriteOf(SpriteType.Illust, VolatileData.GetStageData(stage, StageInfoType.POV));
		IllustBG.sprite = Illust.sprite;

		if (afterSceneName == "Battle"){
			var Tips = Parser.GetMatrixTableFrom("Data/Tips");
			var availableTips = Tips.FindAll(row => row != Tips[0] && int.Parse(row[0]) <= chapter && int.Parse(row[1]) >= chapter);
			Tip.text = Language.Select("도움말: ", "Tip: ") + availableTips[Random.Range(0, availableTips.Count)][(int) VolatileData.language + 2];	
		}else
			Tip.gameObject.SetActive(false);
	}
	
	void Update() {
		if (SceneManager.GetActiveScene().name.ToLower() == afterSceneName.ToLower()){
			if(loadedFinishTime == -1)
				loadedFinishTime = Time.time;
			loadingText.enabled = false;
			clickAnyWhereText.gameObject.SetActive(true);
			if (Input.GetMouseButtonUp(0) || VolatileData.gameMode == GameMode.AllStageTest)
				clicked = true;
		}
		float time = (Time.time - loadedFinishTime) % PERIOD;
		if(time < DURATION)
			clickAnyWhereText.color = new Color(1, 1, 1, 1 - (1 - minAlpha) * time / DURATION);
		else if(DURATION <= time && time < DURATION * 2)
			clickAnyWhereText.color = new Color(1, 1, 1, 1 - (1 - minAlpha) * Mathf.Pow((2 * DURATION - time) / DURATION, 2));
	}
}
