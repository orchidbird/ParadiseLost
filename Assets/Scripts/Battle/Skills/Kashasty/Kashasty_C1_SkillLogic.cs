using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Kashasty_C1_SkillLogic : BasePassiveSkillLogic {
	public override void OnCastingAmountCalculation(CastingApply castingApply){
		DamageCalculator.AttackDamage attackDamage = castingApply.GetDamage();
		Sprite dirSprite = null;
		float mod = 1;
		foreach (var modifier in attackDamage.relativeModifiers){
			if (modifier.Key.name != "Direction") continue;

			if (modifier.Value > 1){
				dirSprite = modifier.Key;
				mod = (modifier.Value - 1) * 2 + 1;
			}
			break;
		}

		if (dirSprite == null) return;
		attackDamage.relativeModifiers.Remove(dirSprite);
		attackDamage.relativeModifiers.Add(dirSprite, mod);
	}
}
}
