using Battle.Damage;
using System.Collections.Generic;
using System;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills {
	class Deus_0_1_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerExistingUnitPassiveOnDebuffAttach(UnitStatusEffect se, Unit existingUnit) {
			var actuals = se.actuals;
			for (int i = 0; i < se.actuals.Count; i++) {
				se.SetAmount (i, actuals [i].amount * 1.5f);
			}
		}
	}
}
