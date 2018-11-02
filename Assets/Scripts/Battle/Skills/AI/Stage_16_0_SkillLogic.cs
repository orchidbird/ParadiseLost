using Battle.Damage;
using System.Collections;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
	class Stage_16_0_SkillLogic : BaseActiveSkillLogic{ // 슈미트 방패치기
		public override void ActionInDamageRoutine(CastingApply castingApply) {
			Unit caster = castingApply.Caster;
			Unit target = castingApply.Target;

			List<Tile> pushPath = Utility.GetPushPath (castingApply.GetCasting().Location.Dir, target, 1);
			if (pushPath.Count >= 2)
				target.ForceMove (pushPath);
			else
				castingApply.GetDamage().absoluteModifiers.Add(activeSkill.icon, (int)(caster.GetStat(Stat.Power) * 0.2f));
			
		}
	}
}
