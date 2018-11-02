
namespace Battle.Skills {
    public class Eugene_1_l_SkillLogic : BaseActiveSkillLogic {
	    public override void ApplyAdditionalDamage(CastingApply castingApply){
		    var shieldCount = UnitManager.GetAllUnits().FindAll(unit => unit.statusEffectList.Exists(se => se.DisplayName(true) == "순백의 방패")).Count;
		    castingApply.GetDamage().baseDamage *= (3 + shieldCount);
		    castingApply.GetDamage().baseDamage /= 3;
	    }
    }
}
