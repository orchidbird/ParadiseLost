using Enums;
using Battle.Damage;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    class Lucius_Unique_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit neutralizedUnit, TrigActionType actionType){
            if(actionType == TrigActionType.Retreat && neutralizedUnit.GetHP > 0)
	            StatusEffector.FindAndAttachUnitStatusEffectsOfPrecalculatedAmounts (passiveSkill.owner, passiveSkill, passiveSkill.owner, new List<List<float>>{ new List<float>{ neutralizedUnit.GetHP } });
        }
    }
}
