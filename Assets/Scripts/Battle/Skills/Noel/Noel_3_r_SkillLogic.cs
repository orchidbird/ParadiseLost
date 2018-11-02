using Battle.Damage;

namespace Battle.Skills {
    class Noel_3_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerWhenShieldExhaustedByDamage(Unit shieldOwner, Unit shieldCaster) {
            StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(shieldCaster, passiveSkill, shieldCaster);
        }
    }
}
