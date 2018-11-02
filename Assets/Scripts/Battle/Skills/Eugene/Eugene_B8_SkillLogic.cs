
using Battle.Damage;

namespace Battle.Skills {
    public class Eugene_B8_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStageStart(Unit caster) {
            UnitStatusEffectInfo seInfo1 = StatusEffector.USEInfoList.Find(seInfo =>
                seInfo.GetOriginSkillName() == "순백의 방패" && seInfo.displayKor == "순백의 방패");
            seInfo1.defaultPhase = 2;

            UnitStatusEffectInfo seInfo2 = StatusEffector.USEInfoList.Find(seInfo =>
                seInfo.GetOriginSkillName() == "순백의 방패" && seInfo.displayKor == "저항력 상승");
            seInfo2.maxStack = 99;
        }
    }
}
