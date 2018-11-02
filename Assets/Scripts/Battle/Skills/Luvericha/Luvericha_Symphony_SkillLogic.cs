namespace Battle.Skills {
    public class Luvericha_Symphony_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            var target = castingApply.Target;
            castingApply.GetDamage().baseDamage = target.GetHP * 0.2f;
        }

	    public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply,  int chain){
		    return castingApply.Caster.GetPassiveSkillList().Exists(ps => ps.required == activeSkill.address);
	    }
    }
}
