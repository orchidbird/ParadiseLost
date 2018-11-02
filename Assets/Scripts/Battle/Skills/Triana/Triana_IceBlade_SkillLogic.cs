using Enums;
using Battle.Damage;

namespace Battle.Skills {
    class Triana_IceBlade_SkillLogic : BaseActiveSkillLogic{
	    public override void ApplyAdditionalDamage(CastingApply castingApply){
		    if(activeSkill.Owner.GetElement() == Element.Water)
			    castingApply.GetDamage().AddModifier(activeSkill, 2);
	    }

	    public override void ActionInDamageRoutine(CastingApply castingApply){
		    castingApply.Target.ChangeAP(-(int)(castingApply.Target.activityPoint * 0.2f));
		    if(activeSkill.Owner.GetElement() == Element.Water)
			    StatusEffector.AttachGeneralRestriction(StatusEffectType.Bind, activeSkill, castingApply.Target, true);
	    }
    }
}
