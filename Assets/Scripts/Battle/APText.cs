using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class APText : MonoBehaviour {
	Text text;

	// Use this for initialization
	void Start () {
		text = gameObject.GetComponent<Text>();
	}

	// Update is called once per frame
	void Update () {
		string newText = "";

		string phaseText = "[Phase " + BattleData.currentPhase + "]\n";
		newText += phaseText;
		/*string apText = "[Agility : " + unitManager.GetStandardActivityPoint() + "]\n";
		newText += apText;*/
		foreach (var unit in UnitManager.GetAllUnits()){
			// 현재 턴인 유닛에게 강조표시.
			if (BattleData.turnUnit == unit)
				newText += "> ";
			string unitText = unit.name + " : " + unit.GetCurrentActivityPoint() + "\n";
			newText += unitText;
		}
		text.text = newText;
	}
}
