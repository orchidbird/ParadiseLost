using Enums;
using System.Collections.Generic;
namespace Battle.Skills {
    class FireSpiritPassive : BasePassiveSkillLogic{
		public override void TriggerOnMyTurnStart(Unit caster) {
            TileRange range = new TileRange(RangeForm.Diamond, 1, 2, 1);
            List<Tile> tiles = TileManager.Instance.TilesInRange(range, caster.Pos, Direction.Down, 1);
			List<Unit> targets = new List<Unit> ();
            foreach(var tile in tiles)
				if (tile.IsUnitOnTile() && tile.GetUnitOnTile().GetElement() != Element.Fire)
					targets.Add (tile.GetUnitOnTile ());

			if (targets.Count <= 0) return;
			CameraMover.MoveCameraToUnit (caster);
			float damage = caster.GetStat(Stat.Power) * 0.8f;
			foreach (Unit unit in targets) 
				unit.ApplyDamageByNonCasting (damage, caster, true);
        }
    }
}
