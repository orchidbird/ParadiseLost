using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Battle.Skills {
    class Arcadia_C1_SkillLogic : BaseActiveSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            Unit target = castingApply.Target;
            target.ForceMove(Utility.GetGrabPath(caster, target));
        }
    }
}
