using Enums;
using System;

namespace Battle.Skills {
    class Darkenir_A8_SkillLogic : BasePassiveSkillLogic{
	    public override void TriggerOnChain(Chain chain){
		    foreach (var unit in chain.Casting.GetTargets())
			    unit.RemoveStatusEffect(chain.Caster, StatusEffectCategory.Buff, 1);
	    }
    }
}
