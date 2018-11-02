using System.Collections;
using Enums;

namespace Battle.Skills {
    class Luvericha_6_l_SkillLogic : BaseActiveSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit target = castingApply.Target;
            Unit caster = castingApply.Caster;
            target.RemoveStatusEffect(caster, StatusEffectCategory.Buff, 1);
        }
    }
}
