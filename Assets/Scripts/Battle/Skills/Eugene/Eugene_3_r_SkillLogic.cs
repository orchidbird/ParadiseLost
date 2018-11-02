using Battle.Damage;
using System.Collections;
using System.Collections.Generic;

namespace Battle.Skills {
	class Eugene_3_r_SkillLogic : BasePassiveSkillLogic{
		public override void TriggerOnAnyTurnStart(Unit caster, Unit turnStarter) {
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
		}
		public override void TriggerAfterCast(CastLog castLog) {
			Unit caster = castLog.casting.Caster;
			List<UnitStatusEffect> statusEffectList = caster.statusEffectList;
			UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == passiveSkill.Name);
			if (statusEffect != null)
				caster.RemoveStatusEffect(statusEffect);
		}
    }
}
