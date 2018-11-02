using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills{
	public class Curi_4_r_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerUnitStatusEffectRemoved(UnitStatusEffect statusEffect, Unit target){
			if(statusEffect.DisplayName(true) == "시한폭탄"){
				// 범위 잡기
				List<Unit> targets = Utility.UnitsInRange(Utility.GetSquareRange(target.Pos, 0, 1).ConvertAll(pos => TileManager.Instance.GetTile(pos)));
				Unit caster = statusEffect.GetCaster();
				// 데미지 잡기
				float damage = caster.GetStat(Stat.Power)*2;

				// 데미지 적용
				foreach(var damageTarget in targets)
					damageTarget.ApplyDamageByNonCasting(damage, caster, true);
			}

			return true;
		}
	}

}
