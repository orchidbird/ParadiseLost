namespace Battle.Skills{
	public class S161_Spirit_Flash_SkillLogic : BasePassiveSkillLogic{
		public override bool IgnoreCasting(CastingApply apply, int chainCombo){
			return apply.GetSkill().IsOffensive() && chainCombo == 1;
		}
	}
}
