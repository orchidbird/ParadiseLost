using System.Linq;
using Battle.Damage;
using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
	public class S81_SkillLogic2 : BaseActiveSkillLogic {
		// S81 요정 정령사 "숲의 은총" 스킬로직
		public override void OnCast(Casting casting) {
			Unit caster = casting.Caster;
			foreach(var unit in casting.GetTargets()) 
				if(unit.GetSide() == caster.GetSide())
					unit.RecoverHealth(unit.GetMaxHealth() * 0.1f, caster);
		}
	}
}
