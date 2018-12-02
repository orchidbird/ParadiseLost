using Enums;
using TMPro;
using BattleUI;
using GameData;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UtilityMethods;
using System.Collections;
using System.Collections.Generic;
using Image = UnityEngine.UI.Image;
using Language = UtilityMethods.Language;

public class BattleUIManager : UIManager{
	private static BattleUIManager instance;
	public static BattleUIManager Instance{
		get { return instance; }
	}
	
	public bool startFinished;

    public GameObject battleUICanvas;
    public GameObject movedUICanvas;
    public APBarPanel UpperUI;
	public SkillViewer skillViewer;
	public UnitViewer unitViewerUI;
	public UnitViewer selectedUnitViewerUI;
	public TileViewer tileViewerUI;
	public SelectDirectionUI selectDirectionUI;
    public GameObject skillNamePanelUI;
    public GameObject phaseUI;
    public DetailInfoPanel detailInfoUI;
	public GameObject sePanelPrefab;
	public GameObject sePanelList;
    public StatusEffectDisplayPanel statusEffectDisplayPanel;
    public GameObject changeDisplayPanel;
    public LogDisplayPanel logDisplayPanel;
    public ConditionPanel conditionPanel;
	public GameObject smallConditionPanel;
    public GameObject menuPanel;
    public ResultPanel resultPanel;
	public Text turnUnitApText;
	
    // 화면 우상단에서 펼치고 접을 수 있는 승리/패배 조건 텍스트들
    public List<ConditionText> smallConditionTexts = new List<ConditionText>();

	GameObject notImplementedDebugPanel;
    public GameObject actionButtonsParent;
	public List<ActionButton> actionButtons = new List<ActionButton>();

	//미리 Load해서 사용
	public static TMP_SpriteAsset violetNumber;
	public static TMP_SpriteAsset redNumber;
	public static TMP_SpriteAsset greenNumber;
	
	void Awake(){
		instance = this;
        selectDirectionUI.Initialize();
        TutorialScenario.selectDirectionUI = selectDirectionUI;

		violetNumber = Resources.Load<TMP_SpriteAsset>("Battle/DamageAndHealFont/NumberViolet");
		redNumber = Resources.Load<TMP_SpriteAsset>("Battle/DamageAndHealFont/NumberRed");
		greenNumber = Resources.Load<TMP_SpriteAsset>("Battle/DamageAndHealFont/NumberGreen");
	}

	void Start(){
		startFinished = true;
        InitializeUIsDisplacements();
	}

