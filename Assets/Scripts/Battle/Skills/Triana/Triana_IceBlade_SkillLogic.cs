using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Triana_IceBlade_SkillLogic : BaseActiveSkillLogic{
	    public override void ActionInDamageRoutine(CastingApply castingApply){
		    castingApply.Target.ChangeAP(-(int)(castingApply.Target.activityPoint * 0.2f));
		    StatusEffector.AttachGeneralRestriction(StatusEffectType.Bind, activeSkill, castingApply.Target, true);
	    }
    }
}
