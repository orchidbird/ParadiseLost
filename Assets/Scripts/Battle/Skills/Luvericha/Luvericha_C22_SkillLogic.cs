namespace Battle.Skills {
    class Luvericha_C22_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
	        return castingApply.Caster != castingApply.Target;
        }
    }
}
