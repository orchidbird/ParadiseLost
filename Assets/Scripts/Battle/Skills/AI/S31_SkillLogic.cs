using System.Collections;
using System.Collections.Generic;
using Battle.Damage;
using Battle.Skills;
using UnityEngine;

public class S31_SkillLogic : BasePassiveSkillLogic{
	public override void TriggerAfterDamaged(Unit target, int damage, Unit caster){
		Debug.Log("트리아나 피격");
		if ((target.GetHP - damage) * 2 < target.GetMaxHealth()){
			Debug.Log("HP 50% 미만");
			var foo = StatusEffector.AttachAndReturnUSE(target, StatusEffector.UnitStatusEffectsOfSkill(passiveSkill, target, target), target, false);
			Debug.Log(foo.Count + "개의 효과 부착");
		}
	}
}
