namespace Battle.Skills {
    class Luvericha_Soul_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerApplyingHeal(CastingApply castingApply) {
            Unit caster = castingApply.Caster;
            caster.RecoverHealth(castingApply.GetDamage().resultDamage * 0.2f, caster);
        }
    }
}
