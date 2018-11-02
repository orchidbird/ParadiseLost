
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_B15_SkillLogic: BasePassiveSkillLogic{
        public override void TriggerOnStageStart(Unit caster){
	        var reqSkill = passiveSkill.RequiredSkill;
	        var USEList = passiveSkill.unitStatusEffectList;
		        
	        reqSkill.unitStatusEffectList.AddRange(USEList);
            ((ActiveSkill)reqSkill).powerFactor = 0.9f;
        }
    }
}
