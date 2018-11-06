using Enums;
using System;
using System.Collections.Generic;
using Battle.Damage;
using Battle.Skills;
using GameData;
using UnityEngine;
using UtilityMethods;
using Convert = System.Convert;

public class Skill{
    //기본 정보
    public string ownerName;
	public Unit owner;
    public int ether;
	public string address;
	public string required;
	public string korName;
	public string engName;
	public string Name{get { return Language.Select(korName, engName); }}
	public int RequireLevel{get { return int.Parse(address.Substring(1)); }}

    //유저에게 보여질 설명 텍스트 구성
    public string explainKor;
	public string explainEng;
	public string Explaination{get { return Language.Select(explainKor, explainEng); }}

	public List<Formula> textValueFormulas = new List<Formula>();
    public Sprite icon;

	public int intTemp = 0;

    public List<UnitStatusEffectInfo> unitStatusEffectList = new List<UnitStatusEffectInfo>();
	public List<TileStatusEffectInfo> tileStatusEffectList = new List<TileStatusEffectInfo>();
	
    //기술,특성의 공통되는 부분을 받아온다
    public void GetCommonSkillData(StringParser parser){
        ownerName = parser.ConsumeString();
        ether = parser.ConsumeInt();
	    address = parser.ConsumeString();
	    required = parser.ConsumeString();
		korName = parser.ConsumeString();
	    engName = parser.ConsumeString();
        icon = Resources.Load<Sprite> ("Icon/Skill/" + _String.GeneralName(ownerName) + "/" + engName) ?? VolatileData.GetIcon(IconSprites.Transparent);
	    
	    ApplyUnitStatusEffectList();
	    ApplyTileStatusEffectList();
    }

    public void GetCommonSkillExplanationText(StringParser parser) {
	    explainKor = parser.ConsumeString();
	    explainEng = parser.ConsumeString();

	    if (this is ActiveSkill){
		    ActiveSkill skill = (ActiveSkill)this;
		    string powerFactorString = skill.powerFactor.ToString();
		    if (powerFactorString == "1") powerFactorString = "1.0";

		    if (skill.GetSkillApplyType() == SkillApplyType.DamageHealth){
			    explainKor = explainKor.Replace ("DEFAULT", "피해량 " + powerFactorString + "×공격력");
			    explainEng = explainEng.Replace ("DEFAULT", "Damage " + powerFactorString + "×Power");   
		    }else if (skill.GetSkillApplyType() == SkillApplyType.HealHealth){
			    explainKor = explainKor.Replace ("DEFAULT", "회복량 " + powerFactorString + "×공격력");
			    explainEng = explainEng.Replace ("DEFAULT", "Healing " + powerFactorString + "×Power");
		    }
	    }
	    
	    explainKor = _String.ColorExplainText(explainKor);
	    explainEng = _String.ColorExplainText(explainEng);

	    if (parser.Remain <= 0) return;	    
	    string numValuesString = parser.ConsumeString();
	    if (numValuesString == string.Empty) return;
	    int numValues = int.Parse(numValuesString);
	    for (int i = 0; i < numValues; i++)
		    textValueFormulas.Add (new Formula (parser.ConsumeEnum<FormulaVarType> (), parser.ConsumeFloat (), parser.ConsumeFloat ()));
    }

    public static Skill Find(List<Skill> list, string owner, string location){
        return list.Find(skill => skill.ownerName == owner && skill.address == location);
    }
	public Skill RequiredSkill{get { return Find(TableData.AllSkills, ownerName, required); }}
	
	void ApplyUnitStatusEffectList(){
		unitStatusEffectList.Clear();
		foreach (var SEInfo in StatusEffector.USEInfoList) {
			if(SEInfo.GetOriginSkillName().Equals(korName) && SEInfo.GetOwnerOfSkill() == ownerName 
			    && !unitStatusEffectList.Contains(SEInfo)) {    //같은 스킬을 가진 유닛이 여러 개일 때 중복으로 들어가는 것 방지
				unitStatusEffectList.Add(SEInfo);
			}
		}
	}
	void ApplyTileStatusEffectList(){
		tileStatusEffectList.Clear();
		foreach (var SEInfo in StatusEffector.TSEInfoList) {
			if(SEInfo.GetOriginSkillName().Equals(korName)
			    && !tileStatusEffectList.Contains(SEInfo)){
				tileStatusEffectList.Add(SEInfo);
			}
		}
	}
}
