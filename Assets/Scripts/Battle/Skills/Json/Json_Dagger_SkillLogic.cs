
namespace Battle.Skills {
    class Json_Dagger_SkillLogic : BaseActiveSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            if (castingApply.Target.statusEffectList.Find(se => se.myInfo.displayKor == "표식") != null)
	            castingApply.GetDamage().relativeModifiers.Add(activeSkill.icon, 9);
        }
    }
}
