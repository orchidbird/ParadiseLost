using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class PhaseDisplay : MonoBehaviour {
	Text phaseText;
	BattleManager battleManager;

	void Start () {
		phaseText = GetComponent<Text>();
		battleManager = FindObjectOfType<BattleManager>();
	}

	void Update () {
		phaseText.text = BattleData.currentPhase.ToString("D2");
	}
}
