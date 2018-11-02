using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using GameData;
using Enums;
using UtilityMethods;

public class Glossary : MonoBehaviour{
    public DialogueManager DM;
	GlossaryType currentType = GlossaryType.Person;
	GameObject currentButton;
	public Text InformationTitle;
	public Text InformationText;
	public Image UnitPortrait;
	public List<GameObject> typeButtons;
    public List<GameObject> buttons;
	public RectTransform buttonHolder;
	public GameObject buttonPrefab;
	public GameObject ScrollBar;
	public RectTransform SlideArea;

	void OnEnable(){
		// 위치 계산용 더미 버튼들을 전부 숨김
		buttonHolder.GetComponentsInChildren<Button>().ToList().ForEach(child => child.gameObject.SetActive(false));		
		
		//새로운(아직 확인하지 않은) 항목을 찾아서 자동으로 펼침
		bool notCheckedFound = false;
		for(int i = 0; i <= (int)GlossaryType.Etc; i++){
			GlossaryType glosType = (GlossaryType)i;
			if(NotChecked(glosType) == null)
				DM.RemoveExclamationMarkFrom(typeButtons[i]);
			else{
				DM.GenerateExclamationMarkAt(typeButtons[i]);
				if (notCheckedFound) continue;
				SetCurrentType(i);
				notCheckedFound = true;
			}	
		}

		if (!notCheckedFound)
		{
			SetCurrentType(0);
		}
    }

	void SetTypeButtonImageSelected(GameObject typeButton, bool activate){
		typeButton.GetComponent<Image>().color = activate ? Color.white : Color.gray;
	}

	void SetButtonImageSelected(GameObject button, bool activate) {
		if(button != null)
			button.transform.Find("FrameImage").gameObject.SetActive(activate);
	}

	void SetScrollBar(){
		if (currentType == GlossaryType.Person){
			// type 0, '인물' 탭. 항목 13개
			SlideArea.offsetMax = new Vector2(SlideArea.offsetMax.x, 180);
			SlideArea.offsetMin = new Vector2(SlideArea.offsetMin.x, -200);
		}
		else if (currentType == GlossaryType.Etc){
			// type 3, '기타' 탭. 항목 11개
			SlideArea.offsetMax = new Vector2(SlideArea.offsetMax.x, 300);
			SlideArea.offsetMin = new Vector2(SlideArea.offsetMin.x, -300);
		}
		else{			
			// type 1, 2, 항목 < 8
			SlideArea.offsetMax = new Vector2(SlideArea.offsetMax.x, 515);
			SlideArea.offsetMin = new Vector2(SlideArea.offsetMin.x, -200);
		}

		ScrollBar.GetComponent<Scrollbar>().value = 1;
	}

	//유니티 에디터에서 버튼 참조로 호출되는 함수.
	public void SetCurrentType(int number) {
		SetTypeButtonImageSelected(typeButtons[(int)currentType], false);
		currentType = (GlossaryType)number;
		SetTypeButtonImageSelected(typeButtons[number], true);
		InitializePage();
		GlossaryData data = NotChecked(currentType);
		if (data != null) {
			SetText(data);
		} else {
			// 새로 열린 항목이 없으면 열려있는 것 중에 첫번째
			data = GlosDataOf(currentType)
				.FindAll(glosData => glosData != null && glosData.level > 0)
				.OrderBy(glosData => glosData.index)
				.FirstOrDefault();

			if (data != null)
				SetText(data);
			else
				SetButtonImageSelected(currentButton, false);
		}
		
		// 스크롤바 세팅
		SetScrollBar();
	}

	GlossaryData NotChecked(GlossaryType glosType){
		return GlosDataOf(glosType).Find(glos => glos.isNew);
	}

	IEnumerator CheckThisTypeHasSomethingNew(){
		DM.RemoveExclamationMarkFrom(typeButtons[(int)currentType]);
		yield return null;
		if(NotChecked(currentType) != null){
			DM.GenerateExclamationMarkAt(typeButtons[(int) currentType]);
		}
	}

