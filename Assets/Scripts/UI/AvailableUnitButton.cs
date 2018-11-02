using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Enums;
using GameData;
using UtilityMethods;

public class AvailableUnitButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler{
	public Image highlightImage;
	public Image standingImage;
	public Image UnitImage;
	public Image StatusIcon;
	public Text NameText;
	public Image ElementOrb;
	public RectTransform HpBar;
	
	public bool isPC;
	public bool isFixed;
	public string codeName{get { return candidate.CodeName; }}
    
    public UnitInfo unitInfo;
	public Candidate candidate;

	public BattleReadyPanel readyPanel;
	public RightScreen_BattleReady RightPanel; //오른쪽 절반 화면(RightScreen)을 담당
	ReadyManager RM;

	private void Awake (){
		ActiveHighlight(false);
		RM = ReadyManager.Instance;
		readyPanel = RM.ReadyPanel;
		RightPanel = RM.rightPanel;
	}

    // PC일 경우 이름과 unitInfo를 넣어주고, PC가 아닐 경우 unitInfo만 넣어줄 것
	/*public void Initialize(bool isPC, Candidate candidate = null, UnitInfo info = null, bool isFixed = false) {
		this.isPC = isPC;
        unitInfo = info;
		this.isFixed = isFixed;
		if(VolatileData.gameMode == GameMode.Challenge)
			this.isFixed = false;
        this.candidate = candidate;
		NameText.text = isPC ? UnitInfo.ConvertName(codeName) : info.nameKor;

		if (codeName == "unselected" || !isPC){
			standingImage.sprite = VolatileData.GetIcon(IconSprites.Transparent);
			UnitImage.sprite = VolatileData.GetSpriteOf(SpriteType.Portrait, codeName);

			var maxLevelStat = Stat.None;
			float maxLevel = -100;
			foreach (var kv in info.baseStats){
				if(kv.Key == Stat.Will) continue;
				var statLevel = Calculate.StatLevel(kv.Key, kv.Value);
				//Debug.Log(codeName + "의 " + kv.Key + " 레벨: " + statLevel);
				if (maxLevelStat != Stat.None && statLevel <= maxLevel) continue;
				maxLevelStat = kv.Key;
				maxLevel = statLevel;
			}

			if (maxLevelStat == Stat.Defense && info.baseStats[Stat.Defense] == info.baseStats[Stat.Resistance])
				StatusIcon.sprite = Resources.Load<Sprite>("Icon/Stat/durability");
			else
				StatusIcon.sprite = Resources.Load<Sprite>("Icon/Stat/" + maxLevelStat);			
		}else{
			UI.SetIllustPosition(standingImage, codeName);
			UnitImage.gameObject.SetActive(false);
			HpBar.gameObject.SetActive(false);
			StatusIcon.gameObject.SetActive(false);
			if (isFixed)
				transform.Find("FrameImage").GetComponent<Image>().color = new Color(1, 0.35f, 0.35f);
		}

		gameObject.name = codeName + "Button";

		if (unitInfo == null) return;
		HpBar.Find("Frame").GetComponent<Image>().sprite = HealthViewer.HpFrameOf(unitInfo);
		ElementOrb.sprite = HealthViewer.GetElementSprite(unitInfo);
		if (unitInfo.GetUnitClass == UnitClass.Magic){
			HpBar.anchoredPosition = new Vector2(-10, 11);
			ElementOrb.rectTransform.anchoredPosition = new Vector2(94, -12);
		}
	}*/

	public void ActiveHighlight(bool onoff = true) {
		highlightImage.enabled = onoff;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		OnClicked();
	}
	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		foreach (var unitImage in FindObjectOfType<MapBriefingManager>().unitImages.FindAll(item => _String.GeneralName(item.name) ==  _String.GeneralName(codeName)))
			unitImage.GetComponent<HighlightBorder>().HighlightWithBlackAndWhite();
	}
	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		foreach (var unitImage in FindObjectOfType<MapBriefingManager>().unitImages)
			unitImage.GetComponent<HighlightBorder>().InactiveBorder();
	}

	public void OnClicked(){
		RightPanel.unitName.text = UnitInfo.ConvertName(codeName, isPC);

		if (isPC){
			readyPanel.mainPanel.SetActive(true);
			//출전중이 아니라면
			if(RM.pickedList.All(unit => unit.CodeName != codeName)){
				SoundManager.Instance.PlaySE ("Click");
				if (VolatileData.stageData.IsTwoSideStage()){
					var faction = Utility.PCNameToFaction (codeName);
					RM.PickFaction (faction);
				}else
					RM.PickUnit(this, true);
			}
			//이미 출전 중 && 최근 선택한 유닛이면 빠짐(파티 화면 한정)
			else if(RM.RecentUnitButton == this) {
				SoundManager.Instance.PlaySE ("Cancel");
				if(VolatileData.stageData.IsTwoSideStage ())
					RM.UnPickAll();
				else
					RM.PickUnit(this, false);
			}
		}
		
		//UpdateRightPanel();
	}

    /*public void UpdateRightPanel() {
        if (RM == null) { RM = ReadyManager.Instance; }
        RM.RecentUnitButton = this;

	    if (isPC){
		    RightPanel.SetCommonUnitInfoUI(codeName);
		    readyPanel.SetAllSkillSelectButtons();
		    var classImage = ReadyManager.Instance.ReadyPanel.mainPanel.transform.Find("ClassImage");
		    Utility.SetPropertyImages(classImage.GetComponent<Image>(), classImage.Find("ElementImage").GetComponent<Image>(), new UnitInfo(codeName, true));
	    }else{
		    UIManager.Instance.PushUI(ReadyManager.Instance.detailPanel.gameObject);
		    ReadyManager.Instance.detailPanel.InitializeInReadyScene(unitInfo);
	    }
    }*/

    public void SetGray(bool gray){
		if (gray && !isFixed) {
            foreach (var image in GetComponentsInChildren<Image>())
                image.material = VolatileData.GetGrayScale();
        } else foreach (var image in GetComponentsInChildren<Image>())
                image.material = null;
    }
	public void UpdateGray(){
		SetGray(isPC && !candidate.picked);
	}
}
