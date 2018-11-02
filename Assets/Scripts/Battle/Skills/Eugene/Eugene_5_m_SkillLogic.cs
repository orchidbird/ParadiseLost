
using Battle.Damage;

namespace Battle.Skills {
    public class Eugene_5_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster) {
            UnitStatusEffectInfo statusEffectInfo = StatusEffector.USEInfoList.Find(seInfo => 
                seInfo.GetOriginSkillName() == "순백의 방패" && seInfo.displayKor == "순백의 방패");
            statusEffectInfo.actuals[0].formula.basic = 3;
        }
    }
}
