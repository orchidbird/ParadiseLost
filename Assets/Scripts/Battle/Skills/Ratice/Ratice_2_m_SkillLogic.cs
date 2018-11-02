using Enums;

namespace Battle.Skills {
	class Ratice_2_m_SkillLogic : BasePassiveSkillLogic {
		public override void OnReceivingAmountCalculation(CastingApply castingApply) {
			ActiveSkill skill = castingApply.GetSkill();
			if (skill.GetSkillApplyType () == SkillApplyType.DamageHealth && castingApply.GetDamage ().attackDirection == DirectionCategory.Front) {
				castingApply.GetDamage ().relativeModifiers.Add(passiveSkill.icon, 0.85f);
				//castingApply.GetDamage ().RelativeModifier *= 0.85f;
			}
		}
	}
}
