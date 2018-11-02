using Enums;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;
using UnityEngine;

namespace Battle.Skills{
	class Sepia_2_m_SkillLogic : BasePassiveSkillLogic {
		public override void OnCastingAmountCalculation(CastingApply castingApply){
			Unit target = castingApply.Target;
			Unit caster = castingApply.Caster;
			if(target.IsAllyTo(caster)){
				castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 0);
			}
		}
	}
}
