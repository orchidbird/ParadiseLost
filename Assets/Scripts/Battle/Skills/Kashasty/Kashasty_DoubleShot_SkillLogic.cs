using System.Linq;

namespace Battle.Skills{
    public class Kashasty_DoubleShot_SkillLogic : BaseActiveSkillLogic {
			public override void ActionInDamageRoutine(CastingApply castingApply){
            Unit target = castingApply.Target;
            if(castingApply.Caster.GetPassiveSkillList().Any(passive => passive.korName == "그물탄"))
                target.ChangeAP(-(int)(target.activityPoint * 0.35f));
        }

	    public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
		    int count = castingApply.GetDamage().CountTacticalBonus;
		    statusEffect.SetRemainStack(count);
		    return count > 0;
	    }
    }
}
