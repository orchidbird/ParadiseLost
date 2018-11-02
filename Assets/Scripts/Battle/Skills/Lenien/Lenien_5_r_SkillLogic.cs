using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills
{
public class Lenien_5_r_SkillLogic : BasePassiveSkillLogic {
	public override void TriggerOnActionEnd(Unit lenien){
		StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(lenien, passiveSkill, lenien);
	} 
}
}
