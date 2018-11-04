using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class AttachAuraOnStart : BasePassiveSkillLogic {
	    public override void TriggerOnStageStart(Unit caster){
		    var aura = StatusEffector.UnitStatusEffectsOfSkill(passiveSkill, caster, caster)
			    .FindAll(se => se.IsTypeOf(StatusEffectType.Aura));
		    StatusEffector.AttachAndReturnUSE(caster, aura, caster, false);
	    }
    }
}
