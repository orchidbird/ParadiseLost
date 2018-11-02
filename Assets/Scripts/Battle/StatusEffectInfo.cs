using Enums;
using System.Collections.Generic;
using UtilityMethods;

public class StatusEffectInfo{
	public string ownerOfSkill;
	public string originSkillName; // 효과를 유발하는 기술/특성의 이름
	public string displayKor; // 상태창에 표시되는 효과의 이름
	public string displayEng;
	public string DisplayName{get{return Language.Select(displayKor, displayEng);}}
	private string korText;
	private string engText;
	public string Explanation{get { return Language.Select(korText, engText); }}
	
	public bool isOnce;
	public int defaultPhase;
	public int maxStack;
	public bool amountToBeUpdated;
	public bool isRemovable;
	
	public List<StatusEffect.ActualElement> actuals = new List<StatusEffect.ActualElement>();
	
	public string GetOwnerOfSkill(){ return ownerOfSkill; }
	public string GetOriginSkillName() { return originSkillName; }

	protected void ParseCommonStatusEffectInfo(StringParser parser){
		ownerOfSkill = parser.ConsumeString();
		originSkillName = parser.ConsumeString();
		displayKor = parser.ConsumeString();
		displayEng = parser.ConsumeString();
		
		isOnce = parser.ConsumeBool();
		defaultPhase = parser.ConsumeInt();
		maxStack = parser.ConsumeInt();
		amountToBeUpdated = parser.ConsumeBool();
		isRemovable = parser.ConsumeBool();
		
		int num = parser.ConsumeInt();
		for (int i = 0; i < num; i++){
			StatusEffectType statusEffectType = parser.ConsumeEnum<StatusEffectType>();

			FormulaVarType statusEffectVar = parser.ConsumeEnum<FormulaVarType>();
			float statusEffectCoef = parser.ConsumeFloat("X", 0);
			float statusEffectBase = parser.ConsumeFloat("X", 0);

			bool isPercent = parser.ConsumeBool();
			bool isMultiply = parser.ConsumeBool("NONE", false);

			var actualElement = new StatusEffect.ActualElement(statusEffectType, 
				new Formula(statusEffectVar, statusEffectCoef, statusEffectBase), 
				isPercent, isMultiply);
			actuals.Add(actualElement);
		}
		for (int i = num; i < 3; i++) {
			for(int j = 0; j < 6; j++)
				parser.ConsumeString();
		}
		korText = _String.ColorExplainText(parser.ConsumeString());
		engText = _String.ColorExplainText(parser.ConsumeString());
	}
}

public class UnitStatusEffectInfo : StatusEffectInfo{	
	public StatusEffectCategory category;
	public UnitStatusEffectInfo(string data){
		StringParser parser = new StringParser(data, '\t');
		category = parser.ConsumeEnum<StatusEffectCategory>();
		ParseCommonStatusEffectInfo(parser);
	}
}

public class TileStatusEffectInfo : StatusEffectInfo{
	public TileStatusEffectInfo(string data){
		StringParser commaParser = new StringParser(data, '\t');
		ParseCommonStatusEffectInfo(commaParser);
	}
}
