using System;
using System.Linq;
using Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameData;
using UtilityMethods;

public class SkillSelectButton : SkillUI, IPointerDownHandler{
    ReadyManager RM;
    public bool selected;
    public TMPro.TextMeshProUGUI EtherText;
	public Text skillNameText;

    void Awake(){
        iconSlot = GetComponent<Image>();
    }

    void Update(){
        if(selected){
            iconSlot.color = Color.white;
            iconSlot.material = VolatileData.GetGlowBorder();
        } else{
            iconSlot.color = Color.gray;
            iconSlot.material = VolatileData.GetGrayScale();
        }
    }

    static Sprite notLearnedSkillIcon;
    static Sprite GetNotLearnedSkillIcon() {
        if(notLearnedSkillIcon == null)
            notLearnedSkillIcon = Resources.Load<Sprite>("Icon/Rose");
        return notLearnedSkillIcon;
    }

    public void Initialize(Skill inputSkill){
        RM = ReadyManager.Instance;
	    mySkill = inputSkill;
        if(inputSkill == null || inputSkill.RequireLevel > 20){
            gameObject.SetActive(false);
	        return;
        }
	    
	    EtherText.text = _String.NumberToSprite(inputSkill.ether);
        iconSlot.sprite = inputSkill.icon;

        Candidate owner = RM.candidates.Find(unit => unit.CodeName == inputSkill.ownerName);
	    transform.Find("ActiveSkillFrame").GetComponent<Image>().enabled = inputSkill is ActiveSkill; 
	    transform.Find("PassiveSkillFrame").GetComponent<Image>().enabled = inputSkill is PassiveSkill; 
		if (owner != null)
            selected = owner.selectedSkills.Any(skill => skill.korName == inputSkill.korName);
	        
	    int i = RM.StageAvailablePCTable.FindIndex(array =>
		    array[0] == ((int) VolatileData.progress.stageNumber).ToString()) -1;
	    var recentStageString = RM.StageAvailablePCTable[i][0];
	    for (; i >= 0; i--){
		    if (i == 0){
			    recentStageString = "-10";
			    break;
		    }

		    recentStageString = RM.StageAvailablePCTable[i][0];
		    if (StageData.CandidatesOfStage(RM.StageAvailablePCTable[i]).Contains(inputSkill.ownerName))
			    break;
	    }

	    transform.Find("Notification").GetComponent<Image>().enabled = inputSkill.RequireLevel > int.Parse(recentStageString) / 10;
		skillNameText.text = mySkill.Name;

		name = "SkillSelectButton(" + mySkill.Name + ")";
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData){
        Candidate owner = RM.candidates.Find(unit => unit.CodeName == mySkill.ownerName);
        if(mySkill.RequireLevel > RecordData.level || mySkill.RequireLevel == 0 || owner == null) return;
        
	    if(owner.selectedSkills.Exists(skill => skill.korName == mySkill.korName)){
		    Select(owner, false);
        }else if(owner.IsThisPickable(mySkill))
            Select(owner, true);
        
        RM.UpdateEtherText(RM.RecentUnitButton);
    }

    public void Select(Candidate owner, bool pickIn){
	    if (mySkill.RequireLevel == 0 && !pickIn) return;
	    selected = pickIn;
	    if (pickIn)
		    owner.selectedSkills.Add(mySkill);
	    else
		    owner.selectedSkills.Remove(mySkill);
    }
}
