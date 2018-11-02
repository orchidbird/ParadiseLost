using Battle.Damage;
using Enums;

namespace Battle.Skills {
	class ExplosiveSkillLogic : BasePassiveSkillLogic{
		public override void TriggerOnDestroyed(Unit actor, TrigActionType reason, Unit destroyedUnit){
			var tiles = TileManager.V2ToTiles(Utility.GetSquareRange(destroyedUnit.Pos, 1, 1));
			var units = tiles.FindAll(tile => tile.IsUnitOnTile()).ConvertAll(tile => tile.GetUnitOnTile());
			foreach (var unit in units){
				StatusEffector.AttachGeneralRestriction(StatusEffectType.Faint, passiveSkill, unit, false, 2);
				unit.ApplyDamageByNonCasting(unit.GetHP * 0.6f, destroyedUnit, true);
			}
		}
	}
}
