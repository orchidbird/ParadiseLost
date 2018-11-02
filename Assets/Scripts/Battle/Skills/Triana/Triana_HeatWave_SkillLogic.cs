using System.Collections;
using System.Collections.Generic;
using Battle.Damage;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Triana_HeatWave_SkillLogic : BasicChargeSkillLogic{
	    public override void ApplyAdditionalDamage(CastingApply castingApply){
		    if(activeSkill.Owner.GetElement() == Element.Fire)
			    castingApply.GetDamage().AddModifier(activeSkill, 2);
	    }
	    
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            if (castingApply.Caster.myInfo.GetElement == Element.Fire)
		        StatusEffector.AttachGeneralRestriction(StatusEffectType.Silence, activeSkill, castingApply.Target, true);
        }
    }
}
