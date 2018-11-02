using System.Collections;
using System.Collections.Generic;
using Battle.Damage;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Triana_Capture_SkillLogic : BaseActiveSkillLogic{
	    public override void ApplyAdditionalDamage(CastingApply castingApply){
		    if(activeSkill.Owner.GetElement() == Element.Plant)
			    castingApply.GetDamage().AddModifier(activeSkill, 2);
	    }
	    
	    public override void ActionInDamageRoutine(CastingApply castingApply){
		    var target = castingApply.Target;
		    target.ForceMove(Utility.GetGrabPath(castingApply.Caster, target));
		    if(activeSkill.Owner.GetElement() == Element.Plant)
			    StatusEffector.AttachGeneralRestriction(StatusEffectType.Taunt, activeSkill, target, true);
	    }
    }
}
