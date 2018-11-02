namespace Battle.Skills {
    class Darkenir_1_l_SkillLogic : BasePassiveSkillLogic {
        public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target){
	        return statusEffect.Duration() != 1 || !statusEffect.IsDebuff;
        }
    }
}
