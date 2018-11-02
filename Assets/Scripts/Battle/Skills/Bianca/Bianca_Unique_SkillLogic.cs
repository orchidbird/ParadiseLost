namespace Battle.Skills {
    class Bianca_Unique_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerOnSteppingTrap(Unit caster, TileStatusEffect trap){
	        return trap.GetCaster () != caster;
        }
		public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target){
			return !statusEffect.IsSourceTrap();
		}
    }
}
