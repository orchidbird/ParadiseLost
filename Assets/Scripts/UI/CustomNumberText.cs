using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomNumberText : MonoBehaviour {
	public List<GameObject> numbers;
	Sprite[] numberSpriteSheet;

	public void Clear(){
		for(int i = 0; i < numbers.Count; i++)
			numbers[i].SetActive(false);
	}

	public void PrintText (string text, Color color) {
		//Clear();

		if (color == Color.white && numberSpriteSheet == null)
			numberSpriteSheet = Resources.LoadAll<Sprite>("Battle/DamageAndHealFont/number_white");

		char[] chs = text.ToCharArray();
		for (int i = 0; i < chs.Length; i++){
			if(!numbers[i].activeSelf)
				numbers[i].SetActive(true);
		
			if (chs[i] == '+')
				numbers[i].GetComponent<Image>().sprite = numberSpriteSheet[10];
			else if (chs[i] == '-')
				numbers[i].GetComponent<Image>().sprite = numberSpriteSheet[11];
			else{
				int number = Int32.Parse(chs[i].ToString());
				numbers[i].GetComponent<Image>().sprite = number == 0 ? numberSpriteSheet[9] : numberSpriteSheet[number-1];
			}
		}
		for(int i = chs.Length; i < numbers.Count; i++)
			numbers[i].SetActive(false);
	}

	// Use this for initialization
	void Awake () {
		Clear();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
