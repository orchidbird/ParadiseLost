using System.Collections.Generic;
using Enums;
using Battle.Damage;

namespace Battle.Skills
{
    public class Eren_7_l_SkillLogic : BasePassiveSkillLogic {
	    public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit deadUnit, TrigActionType actionType){
			if(actionType != TrigActionType.Kill) return;
		    List<Unit> targets = Utility.UnitsInRange(Utility.TilesInDiamondRange(deadUnit.Pos, 1, 2, 1));
			targets.FindAll(target => target.GetSide() == Side.Enemy).ForEach(unit => {
				 StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(hitInfo.caster, this.passiveSkill, unit);
			});			   
	    }
    }
}
