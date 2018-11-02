using Battle.Damage;
using UtilityMethods;

namespace Battle.Skills {
    class Eugene_7_r_SkillLogic : BasePassiveSkillLogic {
        // 반경 2 내에서 턴을 시작할 경우 '길잡이'라는 버프가 붙고, 그 상태에서 스킬을 사용하지 않고 이동할 경우 '속도 상승' 버프가 붙음
        public override void TriggerOnAnyTurnStart(Unit caster, Unit turnStarter) {
	        if (Calculate.Distance(caster.Pos, turnStarter.Pos) > 2) return;
	        UnitStatusEffectInfo useInfo = passiveSkill.unitStatusEffectList.Find(se => se.displayKor == "길잡이");
	        UnitStatusEffect statusEffectToAttach = new UnitStatusEffect(useInfo, caster, turnStarter, passiveSkill);
	        turnStarter.statusEffectList.Add(statusEffectToAttach);
        }
        public override void TriggerStatusEffectsOnMove(Unit target, UnitStatusEffect statusEffect) {
            if (!target.GetHasUsedSkillThisTurn())
				StatusEffector.FindAndAttachUnitStatusEffectsToCastingTargets(statusEffect.GetCaster(), passiveSkill, target);
        }
    }
}
