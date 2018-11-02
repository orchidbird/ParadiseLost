using Enums;

namespace Battle.Skills {
	public class Deus_1_m_SkillLogic : BaseActiveSkillLogic {
		public override void ActionInDamageRoutine(CastingApply castingApply) {
			Unit target = castingApply.Target;
			Unit caster = castingApply.Caster;
			float defense = target.GetStat(Stat.Defense);
			if (target == caster) {
				target.ApplyDamageByNonCasting(target.GetMaxHealth() * 0.1f, caster, true, -defense);
				target.ChangeAP((int)(target.GetStat(Stat.Agility) * 0.8f));
			}
		}
	}
}
