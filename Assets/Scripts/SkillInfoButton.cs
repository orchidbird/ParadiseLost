using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillInfoButton : SkillUI{
	Text myNameText;

	public void Awake(){
		myNameText = transform.Find("SkillText").GetComponent<Text>();
		iconSlot = transform.Find("SkillImage").GetComponent<Image>();
	}

	public void Initialize(Skill newSkill, Unit owner, UnitInfo ownerInfo = null){
		mySkill = newSkill;
		if(newSkill == null)
			gameObject.SetActive(false);
		else{
			myNameText.text = mySkill.Name;
			iconSlot.sprite = mySkill.icon ?? GameData.VolatileData.GetIcon(Enums.IconSprites.Transparent);
		}
		this.owner = owner;
        this.ownerInfo = ownerInfo;
	}
}
