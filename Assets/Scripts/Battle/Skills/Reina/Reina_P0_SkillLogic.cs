using Battle.Damage;

namespace Battle.Skills
{
public class Reina_P0_SkillLogic : AttachOnStart{
	public override void TriggerOnActionEnd(Unit caster) {
		StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets(caster, caster, passiveSkill, UnitManager.GetAllUnits().FindAll(unit => unit.IsAllyTo(caster)).Count);
	}
}
}
