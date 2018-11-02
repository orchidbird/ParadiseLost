namespace Battle.Skills{
	public class BaseCommonSkillLogic{
		public Skill skill;
		public virtual bool IsAuraTarget(Unit target){return true;}
	}
}
