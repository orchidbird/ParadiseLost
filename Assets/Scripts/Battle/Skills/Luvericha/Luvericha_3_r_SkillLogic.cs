using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills{
    class Luvericha_3_r_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerOnPhaseStart(Unit caster, int phase) {
			List<Unit> targets = Utility.UnitsInRange(Utility.TilesInDiamondRange(caster.Pos, 1, 3, 1));
            targets.ForEach(target => {
                target.RecoverHealth(target.GetMaxHealth() * 0.05f, caster);
            });
        }
    }
}
