using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastingApply {
	Casting casting;
	Unit target;
	Battle.DamageCalculator.AttackDamage damage = new Battle.DamageCalculator.AttackDamage ();

    public CastingApply(Casting casting, Unit target){
		this.casting = casting;
        this.target = target;
	    casting.applies.Add(this);
    }

    public Battle.DamageCalculator.AttackDamage GetDamage() { return damage; }
    public ActiveSkill GetSkill() { return casting.Skill; }
    public Unit Caster{get { return casting.Caster; }}
	public Unit Target{get { return target; }}
	public int GetTargetCount() { return GetTargets().Count; }
	public List<Tile> GetRealEffectRange() { return casting.RealRange; }
    public List<Unit> GetTargets() {
        List<Unit> targets = new List<Unit>();
		foreach(var tile in GetRealEffectRange()){
            Unit target = tile.GetUnitOnTile();
            if(!targets.Contains(target)) {targets.Add(target);}
        }
        return targets;
    }
    public Casting GetCasting() { return casting; }
}
