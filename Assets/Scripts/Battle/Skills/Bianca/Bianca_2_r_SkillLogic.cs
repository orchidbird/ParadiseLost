using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battle.Skills {
    class Bianca_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnTrapOperated(Unit unit, TileStatusEffect trap) {
            unit.ChangeAP(8);
        }
    }
}
