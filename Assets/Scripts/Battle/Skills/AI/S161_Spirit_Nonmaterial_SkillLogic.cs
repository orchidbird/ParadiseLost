using Enums;

namespace Battle.Skills{
	public class S161_Spirit_Nonmaterial_SkillLogic : BasePassiveSkillLogic{
		public override void OnReceivingAmountCalculation(CastingApply castingApply){
			if (castingApply.GetSkill().GetSkillApplyType() != SkillApplyType.HealHealth) return;
			
			castingApply.Target.ApplyDamageByNonCasting(castingApply.GetDamage().baseDamage, castingApply.Caster, true);
			castingApply.GetDamage().AddModifier(passiveSkill, 0);
		}
	}
}
