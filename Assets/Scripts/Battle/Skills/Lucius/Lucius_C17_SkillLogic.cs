using GameData;

namespace Battle.Skills {
    class Lucius_C17_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster){
	        var required = (ActiveSkill)(passiveSkill.RequiredSkill);
			if (required != null) {
				required.SetRequireAP (required.GetRequireAP () - 7);
				required.SetCooldown (1);
			}
        }
    }
}
