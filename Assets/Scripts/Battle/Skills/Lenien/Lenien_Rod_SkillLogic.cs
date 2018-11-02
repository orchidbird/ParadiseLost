using System.Linq;
using System.Collections.Generic;
using Enums;
using Battle.Damage;

namespace Battle.Skills{
    public class Lenien_Rod_SkillLogic : BasePassiveSkillLogic{
	    public override void TriggerOnMyTurnStart(Unit caster){
		    int casterHeight = caster.GetHeight();
		    bool isThereAnyUnitHigherThanCaster = UnitManager.GetAllUnits().Any(x => x.GetHeight() > casterHeight);

		    if (!isThereAnyUnitHigherThanCaster){
			    StatusEffector.FindAndAttachUnitStatusEffectsOfPrecalculatedAmounts(caster, passiveSkill, caster,
				    new List<List<float>>{
					    new List<float>{
						    caster.GetStat(Stat.Power) * 0.2f
					    }
				    });
		    }
	    }
    }
}
