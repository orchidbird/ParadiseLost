using Battle.Damage;
using System.Collections;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
	class Ratice_1_m_SkillLogic : BaseActiveSkillLogic{
		public override void ActionInDamageRoutine(CastingApply castingApply) {
			Unit target = castingApply.Target;

			List<Tile> pushPath = Utility.GetPushPath (castingApply.GetCasting().Location.Dir, target, 1);
			if (pushPath.Count >= 2) {
				target.ForceMove (pushPath);
			} else
			{
				castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 1.3f);
				//DamageCalculator.AttackDamage damage = castingApply.GetDamage ();
				//damage.RelativeModifier *= 1.3f;
			}
		}
	}
}
