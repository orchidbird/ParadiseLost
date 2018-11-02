using System.Linq;
using Battle.Damage;
using Enums;

namespace Battle.Skills{
	public class Triana_WildFire_SkillLogic : BasePassiveSkillLogic{
		public override void TriggerOnTurnEnd(Unit caster, Unit turnEnder){
			if (turnEnder != passiveSkill.owner || turnEnder != caster) return;
		
			var targets = Utility.TilesInDiamondRange(caster.Pos, 1, 2, 0).FindAll(tile => tile.IsUnitOnTile()).Select(tile => tile.GetUnitOnTile());
			foreach (var target in targets)
				StatusEffector.AttachAndReturnUSE(caster, StatusEffector.UnitStatusEffectsOfSkill(skill, caster, target), target, false);
		}
	}
}
