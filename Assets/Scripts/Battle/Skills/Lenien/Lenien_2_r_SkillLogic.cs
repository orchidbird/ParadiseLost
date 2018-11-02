using UnityEngine;
using System.Linq;

namespace Battle.Skills{
	public class Lenien_2_r_SkillLogic : BasePassiveSkillLogic{
		public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance){
			float ignoreAmountPerLevel = 0.7f;
			float baseAmountPerLevel = 51;

			Tile tileUnderTarget = target.TileUnderUnit;
			if (tileUnderTarget != null)
				resistance -= baseAmountPerLevel + (ignoreAmountPerLevel * GameData.RecordData.level);
			return resistance;
		}
	}
}
