using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class TranslateUI : MonoBehaviour{
	public string codeName;
	private Text Text;
	private static List<string[]> table;
	public static List<string[]> Table{get {
		if(table == null)
			table = Parser.GetMatrixTableFrom("Data/TranslateUI"); 
		return table;}
	}
	// Use this for initialization
	void Start (){
		Text = GetComponent<Text>();
		Update();
	}
	
	// Update is called once per frame
	void Update (){
		Text.text = Language.Find(codeName);
	}
}
