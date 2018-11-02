using UnityEngine;
using Enums;
using System.Collections;
using System.Collections.Generic;

public class Chain {
	readonly Casting casting;
	public Chain (Casting casting){
		this.casting = casting;
	}
	public Casting Casting { get { return casting; } }
	public Unit Caster { get { return casting.Caster; } }
	public ActiveSkill Skill { get { return casting.Skill; } }
	SkillLocation Location { get { return casting.Location; } }
	void UpdateLocation(){
		Location.SetRealTargetTile(Skill);
	}
	public List<Unit> CurrentTargets{
		get {
			List<Unit> targets = new List<Unit> ();
			foreach (Tile targetTile in RealEffectRange) {
				if (targetTile.IsUnitOnTile ())
					targets.Add (targetTile.GetUnitOnTile ());
			}
			return targets;
		}
	}

	public List<Tile> RealEffectRange {
		get {
			UpdateLocation();
			return casting.RealRange;
		}
	}

	public bool Overlapped(Chain allyChain){
		List<Unit> myTargets = CurrentTargets;
		List<Unit> allyTargets = allyChain.CurrentTargets;
		foreach (Unit allyTarget in allyTargets)
		{
			if (myTargets.Contains(allyTarget))
				return true;
		}
		return false;
	}

	public void Cast(int chainCombo, bool duringAIDecision){
		UpdateLocation();
		casting.Cast (chainCombo, duringAIDecision);
	}
}