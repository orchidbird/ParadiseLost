using UnityEngine;
using UnityEngine.UI;
using GameData;
using UtilityMethods;

public class EtherText : MonoBehaviour{
	public ReadyManager Manager;
	public Text etherText;

	public void Update(){
		Candidate unit = Manager.candidates.Find(item => item.CodeName == Manager.RecentUnitButton.codeName);
		if(unit == null)
			etherText.text = "";
		else if(Manager.CanStart)
			etherText.text = Manager.CurrentEther + " / " + Manager.MaxEther;
		else
			etherText.text = "<color=red>" + Manager.CurrentEther + " / " + Manager.MaxEther + "</color>";
	}		
}
