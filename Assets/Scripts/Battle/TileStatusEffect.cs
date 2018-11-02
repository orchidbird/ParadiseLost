public class TileStatusEffect : StatusEffect {
    public TileStatusEffect(TileStatusEffectInfo info, Unit caster, Tile ownerTile, Skill originSkill) : base(){
	    myInfo = info;
	    flexibleElem = new FlexibleElement(this, caster, ownerTile, originSkill);
        ((FlexibleElement.DisplayElement)flexibleElem.display).ownerTile = ownerTile;
	    
        for (int i = 0; i < info.actuals.Count; i++) {
	        actuals.Add(new ActualElement(info.actuals[i]));
            CalculateAmount(i, false);
        }
    }
    
    public new class FlexibleElement : StatusEffect.FlexibleElement {
        public new class DisplayElement : StatusEffect.FlexibleElement.DisplayElement {
            public Tile ownerTile;

            public DisplayElement(Unit caster, Tile ownerTile, Skill originSkill, int defaultPhase) 
                    : base(caster, originSkill, defaultPhase) {
                this.ownerTile = ownerTile;
            }
        }
        public FlexibleElement(StatusEffect statusEffect, Unit caster, Tile ownerTile, Skill originSkill) 
                : base(statusEffect, caster, originSkill) {
            display = new DisplayElement(caster, ownerTile, originSkill, statusEffect.myInfo.defaultPhase);
        }
    }
    public Tile GetOwnerTile() { return ((FlexibleElement.DisplayElement)flexibleElem.display).ownerTile; }
}
