using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Eren_6_m_SkillLogic : BasePassiveSkillLogic {

	public override void OnCastingAmountCalculation(CastingApply castingApply) {
		float damageBonusPerEachEnemyUnit = 0.05f;
		Unit caster = castingApply.Caster;
		Unit target = castingApply.Target;
		List<Unit> unitsInBonusRange = Utility.UnitsInRange(Utility.TilesInDiamondRange(target.Pos, 1, 2, 1));

		int numberOfEnemyUnitsInRange = unitsInBonusRange.Count(unit => unit.IsEnemyTo(caster));
		float totalDamageBonus = damageBonusPerEachEnemyUnit * numberOfEnemyUnitsInRange + 1.0f;
		castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, totalDamageBonus);
		//castingApply.GetDamage().RelativeModifier *= totalDamageBonus;
	}
}
}
