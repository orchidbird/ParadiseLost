using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Lenien_5_l_SkillLogic : BasePassiveSkillLogic {
	    public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target)
	    {
		    int casterHeight = caster.GetHeight();
		    int targetHeight = target.GetHeight();
		    int deltaHeight = casterHeight - targetHeight;

		    // 2단 이상 낮은 위치의 적을 공격하면 저항력 감소 효과 42 + (레벨당 0.6)
		    if (deltaHeight >= 2)
		    {
			    StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, this.passiveSkill, target);
		    }
	    }
    }
}