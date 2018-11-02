using System;
using Enums;
using System.Collections.Generic;

namespace Battle.Skills {
    class Kashasty_B8_SkillLogic : BasePassiveSkillLogic{
	    public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
		    var owner = passiveSkill.owner;
		    if (chain > 1 && castingApply.Caster == BattleData.turnUnit && castingApply.Caster.IsAllyTo(owner))
			    castingApply.Target.ApplyDamageByNonCasting(owner.GetStat(Stat.Power)*0.5f, owner, true);
	    }
    }
}
