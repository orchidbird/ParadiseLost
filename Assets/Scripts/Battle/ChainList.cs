using System.Collections.Generic;
using UnityEngine;

public class ChainList : MonoBehaviour {
	private static List<Chain> chainList = new List<Chain>();
    public static List<Chain> GetChainList() {return chainList;}
	public static void AddChains(Casting casting){
        LogManager.Instance.Record(new AddChainLog(casting));
	}
	public static void SetChargeEffectToUnit(Unit unit){
		unit.SetChargeEffect();
	}

	public static void RemoveChainOfThisUnit(Unit unit){
		Chain myChain = chainList.Find(x => x.Caster == unit);
        if (myChain != null)
            LogManager.Instance.Record(new RemoveChainLog(unit));
	}
	public static void RemoveChargeEffectOfUnit(Unit unit){
		unit.RemoveChargeEffect();
	}

	public static void ShowChainByThisUnit(Unit unit){
		Chain chain = chainList.Find(item => item.Caster == unit);
		if (chain == null) return;
		TileManager.Instance.PaintTiles(chain.RealEffectRange, TileManager.Instance.TileColorMaterialForChain);
	}
	
	public static void HideChainTilesDisplay(){
		TileManager.Instance.DepaintAllTiles (TileManager.Instance.TileColorMaterialForChain);
	}

	public static void ShowUnitsTargetingThisTile(Tile tile, bool onOff){
		List<Unit> units = GetUnitsTargetingThisTile (tile);
		foreach (Unit unit in units)
			unit.PreviewChainTriggered (true);
	}
	public static List<Unit> GetUnitsTargetingThisTile(Tile tile){
		return chainList.FindAll(chain => chain.RealEffectRange.Contains(tile)).ConvertAll(chain => chain.Caster);
	}

	public static List<Tile> TargetedTiles{get{
		var result = new List<Tile>();
		foreach (var chain in GetChainList())
			result.AddRange(chain.RealEffectRange);
		return result;
	}}

	// 현재 기술 시전을 첫 원소로 넣은 후, 해당 시전 후 발동되는 모든 체인 추가
	public static List<Chain> GetAllChainTriggered(Casting casting){
		Unit caster = casting.Caster;
		Chain nowChain = new Chain (casting);
		List<Chain> allTriggeredChains = new List<Chain>();
		allTriggeredChains.Add (nowChain);

		//체인 발동계열 스킬(공격/약화)이어야 다른 체인을 발동시킨다.
		//아니라면 그냥 현재 시전만 넣은 리스트를 반환하게 됨
		if (casting.Skill.IsOffensive ())
			foreach (var chain in chainList) // 같은 진영이 대기한 체인 중 공격범위 안의 유닛이 서로 겹치면 추가
				if (chain.Caster.IsAllyTo (caster) && chain.Overlapped (nowChain))
					allTriggeredChains.Add (chain);	
		
		//Debug.Log("발동 체인 수: " + allTriggeredChains.Count);
		return allTriggeredChains;
	}
}
