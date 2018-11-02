
using Battle.Damage;

namespace Battle.Skills { 
    public class Curi_B8_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster) {
            UnitStatusEffectInfo statusEffectInfo = StatusEffector.USEInfoList.Find(seInfo => seInfo.GetOriginSkillName() == "가연성 부착물");
            ActiveSkill skill = caster.GetActiveSkillList().Find(sk => sk.GetName() == "수상한 덩어리");
	        if(skill != null)
            	skill.unitStatusEffectList.Add(statusEffectInfo);
        }
    }
}
