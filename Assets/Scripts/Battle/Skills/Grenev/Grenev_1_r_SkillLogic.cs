namespace Battle.Skills {
    class Grenev_1_r_SkillLogic : BasePassiveSkillLogic{
	    public override void OnCastingAmountCalculation(CastingApply castingApply){
		    float lostHpRatio = 1 - castingApply.Target.GetHpRatio();
		    castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, lostHpRatio * 0.2f + 1);
		    //castingApply.GetDamage().RelativeModifier *= lostHpRatio * 0.2f + 1;
	    }
    }
}
