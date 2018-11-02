using Battle.Skills;
using UnityEngine;

public class S31_Cavalry_SkillLogic : BasePassiveSkillLogic{
	public override void OnReceivingAmountCalculation(CastingApply castingApply){
		if (castingApply.GetSkill().IsOffensive()){
			castingApply.GetDamage().relativeModifiers[Resources.Load<Sprite>("Icon/Direction")] =
				castingApply.GetDamage().relativeModifiers[Resources.Load<Sprite>("Icon/Direction")] * 2 - 1;
		}
	}
}
