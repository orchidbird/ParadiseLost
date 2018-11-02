using Enums;

namespace Battle.Skills {
    class Triana_Crystalize_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerStatusEffectAppliedByCasting(UnitStatusEffect statusEffect, CastingApply castingApply, int chain){
            //무조건 버프가 붙으므로 의지 감소는 적용되지 않음
	        return statusEffect.IsBuff;
        }
    }
}
