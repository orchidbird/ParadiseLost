namespace Battle.Skills{
	public class S181_2_SkillLogic : BasePassiveSkillLogic {
		public override void OnAnyCastingDamage(CastingApply castingApply, int chain){
			var casting = castingApply.GetCasting();
			if(casting.Skill.IsOffensive () && casting.GetTargets().Contains(passiveSkill.Owner)
				&& !castingApply.GetDamage().relativeModifiers.ContainsKey(passiveSkill.icon))
				castingApply.GetDamage().relativeModifiers.Add(passiveSkill.icon, 0.7f);
		}
	}
}
