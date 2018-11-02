using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using GameData;

public class RightScreen_BattleReady : MonoBehaviour {
	public Image unitImage;
	public Dictionary<Stat, Transform> statInfoDict = new Dictionary<Stat, Transform>();
	public Text unitName;
	public RectTransform StatLayoutGroup;
    
	void Awake () {
		for (int i = 1; i <= 5; i++){
			var stat = (Stat) i;
			statInfoDict.Add(stat, StatLayoutGroup.Find(stat.ToString()));
		}
	}
	
    //PC용
	public void SetCommonUnitInfoUI(string unitEngName){
		unitName.text = UnitInfo.ConvertName(unitEngName, true);
		unitImage.sprite = VolatileData.GetSpriteOf(SpriteType.Illust, unitEngName);

		foreach (var kv in statInfoDict){
			kv.Value.GetComponentInChildren<Text>().text = UnitInfo.GetStatForPC(unitEngName, kv.Key).ToString();
			OnOffGraphBars(kv.Value, true);
			kv.Value.Find("AmountLevel").GetComponent<Image>().fillAmount =
				(float) (UnitInfo.GetStatForPC(unitEngName, kv.Key, false) + 5) / 9;
		}
	}

    // NPC용
    /*public void SetCommonUnitInfoUI(UnitInfo unitInfo) {
        unitName.text = unitInfo.nameKor;
        unitImage.sprite = VolatileData.GetUnitSprite(unitInfo.codeName, unitInfo.side == Side.Ally)[2];
	    
	    foreach (var kv in statInfoDict){
		    kv.Value.GetComponentInChildren<Text>().text = unitInfo.baseStats[kv.Key].ToString();
		    OnOffGraphBars(kv.Value, false);
	    }
    }*/

	void OnOffGraphBars(Transform statTransform, bool isActive){
		statTransform.Find("AmountLevel").gameObject.SetActive(isActive);
		statTransform.Find("background").gameObject.SetActive(isActive);
		statTransform.GetComponentInChildren<Text>().rectTransform.anchoredPosition = new Vector2(isActive ? 165 : 50, 0);
	}
}
