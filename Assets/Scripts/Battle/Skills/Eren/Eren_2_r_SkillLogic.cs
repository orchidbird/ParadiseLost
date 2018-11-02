using Enums;

namespace Battle.Skills
{
public class Eren_2_r_SkillLogic : BasePassiveSkillLogic {
	public override void TriggerOnNeutralizeByMyHand(HitInfo hitInfo, Unit deadUnit, TrigActionType actionType){
		if(actionType != TrigActionType.Kill) return;
		// 민첩성의 0.1만큼 행동력을 회복
		Unit eren = hitInfo.caster;
		int dexterity = eren.GetStat(Stat.Agility);
		int amount = (int)(dexterity * 0.1f);
		eren.ChangeAP(amount);
	}
}
}
