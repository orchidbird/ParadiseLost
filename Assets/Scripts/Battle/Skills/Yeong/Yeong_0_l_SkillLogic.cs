using Battle.Damage;
using UtilityMethods;

namespace Battle.Skills
{
public class Yeong_0_1_SkillLogic : BasePassiveSkillLogic{
	public override void TriggerAfterMove(Unit caster, Tile beforeTile, Tile afterTile){
		if (Calculate.Distance(afterTile.Location, caster.GetStartPositionOfPhase()) > 2)
			StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(caster, passiveSkill, caster);
	}
}
}
