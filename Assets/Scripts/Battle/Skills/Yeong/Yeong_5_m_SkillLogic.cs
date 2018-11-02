using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Yeong_5_m_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnEvasionEvent(Unit caster, Unit yeong)
	{
		int amount = (int)(yeong.GetStat(Stat.Agility) * 0.3f);
		yeong.ChangeAP(amount);
	}
}
}
