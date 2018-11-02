using Battle.Damage;
using Enums;

namespace Battle.Skills {
	class FragmentSkillLogic : BasePassiveSkillLogic{
		public override void TriggerOnDestroyed(Unit actor, TrigActionType reason, Unit destroyedUnit){
			var tiles = TileManager.V2ToTiles(Utility.GetSquareRange(destroyedUnit.Pos, 0, 1));
			foreach (var tile in tiles)
				if(tile != null)
					LogManager.Instance.Record(new TileAttachLog(tile, 20));
		}
	}
}
