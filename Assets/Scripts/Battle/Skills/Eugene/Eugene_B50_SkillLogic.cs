using Enums;

namespace Battle.Skills {
    class Eugene_B50_SkillLogic : BaseActiveSkillLogic {
        public override bool TriggerTileStatusEffectWhenUnitTryToUseSkill(Tile tile, TileStatusEffect tileStatusEffect){
	        return tile.GetUnitOnTile().GetUnitClass() != UnitClass.Magic;
        }
        public override bool TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(CastingApply castingApply, TileStatusEffect tileStatusEffect){
            return castingApply.Caster.GetUnitClass() != UnitClass.Magic;
        }
    }
}
