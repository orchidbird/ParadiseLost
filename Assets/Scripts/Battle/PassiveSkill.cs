using Battle.Skills;

public class PassiveSkill : Skill{
	public BasePassiveSkillLogic SkillLogic {
		get { return SkillLogicFactory.Get (this); }
	}

	public PassiveSkill(string skillData){
		StringParser commaParser = new StringParser(skillData, '\t');
		
		GetCommonSkillData(commaParser);
        GetCommonSkillExplanationText(commaParser);
	}
}
