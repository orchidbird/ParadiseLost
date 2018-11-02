using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    public class Curi_6_m_SkillLogic : BaseActiveSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply){
            Unit target = castingApply.Target;
            Unit caster = castingApply.Caster;
            float defense = target.GetStat(Stat.Defense);
            if (target == caster)
				target.ApplyDamageByNonCasting(target.GetMaxHealth() * 0.1f, caster, true, -defense);
        }
    }
}
