using UtilityMethods;

namespace Battle.Skills {
	class S102_eichmann_SkillLogic1 : AttachOnStart {
		// 연막탄 스킬로직
		public override float ApplyBonusEvasionFromTargetPassive(CastingApply castingApply){
			int distance = Calculate.Distance(castingApply.Target.Pos, castingApply.Caster.Pos);
			return distance >= 2 ? 1.0f : 0;
		}
	}
}
