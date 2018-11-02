using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Battle.Turn;
using UnityEngine;
using Enums;

public class Casting {
	Unit caster;
    List<Unit> targets;
	public List<CastingApply> applies = new List<CastingApply>();
    ActiveSkill skill;
	SkillLocation location;

    public Casting(Unit caster, ActiveSkill skill, SkillLocation location){
		this.caster = caster;
		this.skill = skill;
		this.location = location;
    }

	public Unit Caster{get { return caster; }}
	public ActiveSkill Skill{get { return skill; }}
	public SkillLocation Location{get { return location; }}
	
	public List<Tile> FirstRange{get { return Skill.GetTilesInFirstRange (Location.CasterPos, Location.Dir); }}
	private List<Tile> realRange;
	public List<Tile> RealRange{get {
		if (realRange == null){
			Location.SetRealTargetTile(skill);
			realRange = Location.TargetTile == null ? new List<Tile>()
				: TileManager.Instance.TilesInRange(skill.secondRange, Location.TargetPos, Location.Dir, 1);
		}
		return realRange;
	}}
	public void ResetRealRange(){
		if (Skill.GetRangeType() != RangeType.Route) return;
		realRange = null;
		Location.SetRealTargetTile(Skill);
	}

    public void SetTargets() {
        targets = new List<Unit>();
        foreach (var tile in RealRange)
            if (tile.IsUnitOnTile() && !targets.Contains(tile.GetUnitOnTile()))
                targets.Add(tile.GetUnitOnTile());
    }
    public List<Unit> GetTargets() {
	    if(targets == null)
		    SetTargets();
        return targets;
    }

	//PC일 때만 체크한다는 전제 하에 Side를 IsAllyTo/IsEnemyTo를 거치지 않고 바로 받아오므로, AI 턴에 사용할 경우 수정하고 나서 쓸 것!
	public bool IsValid{get{
		if (GetTargets().Count == 0) return false;
		if (Skill.IsOffensive() && GetTargets().All(unit => unit.GetSide() == Side.Ally))
			return false;
		if (Skill.IsFriendly() && GetTargets().All(unit => unit.GetSide() == Side.Enemy))
			return false;
		return true;
	}}

	public int RequireAP{get { return Caster.GetActualRequireSkillAP(Skill); }}
	public void Cast(int chainCombo, bool duringAIDecision, bool isPreview = false){
		Skill.Apply(this, chainCombo, duringAIDecision, isPreview: isPreview);
	}
	
	public float GetReward(){
		List<Chain> triggeredChains = ChainList.GetAllChainTriggered (this);
		float totalReward = 0;
		
		foreach (var kv in SkillAndChainStates.GetCastingResultPreview (triggeredChains, true)){
			Unit target = kv.Key;
			float unitValue = caster.GetAI().GetTargetValue(target);
			if(unitValue == 0) continue;
			int originalHealth = target.GetHP + target.GetRemainShield ();
			int resultHealth = (int)kv.Value.unitHp + (int)kv.Value.unitShield;
			int healthChange = resultHealth - originalHealth;
			float relativeHealthChange = (float)healthChange / (originalHealth - target.RetreatHP);
			totalReward += unitValue * relativeHealthChange;
			totalReward += -unitValue * kv.Value.ccLifeChangeAmount [StatusEffectType.Bind] / 8;
			totalReward += -unitValue * kv.Value.ccLifeChangeAmount [StatusEffectType.Silence] / 5;
			totalReward += -unitValue * kv.Value.ccLifeChangeAmount [StatusEffectType.Faint] / 4;
		}
		return totalReward;
	}
}
