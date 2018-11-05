using UnityEngine;
using UtilityMethods;

namespace Battle.Skills {
	class Ratice_Unique_SkillLogic : BasePassiveSkillLogic {
		public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
			var skillOwner = UnitManager.Instance.GetAnUnit(passiveSkill.ownerName);
			if (castingApply.Target == null || Calculate.Distance(castingApply.Target, skillOwner) != 1 || !castingApply.Target.IsAllyTo(skillOwner)) return;
			
			castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 0.5f);
			castingApply.GetDamage().resultDamage *= 0.5f;
			skillOwner.ApplyDamageByNonCasting(castingApply.GetDamage().baseDamage * 0.5f, castingApply.Caster, true);
		}
	}
}
