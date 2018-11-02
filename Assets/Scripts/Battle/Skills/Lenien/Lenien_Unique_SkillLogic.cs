using Enums;
using Battle.Damage;

namespace Battle.Skills
{
public class Lenien_Unique_SkillLogic : BasePassiveSkillLogic {
	public override void TriggerActiveSkillDamageAppliedByOwner(Unit caster, Unit target){
		StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, target);
	}
}
}
