using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battle.Damage;

namespace Battle.Skills {
    class Curi_8_m_SkillLogic : BaseActiveSkillLogic {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit unit, Unit owner) {
            return owner.GetHP; // *0.08은 StatusEffect.CalculateAmount 메서드에서 적용됨.
        }
        public override void TriggerTileStatusEffectAtTurnEnd(Unit turnEnder, Tile tile, TileStatusEffect tileStatusEffect) {
            if (turnEnder.TileUnderUnit == tile) {
                List<Tile> tiles = new List<Tile>();
                tiles.Add(tile);
				StatusEffector.FindAndSetStackUnitStatusEffectsNotToCastingTargets (tileStatusEffect.GetCaster (), turnEnder, activeSkill, 1, add: true);
            }
        }
    }
}
