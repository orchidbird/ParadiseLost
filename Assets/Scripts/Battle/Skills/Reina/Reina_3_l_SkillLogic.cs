using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Reina_3_l_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)	
	{
		int numberOfBuffFromOtherUnits = caster.statusEffectList.Count(x => x.IsBuff && x.GetCaster() != caster);

		// 갯수 * {40 + (레벨 * 0.5)} %
		float ignoreAmount = numberOfBuffFromOtherUnits * (40 + (GameData.RecordData.level * 0.5f));

		return Mathf.Max(0, resistance - ignoreAmount);
	}
}
}
