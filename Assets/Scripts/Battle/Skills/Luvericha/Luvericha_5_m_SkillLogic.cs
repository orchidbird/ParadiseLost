using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Luvericha_5_m_SkillLogic : BaseActiveSkillLogic{
        public override void TriggerShieldAttacked(Unit target, float amount) {
			BattleManager battleManager = BattleManager.Instance;
            target.RecoverHealth(amount, target);
        }
    }
}
