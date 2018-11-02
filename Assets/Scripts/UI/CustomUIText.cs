using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class CustomUIText : MonoBehaviour{
    public string text = "";
	public string Text { get { return text;} set { text = value; ApplyText(); } }
	public Align align;
	public Image[] numberImages;

	private List<GameObject> characterInstances = new List<GameObject>();

	public enum Align{
		LEFT,
		MIDDLE,
		RIGHT
	}

	void Start(){
		Clear();
		GenerateTextInstances();
		RePosition();
	}

	void OnEnable(){
		Clear();
		GenerateTextInstances();
		RePosition();
	}

	void OnDisable(){
		Clear();
	}

	public void RefreshOnInspector(){
		List<Transform> childs = new List<Transform>();
		foreach (Transform child in transform){
			// We cannot destroy child when iterating transform
			childs.Add(child);
		}

		foreach (Transform child in childs){
			// In Editor we cannot use Destroy function
			DestroyImmediate(child.gameObject);
		}
		characterInstances.Clear();
		//여기까지 모든 child를 삭제

		//여기부터 새로운 문자 instance를 생성한 후 위치 조절
		GenerateTextInstances();
		RePosition();
	}
    
    public void ApplyText(){
        Clear();
        GenerateTextInstances();
        RePosition();
    }

    private void Clear(){
	    UI.DestroyAllChilds(transform);
		characterInstances.Clear();
	}

	private void GenerateTextInstances(){
		foreach (var character in text){
			Image image = Instantiate(GetPrefab(character)) as Image;
			image.transform.SetParent(transform);

			characterInstances.Add(image.gameObject);
		}
	}

	private void RePosition(){
		var parentRectTransform = GetComponent<RectTransform>();

		float accumulatedWidth = 0;
		float totalWidth = 0;

		foreach (GameObject characterInstance in characterInstances){
			RectTransform rectTransform = characterInstance.GetComponent<RectTransform>();
			totalWidth += rectTransform.rect.width;
		}

		for (int i = 0; i < text.Length; i++){
			var rectTransform = characterInstances[i].GetComponent<RectTransform>();
            float parentWidth = parentRectTransform.rect.width;
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
			rectTransform.localScale = Vector3.one;

			var relativePosition = MakeRelativePosition((int)accumulatedWidth, (int)totalWidth, (int)parentWidth, align);
			rectTransform.anchoredPosition = relativePosition;

			accumulatedWidth += rectTransform.rect.width;
		}
	}

	private static Vector2 MakeRelativePosition(int accumulatedWidth, int totalWidth, int parentWidth, Align align){
		switch (align) {
			case Align.LEFT:
				return new Vector2(accumulatedWidth, 0);
			case Align.MIDDLE:
				return new Vector2(parentWidth / 2 + accumulatedWidth - (totalWidth / 2), 0);
			case Align.RIGHT:
				return new Vector2(parentWidth + accumulatedWidth - totalWidth, 0);
			default:
				Debug.LogWarning("Invalid align");
				return Vector2.zero;
		}
	}

	private Image GetPrefab(char character){
		int parsedValue = Int32.Parse(character.ToString());
		Debug.Assert(parsedValue >= 0 && parsedValue <= 9, character + " is not Valid input.");
		return numberImages[parsedValue];
	}
}
