using Enums;

namespace Battle.Skills {
    public class Curi_3_r_SkillLogic : BasePassiveSkillLogic {
        public override bool WillReceiveSE(UnitStatusEffect statusEffect, Unit caster, Unit target){
            return !statusEffect.IsTypeOf(StatusEffectType.DefenseChange) && !statusEffect.IsTypeOf(StatusEffectType.ResistanceChange);
        }
    }
}