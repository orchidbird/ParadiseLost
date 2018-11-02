using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class Triana_HeatWave_SkillLogic : BasicChargeSkillLogic{
	    public override void ActionInDamageRoutine(CastingApply castingApply) {
            StatusEffector.AttachGeneralRestriction(StatusEffectType.Silence, activeSkill, castingApply.Target, true);
        }
    }
}
