using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using GameData;
using Enums;
using Save;
using UnityEngine.SceneManagement;
using UtilityMethods;
using Language = UtilityMethods.Language;

public class SaveSlotPanel : MonoBehaviour {
    public List<Button> slots;
	public GameObject DifficultyPanel;
    List<bool> isEmpty = new List<bool>();
    string mode = "";
    int selectedSlotNumber;

    public void OnEnable() {
        Initialize();
    }
	
    public void Initialize() {
        for (int i = 0; i < slots.Count; i++) {
            isEmpty.Add(true);
            UpdateSlot(i);
        }
    }

    public void UpdateSlot(int i) {
        Text text = slots[i].GetComponentInChildren<Text>();
        string filePath = FilePath(i);
        if (!File.Exists(filePath)) {
            text.text = Language.Select("새로 시작하기", "Click To Start");
            isEmpty[i] = true;
        } else {
	        string[] data = File.ReadAllLines(filePath, Encoding.UTF8);
			string name = data[0];

	        if (name == "Test")
			    text.text = Language.Select("테스트", "Test");
		    else if (name.StartsWith("Stage")){
		        var chapter = name.Substring(6, name.Length - 7);
		        text.text = Language.Select("제" + chapter + "장 전투", "Chapter " + chapter + " Battle");
	        }else
			    text.text = Language.Select("장면 " + name.Substring(6), name);

			float playingTime = float.Parse(data[3]);
			
			Difficulty difficulty = SaveFile.GetDifficultyFromSaveFileString(data);
			text.text += "\n" + ConvertSecondsToDDHHMMSS((int)playingTime) + " / " + _String.FromDifficulty(difficulty);

            isEmpty[i] = false;
        }
    }

	string ConvertSecondsToDDHHMMSS(int second) {
		int SS = second % 60;
		int MM = (second / 60) % 60;
		int HH = (second / 3600) % 24;
		int DD = (second / 86400);
		string result = "";
		if(DD != 0)
			result += DD + Language.Select("일 ", "d ");
		if(HH != 0)
			result += HH + Language.Select("시간 ", "h ");
		if(MM != 0)
			result += MM + Language.Select("분 ", "m ");
		if(SS != 0)
			result += SS + Language.Select("초 ", "s ");
		return /*Language.Select("플레이한 시간: ", "Playtime: ") + */result;
	}

	string FilePath(int i){
		return SaveDataPath.GetPathWithIndex(i);
	}

	//이하 문단은 Unity에서 참조 중이라서 Ref가 없음.
    public void OnSlotClicked(int i){
	    if (isEmpty[i]){
		    if(SceneManager.GetActiveScene().name == "Title")
		    	ShowDifficultyPanel(i);
		    else{
			    GameDataManager.SaveAt(i, RecordData.progress.ToSaveString());
			    UpdateSlot(i);
		    }
	    }else{
	        GameDataManager.Load(i);
	        VolatileData.gameMode = GameMode.Story;
	        RecordData.alreadyReadDialogues.Clear();


	        if (RecordData.progress.isDialogue){
		        var dialogueName = RecordData.progress.dialogueName;
		        if (dialogueName.EndsWith("-1") && !dialogueName.EndsWith("1-1"))
			        StartCoroutine(FindObjectOfType<SceneLoader>().LoadScene("Era", true));
			    else
			        FindObjectOfType<SceneLoader>().LoadNextDialogueScene(dialogueName);
	        }else
		        FindObjectOfType<SceneLoader>().LoadNextBattleScene();
        }
    }
	
    public void WarnDelete(int slotNumber){
	    if (isEmpty[slotNumber]){
		    SoundManager.Instance.PlaySE("Cancel");
		    return;
	    }
	    
	    selectedSlotNumber = slotNumber;
	    FindObjectOfType<UIManager>().Warn(DeleteSave, Language.Select("정말 삭제하시겠습니까?", "Delete Saved file?"));
    }
    void DeleteSave(){
        File.Delete(FilePath(selectedSlotNumber));
		GameDataManager.RemoveEntirePlayLog(selectedSlotNumber);
		UpdateSlot(selectedSlotNumber);
    }

	void ShowDifficultyPanel(int slotNumber){
		selectedSlotNumber = slotNumber;
		FindObjectOfType<UIManager>().PushUI(DifficultyPanel);
    }

	public void SelectDifficulty(int input){
		VolatileData.SetDifficulty(input);
		GameDataManager.Reset(selectedSlotNumber);
		FindObjectOfType<SceneLoader>().LoadNextDialogueScene(RecordData.progress.dialogueName);
	}
}
