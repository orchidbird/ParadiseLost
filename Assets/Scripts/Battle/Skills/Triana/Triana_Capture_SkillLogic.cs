using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class Triana_Capture_SkillLogic : BaseActiveSkillLogic{
	    public override void ActionInDamageRoutine(CastingApply castingApply){
		    var target = castingApply.Target;
		    target.ForceMove(Utility.GetGrabPath(castingApply.Caster, target));
		    StatusEffector.AttachGeneralRestriction(StatusEffectType.Taunt, activeSkill, target, true);
	    }
    }
}
