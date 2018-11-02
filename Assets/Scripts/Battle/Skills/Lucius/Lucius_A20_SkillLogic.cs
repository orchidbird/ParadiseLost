
namespace Battle.Skills {
    class Lucius_A20_SkillLogic : BasePassiveSkillLogic {
        public override void OnCastingAmountCalculation(CastingApply castingApply) {
            DamageCalculator.AttackDamage damage = castingApply.GetDamage();
            if(!damage.HasTacticalBonus)
                damage.relativeModifiers.Add(passiveSkill.icon, 1.2f);
        }
    }
}
