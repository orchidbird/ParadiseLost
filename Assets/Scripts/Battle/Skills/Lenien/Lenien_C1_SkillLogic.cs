using System.Collections;
using System.Collections.Generic;
using Battle.Damage;
using UnityEngine;
using Enums;

namespace Battle.Skills{
	public class Lenien_C1_SkillLogic : BaseActiveSkillLogic {
		public override bool TriggerStatusEffectAppliedByCasting (UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
			return statusEffect.IsTypeOf (StatusEffectType.Aura);
		}

		public override bool IgnoredBySpecialAuraCondition(Unit target, StatusEffect aura){
			if (aura is TileStatusEffect)
				return false;
			bool toAttach = target != ((UnitStatusEffect)aura).GetOwner ();
			if (toAttach)
				activeSkill.intTemp++;
			return !toAttach;
		}

		public override void AfterAuraEffectedCounting(StatusEffect aura){
			int stack = activeSkill.intTemp;
			int level = GameData.RecordData.level;
			float total = stack * (level * 0.5f + 40);
			if (aura.GetAmountOfType (StatusEffectType.DefenseChange) != total) {
				aura.SetAmountOfType (StatusEffectType.DefenseChange, total);
			}
			activeSkill.intTemp = 0;
		}
	}
}
