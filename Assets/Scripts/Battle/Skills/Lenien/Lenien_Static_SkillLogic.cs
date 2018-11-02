using Battle.Damage;
using Enums;

namespace Battle.Skills
{
public class Lenien_Static_SkillLogic : BasePassiveSkillLogic {
	public override void TriggerAfterDamaged(Unit lenien, int damage, Unit attacker){
		if (attacker.GetElement() == Enums.Element.Metal)
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(lenien, passiveSkill, attacker);
	}

	public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
		var caster = castingApply.Caster;
		var target = castingApply.Target;
		var casterSE = caster.GetSEofDisplayNameKor("정전기");
		if (casterSE != null && target.GetElement() == Element.Metal){
			caster.RemoveStatusEffect(casterSE);
			StatusEffector.AttachGeneralRestriction(StatusEffectType.Faint, passiveSkill, caster, false);
		}
		var targetSE = target.GetSEofDisplayNameKor("정전기");
		if (targetSE != null && caster.GetElement() == Element.Metal){
			target.RemoveStatusEffect(targetSE);
			StatusEffector.AttachGeneralRestriction(StatusEffectType.Faint, passiveSkill, target, false);
		}
	}
}
}
