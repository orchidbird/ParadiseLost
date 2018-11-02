using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    class Grenev_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit deadUnit, TrigActionType actionType){
		    if(actionType != TrigActionType.Kill) return;

            Unit caster = hitInfo.caster;
			BattleManager battleManager = BattleManager.Instance;
            caster.ChangeAP((int)(caster.GetStat(Stat.Agility) * 0.6f));
        }
    }
}