	// 오른쪽 내용 초기화
    void InitializePage(){
	    MakeButtons();
	    UnitPortrait.enabled = false;
	    InformationText.rectTransform.offsetMax = new Vector2(-50, InformationText.rectTransform.offsetMax.y);
		InformationTitle.text = "";
		InformationText.text = "";	
    }
	
	public void SetText(GlossaryData glosData){
		if (glosData == null || glosData.level <= 0) return;

		var clickedButton = buttons[glosData.index];
		
		if(currentButton != null)
			SetButtonImageSelected(currentButton, false);
		currentButton = clickedButton;
		SetButtonImageSelected(currentButton, true);

		glosData.isNew = false;
		InformationTitle.text = currentButton.GetComponentInChildren<Text>().text;
		InformationText.text = glosData.TextDict [glosData.level];
		DM.RemoveExclamationMarkFrom(currentButton);
		StartCoroutine(CheckThisTypeHasSomethingNew());
		
		// 인물에 해당하는 이미지가 있을 경우에만 사전 UI가 달라진다. 인물이어도 이미지가 없으면 글로 채워진다.
		if (currentType == GlossaryType.Person) {
			var nameDataRow = UnitInfo.unitNameTable.Find(row => row.Contains(glosData.Name));
			if (nameDataRow != null)
			{
				UnitPortrait.enabled = true;
				UnitPortrait.sprite = VolatileData.GetSpriteOf(SpriteType.Illust, nameDataRow[0]);
				InformationText.rectTransform.offsetMax = new Vector2(-220, InformationText.rectTransform.offsetMax.y);

				return;
			}
		}

		UnitPortrait.enabled = false;
		InformationText.rectTransform.offsetMax = new Vector2(-50, InformationText.rectTransform.offsetMax.y);
	}

	public void MakeButtons()
	{
		var currentTypeAllGlossaryData = GlosDataOf(currentType);
		buttons.ForEach(Destroy);
		buttons.Clear();
		currentTypeAllGlossaryData.ForEach(glosData =>
		{
			var button = Instantiate(buttonPrefab, buttonHolder);
			
			if (glosData != null && glosData.level > 0) {
				button.GetComponentInChildren<Text>().text = glosData.Name;
				if(glosData.isNew) {
					DM.GenerateExclamationMarkAt(button);
				}
				else DM.RemoveExclamationMarkFrom(button);
			} else {
				button.GetComponentInChildren<Text>().text = string.Empty;
				DM.RemoveExclamationMarkFrom(button);
			}
			
			button.GetComponent<Button>().onClick.AddListener(delegate {SetText(glosData);});
			
			buttons.Add(button);
		});
	}

	List<GlossaryData> GlosDataOf(GlossaryType glosType){
        return GlobalData.GlossaryDataList.FindAll(data => data.Type == glosType);
    }
}


public class GlossaryData{
	public GlossaryType Type;
	public int index;
	public int level;
	public string nameKor;
	public string nameEng;
    public bool isNew;
	public Dictionary<int, string> textKor = new Dictionary<int, string>();
	public Dictionary<int, string> textEng = new Dictionary<int, string>();
	
	public string Name{get { return Language.Select(nameKor, nameEng); }}
	public Dictionary<int, string> TextDict{get { return VolatileData.language == Lang.Kor ? textKor : textEng; }}
	public GlossaryData(string rowDataKor, string rowDataEng){
		StringParser parser = new StringParser(rowDataKor, '\t');
		Type = parser.ConsumeEnum<GlossaryType>();
		index = parser.ConsumeInt();
		level = 0;
		nameKor = parser.ConsumeString();
		int newLevel = 0;

		while(parser.Remain > 0){
			string levelText = parser.ConsumeString();
			if(levelText == "") break;
			newLevel += 1;
			textKor.Add(newLevel, levelText);
		}

		parser = new StringParser(rowDataEng, '\t') {index = 2};
		nameEng = parser.ConsumeString();
		newLevel = 0;
		while(parser.Remain > 0){
			string levelText = parser.ConsumeString();
			if(levelText == "") break;
			newLevel += 1;
			textEng.Add(newLevel, levelText);
		}
	}
}