	public override void Update(){
		if (!Setting.shortcutEnable) return;
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			actionButtons[0].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha2)){
			actionButtons[1].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha3)){
			actionButtons[2].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha4)){
			actionButtons[3].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha5)){
			actionButtons[4].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha6)){
			actionButtons[5].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha7)){
			actionButtons[6].OnClick();
		}if(Input.GetKeyDown(KeyCode.Alpha8)){
			actionButtons[7].OnClick();
		}if(Input.GetKeyDown(KeyCode.Q) && BattleData.turnUnit != null){
			actionButtons[BattleData.turnUnit.GetActiveSkillList().Count].OnClick();
		}if (Input.GetKeyDown(KeyCode.Escape) && DeactivateTopUI() == null)
			BattleManager.Instance.triggers.rightClicked.Trigger();
		
		for (int i = sePanelList.transform.childCount - 1; i >= 0; i--)
			sePanelList.transform.GetChild(i).gameObject.SetActive(!Input.GetKey(KeyCode.LeftShift));
	}

    public void ActivateAPBarUI(bool activate) {
        UpperUI.gameObject.SetActive(activate);
    }
	public void UpdateApBarUI(){
		if (UnitManager.Instance.unitsActThisPhase.Count > 0 && BattleData.turnUnit != null)
			UpperUI.UpdateAPDisplay(UnitManager.GetAllUnits());	
	}

	//"ShowProper~~" 기능은 PC 차례에만 사용할 것!
	public void ShowProperPreviewAp(){
		if(BattleData.currentState == CurrentState.FocusToUnit)
			PreviewAp(null);
		else if(BattleData.currentState == CurrentState.SelectSkillApplyArea)
			PreviewAp(new APAction(APAction.Action.Skill, BattleData.turnUnit.GetActualRequireSkillAP(BattleData.selectedSkill)));
	}
	public void ShowOrHideDirectionUI(){
		SetDirectionUIActive(BattleData.currentState == CurrentState.SelectSkillApplyArea && BattleData.selectedSkill.GetRangeType() != RangeType.Point);
	}

	public void PreviewAp(APAction action){
		BattleData.previewAPAction = action;
		UpdateApBarUI();

		if(action != null) selectedUnitViewerUI.PreviewAp(action.requiredAP);
		else selectedUnitViewerUI.OffPreviewAp();
	}

	public void TurnOnOnlyOneAction(int skillIndex){
		for (int i = 0; i < 8; i++)
			actionButtons[i].SetInteractable(i == skillIndex);
		ActionButtonOnOffLock = true;
	}
	public void TurnOffAllActions(){
		for (int i = 0; i < 8; i++) {
			actionButtons [i].gameObject.SetActive(false);
		}
	}

	public void TurnOffAllActionsGlowBorders(){
		for (int i = 0; i < 8; i++) {
			actionButtons [i].SetGlowBorder (false);
		}
	}

	public bool ActionButtonOnOffLock;

	public void ControlListenerOfActionButton(int i, bool onOff, UnityAction action){
		if(onOff){
			actionButtons[i].clicked.AddListener(action);
		}else{
			actionButtons[i].clicked.RemoveListener(action);
		}
	}

	public void ActivateSkillViewer(Skill skill, Unit caster = null, UnitInfo casterInfo = null){
		skillViewer.gameObject.SetActive(true);
        skillViewer.UpdateSkillViewer(skill, caster);
	}

	public void DeactivateSkillUI(){
		skillViewer.gameObject.SetActive(false);
	}

	public IEnumerator MovePhaseUI(int currentPhase){
		Image img1 = phaseUI.GetComponent<Image>();
		Image img2 = phaseUI.transform.Find("AdditionalPanel").gameObject.GetComponent<Image>();
		Image img3 = phaseUI.transform.parent.Find("Sword").gameObject.GetComponent<Image>();

		phaseUI.transform.localPosition = new Vector3(-1280,0,0);
		phaseUI.transform.Find("Text").GetComponent<Text>().text = Language.Select("페 이 즈  ", "P h a s e  ") + currentPhase;
		img1.DOFade(1, 0.5f);
		img2.DOFade(1, 0.5f);
		img3.DOFade(1, 0.5f);
		img3.GetComponent<RectTransform>().localPosition = new Vector3(200, 400, 0);
		iTween.MoveTo(phaseUI, iTween.Hash("position", new Vector3(0,0,0), "islocal", true, "time", 1));
		iTween.MoveTo(img3.gameObject, iTween.Hash("position", new Vector3(0,0,0), "islocal", true, "time", 1));
		yield return new WaitForSeconds(1f);
		img1.DOFade(0, 0.5f);
		img2.DOFade(0, 0.5f);
		img3.DOFade(0, 0.5f);
		iTween.MoveTo(phaseUI, iTween.Hash("position", new Vector3(1280,0,0), "islocal", true, "time", 1));
		iTween.MoveTo(img3.gameObject, iTween.Hash("position", new Vector3(-200,-400,0), "islocal", true, "time", 1));
		yield return null;
	}
	
	public void ReleaseViewerHold(){
		BattleData.tileInTileViewer = null;
		BattleData.unitInSelectedUnitViewer = null;
		SetUnitViewerRaycast(false);
	}

	public void SetUnitViewerRaycast(bool onoff){
		foreach (var image in unitViewerUI.GetComponentsInChildren<Image>()){
			image.raycastTarget = onoff;
		}
	}

	public void UpdateUnitViewer(Unit unitOnTile){
		unitViewerUI.gameObject.SetActive(true);
		unitViewerUI.UpdateUnitViewer(unitOnTile);
		DeactivateSePanelList();
		ActivateSePanelList(unitOnTile);
	}
	void ActivateSePanelList(Unit unit){
		var seList = unit.VaildStatusEffects;
		//Debug.Log(unit.CodeName + "의 효과: " + seList.Count + " / " + unit.statusEffectList.Count);
		//if(unit.CodeName == "sepia") Debug.Log("세피아 보호막: " + unit.statusEffectList[0].GetAmount(0));
		for (int i = 0; i < seList.Count; i++){
			GameObject sePanel = Instantiate(sePanelPrefab, sePanelList.transform.Find("Line" + i % 2));
			sePanel.transform.Rotate(0, 0, 180f);
			sePanel.GetComponent<StatusEffectDisplayPanel>().SetText(seList[i]);
		}

		var maxWidth = 260;
		UI.SetHorizontalFit(sePanelList.transform.Find("Line1").GetComponent<RectTransform>(), maxWidth);
		UI.SetHorizontalFit(sePanelList.transform.Find("Line0").GetComponent<RectTransform>(), maxWidth);
	}

	public void DisableUnitViewer() {
		DeactivateSePanelList();
        unitViewerUI.ClearStatusEffectIconList();
        unitViewerUI.gameObject.SetActive(false);
	}

	public void SetSelectedUnitViewerUI(Unit selectedUnit){
		if (selectedUnit == null) return;
		
		selectedUnitViewerUI.gameObject.SetActive(true);
		selectedUnitViewerUI.UpdateUnitViewer(selectedUnit);
	}
	public void UpdateSelectedUnitViewerUI(Unit selectedUnit){
		selectedUnitViewerUI.UpdateUnitViewer(selectedUnit);
	}

	public void DisableSelectedUnitViewerUI() {
        unitViewerUI.ClearStatusEffectIconList();
        selectedUnitViewerUI.gameObject.SetActive(false);
	}

	void DeactivateSePanelList(){
		for(int i = sePanelList.transform.childCount-1 ; i >= 0 ; i--)
			UI.DestroyAllChildren(sePanelList.transform.GetChild(i));
	}

    public void ActivateStatusEffectDisplayPanelAndSetText(RectPosition rectPosition, StatusEffect statusEffect) {
        statusEffectDisplayPanel.gameObject.SetActive(true);
        rectPosition.PlaceRect(statusEffectDisplayPanel.GetComponent<RectTransform>());
        statusEffectDisplayPanel.SetText(statusEffect);
    }

    public void DeactivateStatusEffectDisplayPanel() {
        statusEffectDisplayPanel.gameObject.SetActive(false);
    }

    public void ActivateChangeDisplayPanel(Vector3 pivot, string str) {
        changeDisplayPanel.SetActive(true);
        changeDisplayPanel.transform.position = pivot;
        changeDisplayPanel.GetComponentInChildren<Text>().text = str;
    }

    public void DeactivateChangeDisplayPanel() {
        changeDisplayPanel.SetActive(false);
    } 

    public IEnumerator MoveConditionText(){
        float time = 0;
        const float duration = 0.8f;
        const float bigFontDefaultSize = 30;
        const float smallFontDefaultSize = 24;
        float finalSizeRelative = Setting.smallConditionTextSize;
	    RectTransform winRect = conditionPanel.winConditions.GetComponent<RectTransform>();
        RectTransform loseRect = conditionPanel.loseConditions.GetComponent<RectTransform>();
        List<RectTransform> rects = new List<RectTransform> { winRect, loseRect };
        
		Vector3 dest = smallConditionPanel.GetComponent<RectTransform>().anchoredPosition;
        List<Vector3> destPositions = new List<Vector3> { dest, dest - new Vector3(0, winRect.sizeDelta.y * finalSizeRelative, 0) };
	    var originRect = conditionPanel.GetComponent<RectTransform>();
	    Vector3 moveDirection = smallConditionPanel.GetComponent<RectTransform>().anchoredPosition -
	                            originRect.anchoredPosition + new Vector2(0, originRect.rect.height / 3 * 2);
	    
        float initAlpha = rects[0].GetComponentInChildren<Image>().color.a;

	    // index 0은 "Title"이므로 제외
	    for(int i = 1; i < winRect.childCount; i++) {
		    ConditionText conditionText = winRect.GetChild(i).GetComponent<ConditionText>();
		    conditionText.trigger = conditionPanel.winConditions.transform.GetChild(i).GetComponent<ConditionText>().trigger;
		    if(conditionText.trigger != null)
			    smallConditionTexts.Add(conditionText);
	    }
	    for (int i = 1; i < loseRect.childCount; i++) {
		    ConditionText conditionText = loseRect.GetChild(i).GetComponent<ConditionText>();
		    conditionText.trigger = conditionPanel.loseConditions.transform.GetChild(i).GetComponent<ConditionText>().trigger;
		    if(conditionText.trigger != null)
			    smallConditionTexts.Add(conditionText);
	    }
	    
        float bigFontSize = bigFontDefaultSize;
        float smallFontSize = smallFontDefaultSize;
        yield return new WaitForSeconds(0.3f);
        while(true) {
            time += Time.deltaTime;
            float x = 2 * time * Time.deltaTime / (duration * duration);
            if(time > duration) break;
            bigFontSize -= bigFontDefaultSize * x * (1 - finalSizeRelative);
            smallFontSize -= smallFontDefaultSize * x * (1 - finalSizeRelative);
	        originRect.anchoredPosition3D += moveDirection * x;
	        foreach (var text in originRect.GetComponentsInChildren<Text>()) {
		        if (text.gameObject.name == "Title")
			        text.fontSize = (int)bigFontSize;
		        else
			        text.fontSize = (int)smallFontSize;
	        }
	        foreach (var image in originRect.GetComponentsInChildren<Image>()) {
		        Color color = image.color;
		        color.a -= initAlpha * x;
		        image.color = color;
	        }
            yield return null;
        }
        for(int i = 0; i < 2; i++) {
            rects[i].anchoredPosition3D = destPositions[i];
	        rects[i].GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            foreach(var text in rects[i].GetComponentsInChildren<Text>()) {
                if(text.gameObject.name == "Title")
                    text.fontSize = (int)(bigFontDefaultSize * finalSizeRelative);
                else
	                text.fontSize = (int)(smallFontDefaultSize * finalSizeRelative);
			}
			foreach (var image in rects[i].GetComponentsInChildren<Image>()) {
				Color color = image.color;
				color.a = 0;
				image.color = color;
			}
			
	        rects[i].SetParent(smallConditionPanel.transform);
	        rects[i].GetComponent<VerticalLayoutGroup>().padding.left = 0;
	        rects[i].GetComponent<VerticalLayoutGroup>().padding.right = 0;
	        rects[i].GetComponent<VerticalLayoutGroup>().padding.top = 0;
	        rects[i].GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
	        rects[i].GetComponent<VerticalLayoutGroup>().childControlWidth = true;
        }

	    DeactivateTopUI();
    }

    public void UpdateSmallConditionTexts(){
        foreach(var conditionText in smallConditionTexts)
            ConditionPanel.UpdateTriggerText(conditionText);
    }

    public void ActivateMenuPanel(){
	    PushUI(menuPanel);
	    menuPanel.transform.Find("Buttons/Difficulty/Text").GetComponent<Text>().text =
		    Language.Select("난이도 변경\n(현재 ", "Change difficulty\n(Now ") + _String.FromDifficulty(VolatileData.difficulty) + ")"; 
    }

	public void SetTileViewer(Tile tile){
		tileViewerUI.gameObject.SetActive(true);
		FindObjectOfType<TileViewer>().UpdateTileViewer(tile);
	}

	public void DisableTileViewerUI(){
		tileViewerUI.RefreshStatusEffectIconList();
		if(tileViewerUI != null){
        	tileViewerUI.gameObject.SetActive(false);
		}
	}

	public void SetDirectionUIActive(bool OnOff){
		selectDirectionUI.gameObject.SetActive(OnOff);
	}

	public void HideSkillNamePanelUI()
	{
		skillNamePanelUI.SetActive(false);
		skillNamePanelUI.GetComponentInChildren<Text>().text = "";
	}

	public void SetSkillNamePanelUI(string skillName){
		skillNamePanelUI.SetActive(true);
		skillNamePanelUI.GetComponentInChildren<Text>().text = skillName;
	}

	public void SetMovedUICanvasOnUnitAsCenter(Unit unit)
	{
		SetMovedUICanvasOnObjectAsCenter (unit);
	}
	public void SetMovedUICanvasOnTileAsCenter(Tile tile)
	{
		SetMovedUICanvasOnObjectAsCenter (tile);
	}
	private void SetMovedUICanvasOnObjectAsCenter(MonoBehaviour obj){
		if (obj == null) return;
		Vector2 position = obj.gameObject.transform.position;
		SetMovedUICanvasOnCenter (position);
	}
	private void SetMovedUICanvasOnCenter(Vector2 position){
		Vector3 newPosition = (new Vector3(position.x, position.y, -8));
		movedUICanvas.transform.position = newPosition;
	}

	public UnityEvent activateDetailInfoEvent = new UnityEvent ();
	public UnityEvent deactivateDetailInfoEvent = new UnityEvent ();

	public void ActivateDetailInfoUI(Unit unit){
		if(BattleData.detailInfoLock) return;
		if (BattleData.activateDetailInfoUnit != null && BattleData.activateDetailInfoUnit != unit) return;
		activateDetailInfoEvent.Invoke();
		UIStack.Push(detailInfoUI.gameObject);
		detailInfoUI.unit = unit;
		detailInfoUI.InitializeInBattleScene();
	}
    
	public bool isDetailInfoUIActive(){
		return detailInfoUI.gameObject.activeSelf;
    }

    public void AppendNotImplementedLog(string text){
		if (notImplementedDebugPanel == null){
			Debug.LogError("Cannot find not implemented debug panel\n" + text);
			return;
		}

		var debugPanel = notImplementedDebugPanel.GetComponent<NotImplementedDebugPanel>();
		debugPanel.Append(text);
	}

    public Vector2 GetActionButtonPosition(int i) {
        return actionButtons[i].transform.position;
    }
	
	public void HideActionButtons(){
		foreach (var button in actionButtons){
			button.gameObject.SetActive(false);
		}
	}
	
	public void SetActionButtons(){
		HideActionButtons();
		for (int i = 0; i < 8; i++)
			actionButtons[i].gameObject.SetActive(i <= BattleData.turnUnit.GetActiveSkillList().Count);
		UnitManager.Instance.CheckCollectableObjects();
	}
    public void AddCollectableActionButton(){
        var button = actionButtons[BattleData.turnUnit.GetActiveSkillList().Count + 1];
	    Debug.Log(button.gameObject.name);
	    button.gameObject.SetActive(true);		   
		button.image.sprite = VolatileData.GetIcon (IconSprites.Collect);
        button.type = ActionButtonType.Collect;
    }

	public void DeactivateNonSkillButtons(){ // 스킬범위 선택 중엔 다른 스킬로 교체 가능하나 대기/수집은 선택 불가
		for (int i = 0; i < 8; i++) {
			if (actionButtons [i].type != ActionButtonType.Skill) {
				actionButtons [i].gameObject.SetActive(false);
			}
		}
	}

    Dictionary<RectTransform, Vector3> UIInPositions = new Dictionary<RectTransform, Vector3>();
    Dictionary<RectTransform, Vector3> UIOutPositions = new Dictionary<RectTransform, Vector3>();

    void InitializeUIsDisplacements() {
        UIInPositions.Add(UpperUI.GetComponent<RectTransform>(), new Vector3(0, 0, 0)); //(0, 170)만큼 이동
        UIInPositions.Add(tileViewerUI.GetComponent<RectTransform>(), new Vector3(-5, 150, 0));
        UIInPositions.Add(selectedUnitViewerUI.GetComponent<RectTransform>(), new Vector3(5, 5, 0));
        UIInPositions.Add(unitViewerUI.GetComponent<RectTransform>(), new Vector3(-5, 0, 0));
        UIInPositions.Add(actionButtonsParent.GetComponent<RectTransform>(), new Vector3(0, 90, 0));

        UIOutPositions.Add(UpperUI.GetComponent<RectTransform>(), new Vector3(0, 170, 0));
        UIOutPositions.Add(tileViewerUI.GetComponent<RectTransform>(), new Vector3(240, 200, 0));
        UIOutPositions.Add(selectedUnitViewerUI.GetComponent<RectTransform>(), new Vector3(-538, 5, 0));
        UIOutPositions.Add(unitViewerUI.GetComponent<RectTransform>(), new Vector3(529, 0, 0));
        UIOutPositions.Add(actionButtonsParent.GetComponent<RectTransform>(), new Vector3(0, -30, 0));
	}

    public void SlideUIsOut(float duration) {
        foreach(var kv in UIOutPositions)
            StartCoroutine(UI.SlideRect(kv.Key, kv.Value, duration));
	    if (UpperUI.transform.GetComponentInChildren<TogglePosition>().opened)
		    StartCoroutine(UpperUI.transform.GetComponentInChildren<TogglePosition>().Move());
    }
    public void SlideUIsIn(float duration) {
        foreach (var kv in UIInPositions) {
            StartCoroutine(UI.SlideRect(kv.Key, kv.Value, duration));
        }
    }
}
