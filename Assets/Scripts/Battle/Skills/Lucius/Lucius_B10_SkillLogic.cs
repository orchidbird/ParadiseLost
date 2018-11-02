using System.Collections.Generic;
using Enums;
using UnityEngine;
using Battle.Damage;
using System;

namespace Battle.Skills {
    class Lucius_B10_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            TileManager tileManager = TileManager.Instance;
            int count = 0;
            foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
	            if ((int) direction >= 4) continue;
	            Tile tile = tileManager.GetTile(caster.Pos + Utility.DirToV2I(direction));
	            if (tile == null || !tileManager.isTilePassable(caster, tile, true))
		            count++;
            }
			StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets(caster, caster, passiveSkill, count);
        }
    }
}
