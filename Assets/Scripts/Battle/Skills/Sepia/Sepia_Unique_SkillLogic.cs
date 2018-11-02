using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
	public class Sepia_Unique_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerOnActionEnd(Unit caster) {
			List<Tile> nearTiles = Utility.TilesInDiamondRange(caster.Pos, 1, 1, 1);

			foreach (var tile in TileManager.Instance.GetAllTiles()) {
				TileStatusEffect alreadyAppliedStatusEffect = tile.Value.StatusEffectList.Find (se => se.GetOriginSkill () == passiveSkill);
				if (alreadyAppliedStatusEffect == null) {
					if (nearTiles.Contains (tile.Value)) {
						StatusEffector.FindAndAttachTileStatusEffects(caster, passiveSkill, tile.Value);
					}
				} else {
					if (!nearTiles.Contains (tile.Value)) {
						tile.Value.RemoveStatusEffect (alreadyAppliedStatusEffect);
					}
				}
			}

		}
	}
}
