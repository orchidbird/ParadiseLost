using Battle.Damage;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
	class Bianca_1_r_SkillLogic : BaseActiveSkillLogic {
		public override int GetRealAPWithOverload(int originAP, Unit caster)
		{
			UnitStatusEffect overloadSE = caster.statusEffectList.Find (se => se.GetOriginSkillName () == activeSkill.GetName () && se.IsTypeOf(StatusEffectType.Overload));
			if (overloadSE != null) {
				return originAP + (int)overloadSE.GetAmountOfType (StatusEffectType.Overload);
			} else {
				return originAP;
			}
		}
		public override void AttachOverload(Unit caster)
		{
			StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets (caster, caster, activeSkill, 1, add: true);
		}
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit target = castingApply.Target;
			target.ForceMove(Utility.GetPushPath(castingApply.GetCasting().Location.Dir, target, 2));
        }
    }
}
