using Enums;
using GameData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UtilityMethods;

public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler{
	public ActiveSkill skill;
	public Image image;
    public ActionButtonType type;
    public GameObject ActionExplaination;
	public SkillViewer viewer;
	public bool interactable;
	public Image cooldownCurtain;
	public Image frameImage;
	public TextMeshProUGUI ApText;
	public UnityEvent clicked = new UnityEvent();

	private int buttonNumber;

	void Awake(){
		ApText = transform.Find("ReqAP").GetComponent<TextMeshProUGUI>();
		buttonNumber = int.Parse(gameObject.name.Substring(gameObject.name.Length-1));
		cooldownCurtain.material = new Material(cooldownCurtain.material);
	}

	void OnEnable(){
		if (BattleUIManager.Instance.ActionButtonOnOffLock) return;
		
		frameImage.enabled = true;
		ApText.text = string.Empty;
		
		var unitSkills = BattleData.turnUnit.GetActiveSkillList();
		if(unitSkills.Count > buttonNumber)
			InitializeWithSkill(unitSkills[buttonNumber]);
		else{
			skill = null;
			image.sprite = VolatileData.GetIcon(IconSprites.Standby);
			type = ActionButtonType.Standby;
			SetInteractable(true);
			cooldownCurtain.material.SetFloat("_Alpha", 0);
		}
	}

	void InitializeWithSkill(ActiveSkill newSkill){
		Unit unit = BattleData.turnUnit;
		type = ActionButtonType.Skill;
		skill = newSkill;
		image.sprite = skill.icon;

		if (unit.GetUsedSkillDict().ContainsKey(skill.GetName())) {
			cooldownCurtain.material.SetFloat("_Alpha", 0.7f);
			cooldownCurtain.material.SetInt("_Cooldown", skill.GetCooldown());
			cooldownCurtain.material.SetInt("_RemainTime", unit.GetUsedSkillDict()[skill.GetName()]);
		} else {
			cooldownCurtain.material.SetFloat("_Alpha", 0);
		}

		SetInteractable(unit.IsThisSkillUsable(skill));
		
		int actualAP = BattleData.turnUnit.GetActualRequireSkillAP(skill);
		ApText.text = _String.NumberToSprite(actualAP);
		
		if(!interactable)
			ApText.spriteAsset = BattleUIManager.redNumber;
		else if (actualAP < skill.GetRequireAP())
			ApText.spriteAsset = BattleUIManager.greenNumber;
		else
			ApText.spriteAsset = BattleUIManager.violetNumber;
	}

	public void SetInteractable(bool input){
		interactable = input;
		SetColorOrGray();
	}
	
	void SetColorOrGray(){
		if (!gameObject.activeSelf) return;
		
		if (interactable) {
			image.material = null;
			image.color = Color.white;
		} else {
			image.material = GameData.VolatileData.GetGrayScale();
			image.color = new Color(0.4f, 0.4f, 0.4f);
		}
	}

	public void SetGlowBorder(bool onoff){
		if (!gameObject.activeSelf) return;
		
		if (onoff) {
			image.material = GameData.VolatileData.GetGlowBorder ();
			image.material.color = Color.white;
		} else {
			SetColorOrGray ();
		}
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
		if(Setting.clickEnable) OnClick();
	}

	//굳이 OnPointerDown을 거쳐서 오는 건 public으로 선언해서 UIManager에서도 부를 수 있기 위함
	public void OnClick(){
		//Debug.Log("기술 버튼 OnClick");
		if (!interactable || !gameObject.activeSelf){
			SoundManager.Instance.PlaySE("Cannot");
			return;
		}
		
		SoundManager.Instance.PlaySE ("Click");
		BattleUIManager.Instance.TurnOffAllActionsGlowBorders ();

		if (type == ActionButtonType.Skill) {
			BattleManager.Instance.CallbackSkillSelect();
			BattleData.selectedSkill = skill;
			SetGlowBorder (true);
			Debug.Log(skill.GetName() + "Selected.");
		} else if (type == ActionButtonType.Collect)
			BattleManager.Instance.CallbackCollectCommand ();
		else if (image.sprite != VolatileData.GetIcon (IconSprites.Transparent))
			BattleManager.Instance.CallbackStandbyCommand ();
		clicked.Invoke ();
			
		viewer.gameObject.SetActive(false);
		ActionExplaination.SetActive(false);
	}

	void Start(){
		if(viewer.gameObject.activeSelf){
			viewer.gameObject.SetActive(false);
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData){
		var UIM = BattleUIManager.Instance;
		var TM = TileManager.Instance;
		var actor = BattleData.turnUnit;

		if (interactable)
			SetGlowBorder (true);
		
		if (skill != null) {
			UIM.ActivateSkillViewer (skill, actor);
			var firstRange = skill.GetTilesInFirstRange (actor.Pos, actor.GetDir ());
			TM.PaintTiles (firstRange, TM.TileColorMaterialForRange1);
			if (skill.GetRequireAP () != 0)
				UIM.PreviewAp (new APAction (APAction.Action.Skill, actor.GetActualRequireSkillAP (skill)));
			return;
		}
		
		if (interactable) {
			ActionExplaination.SetActive (true);
			var Text = ActionExplaination.GetComponentInChildren<Text>();
			if (type == ActionButtonType.Standby)
				Text.text = Language.Select("턴을 종료합니다.", "End Turn.");
			else if (type == ActionButtonType.Collect){
				var collectObj = BattleData.NearestCollectable;
				Text.text = "<color=#FF9999>" + Language.Select("집중", "Concentration") + "(" + collectObj.phase + ")</color> " + collectObj.Description;
			}
			
			FindObjectOfType<ShowKeywordExplain>().Show(Text.text);
		}
		viewer.gameObject.SetActive(false);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData){
		if (type != ActionButtonType.Skill || BattleData.selectedSkill != skill) {
			SetGlowBorder (false);
		}

		viewer.Initialize();
		if(type == ActionButtonType.Standby || type == ActionButtonType.Collect){
			ActionExplaination.SetActive(false);
		}else if(image.sprite.name != "transparent"){
			Debug.Assert(skill != null, gameObject.name + "'s skill is NULL!");
			BattleManager.Instance.SetProperVisualState();
			viewer.gameObject.SetActive(false);
		}
	}
}
