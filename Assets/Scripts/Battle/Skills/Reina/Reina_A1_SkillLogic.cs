using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills
{
public class Reina_A1_SkillLogic : BaseActiveSkillLogic{
	public override void ApplyAdditionalDamage(CastingApply castingApply){
		var buffs = castingApply.Caster.statusEffectList.FindAll(se => se.IsBuff);
		castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, buffs.Count*0.05f + 1);
	}
}
}
