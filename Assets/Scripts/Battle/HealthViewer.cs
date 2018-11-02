using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using GameData;
using UtilityMethods;

public class HealthViewer : MonoBehaviour { //Unit뿐 아니라 LogDisplay에서도 사용하는 기능임!
    /*height order : 
     * currentHealthBar                     ( z = -0.03 )
     * recoverBar, damageBar                ( z = -0.02 )
     * shieldBar                            ( z = -0.01 )
     * shieldRecoverBar, shieldDamageBar    ( z =  0    )
     * backgroundBar                        ( z =  0.01 )
     */
	public Unit myUnit;
	public GameObject currentHealthBar;
	public GameObject recoverBar;
	public GameObject damageBar;
    public GameObject shieldBar;
    public GameObject shieldDamageBar;
	public SpriteRenderer HpBarIcon;
	public SpriteRenderer HpBarFrame;
	public SpriteRenderer HpBarOrb;

    int currentHealth;
	int maxHealth;
    int currentShield;
    int previewHealth;
    int previewShield;

    public string GetChangeText() {
        return currentHealth + "(+" + currentShield + ") -> " + 
               previewHealth + "(+" + previewShield + ")";
    }

    public void Preview(int health, int shield, Casting casting = null) {
	    //Debug.Log("preview Shield: " + shield);
        AdjustBarScales(health, shield);

        //이하는 처치/이탈 아이콘 표시
	    if (isUI) return;
	    
	    var type = GetComponentInParent<Unit>().GetDestroyReason(casting);
	    if (type == TrigActionType.Kill){
		    HpBarIcon.sprite = _Sprite.GetIcon("UI/Kill");
		    TutorialManager.Instance.Activate("KillIcon");
	    }else if (type == TrigActionType.Retreat)
		    HpBarIcon.sprite = _Sprite.GetIcon("UI/Retreat");
	    if(BattleData.turnUnit.IsPC && myUnit.GetSide() == Side.Ally && health < currentHealth)
		    TutorialManager.Instance.Activate("FriendlyFire");
    }

	public void UpdatecurrentHealth(int currentHealth, int shieldAmount, int maxHealth) {
        currentHealth = Math.Max(currentHealth, 0);
		this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        currentShield = shieldAmount;
        AdjustBarScales(currentHealth, shieldAmount);
		if (HpBarFrame == null || GetComponentInParent<Unit>().IsHiddenUnderFogOfWar()) return;
		HpBarFrame.color = currentShield > 0 ? Color.cyan : Color.white;
	}

	private const float leftPivot = 1.16f;
	
    private void AdjustBarScales(float previewHealth, float previewShield){
        //Adjusts barScales according to parameters
        float maxBarSize = Math.Max(Math.Max(currentHealth, previewHealth) + Math.Max(previewShield, currentShield), maxHealth);
        Debug.Assert(maxBarSize > 0);

	    var BarRatioDict = new Dictionary<GameObject, float>{
		    {currentHealthBar, Math.Min(currentHealth, previewHealth) / maxBarSize},
		    {damageBar, currentHealth / maxBarSize},
		    {recoverBar, Math.Max(currentHealth, previewHealth) / maxBarSize},
		    {shieldBar, (Math.Max(currentHealth, previewHealth) + previewShield) / maxBarSize},
		    {shieldDamageBar, (Math.Max(currentHealth, previewHealth) + Math.Max(previewShield, currentShield)) / maxBarSize}
	    };

	    foreach (var kv in BarRatioDict){
		    var PosX = kv.Value * leftPivot - leftPivot;
		    kv.Key.transform.localPosition = new Vector3(PosX, 0, 0) + Vector3.forward * kv.Key.transform.localPosition.z;
	    } 
    }

    public void CancelPreview() {
        previewHealth = currentHealth;
        previewShield = currentShield;
        UpdatecurrentHealth(currentHealth, currentShield, maxHealth);
	    HpBarIcon.sprite = _Sprite.GetIcon("None");
    }

	public void SetInitHealth(int maxHealth, Unit target){
		this.maxHealth = maxHealth;
		currentHealth = maxHealth;
        previewHealth = currentHealth;
        previewShield = 0;
        currentShield = 0;

		myUnit = target;
		currentHealthBar.transform.localPosition = Vector3.zero + Vector3.forward * currentHealthBar.transform.localPosition.z;
		recoverBar.transform.localPosition = Vector3.zero + Vector3.forward * recoverBar.transform.localPosition.z;
		damageBar.transform.localPosition = Vector3.zero + Vector3.forward * damageBar.transform.localPosition.z;
		shieldBar.transform.localPosition = Vector3.zero + Vector3.forward * shieldBar.transform.localPosition.z;
		shieldDamageBar.transform.localPosition = Vector3.zero + Vector3.forward * shieldDamageBar.transform.localPosition.z;

		if(!isUI)
		    currentHealthBar.GetComponent<SpriteRenderer>().color = HpColorOfUnit(target);
        else
	        currentHealthBar.GetComponent<Image>().color = HpColorOfUnit(target);

		if(HpBarFrame == null) return;
		HpBarFrame.sprite = HpFrameOf(target);
	}

	public static Sprite HpFrameOf(Unit unit){
		var spriteAddress = "Battle/HpBar";
		if (unit.IsPC) spriteAddress += "Special";
		spriteAddress += "Melee";
		return Resources.Load<Sprite>(spriteAddress);
	}

	public static Color HpColorOfUnit(Unit unit){
		if(unit.GetSide() == Side.Ally) {
			if (unit.IsAI || unit.IsObject)
				return new Color (100f/255f, 160f/255f, 1);
			return new Color (45f/255f, 80f/255f, 200f/255f);
		}
		
		if(unit.GetSide() == Side.Neutral)
			return Color.gray;
		return new Color(255f / 255f, 130f / 255f, 0f / 255f);
	}

    readonly Color shieldBarColor = new Color(0.8f, 0.8f, 0.8f);
    readonly Color recoverBarColor = new Color(0f, 1.0f, 0.25f);
    readonly Color damageBarColor = new Color(0.7f, 0f, 0f);
    readonly Color shieldDamageBarColor = new Color(0.9f, 0.3f, 0.3f);
    
    bool isUI;
    void Awake () {
        Initialize();
    }
    public void Initialize() {
        try {
            shieldBar.GetComponent<SpriteRenderer>().color = shieldBarColor;
            recoverBar.GetComponent<SpriteRenderer>().color = recoverBarColor;
            damageBar.GetComponent<SpriteRenderer>().color = damageBarColor;
            shieldDamageBar.GetComponent<SpriteRenderer>().color = shieldDamageBarColor;
            isUI = false;
        }
        catch {
            shieldBar.GetComponent<Image>().color = shieldBarColor;
            recoverBar.GetComponent<Image>().color = recoverBarColor;
            damageBar.GetComponent<Image>().color = damageBarColor;
            shieldDamageBar.GetComponent<Image>().color = shieldDamageBarColor;
            isUI = true;
        }
    }
}
