using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class ShowKeywordExplain : MonoBehaviour {
	static string keywordData;
	static ShowKeywordExplain instance;
	public static ShowKeywordExplain Instance{get { return instance; }}
	
	void Awake(){instance = this;}
	void OnEnable(){
		if(GetComponent<TextMeshProUGUI>().text == string.Empty)
			SetBackgroundOnOff(false);
	}
	
	public void Show(string input){
		if(keywordData == null) keywordData = Resources.Load<TextAsset>("Data/KeywordData").text;
		string[] RowDataStrings = keywordData.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
		string keywordExplain = "";

		bool alreadyAdded = false;
		foreach(string row in RowDataStrings){
			string[] tempRowData = row.Split('\t');
			if (!input.Contains(tempRowData[0])) continue;
			
			if(alreadyAdded) keywordExplain += "\n";
			else alreadyAdded = true;

			keywordExplain += tempRowData[0] + ": " + tempRowData[1];
		}
		
		GetComponentInParent<Image>().enabled = alreadyAdded;
		GetComponent<TextMeshProUGUI>().text = _String.ColorExplainText(keywordExplain);
		UI.SetHorizontalFit(transform.parent.GetComponent<RectTransform>(), 380);
	}
	
	public void Clear(){
		var keywordText = GetComponent<TextMeshProUGUI>();
		if (keywordText == null) return;
		
		keywordText.text = string.Empty;
		SetBackgroundOnOff(false);
	}

	void SetBackgroundOnOff(bool active){
		if(GetComponentInParent<Image>() != null)
			GetComponentInParent<Image>().enabled = active;
	}
}
