using Enums;
using System;

namespace Battle.Skills {
    class Darkenir_2_m_SkillLogic : BasePassiveSkillLogic {
        public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance) {
            return Math.Max(0, resistance - target.statusEffectList.Count * (13 + caster.GetStat(Stat.Level) * 0.2f));
        }
    }
}
