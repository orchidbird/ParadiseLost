using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CustomWorldText : MonoBehaviour
{
	public string text = "013";

	static Dictionary<char, Sprite> spriteAP= new Dictionary<char, Sprite>();
	static Dictionary<char, Sprite> spriteWill= new Dictionary<char, Sprite>();
	static Dictionary<char, Sprite> spriteDamageModifier = new Dictionary<char, Sprite>();
	static Dictionary<char, Sprite> spriteDamage = new Dictionary<char, Sprite>();
	static bool spritesLoaded;

	//private static int gap = 80;
	private static float scale = 3f;
	public List<GameObject> characterInstances = new List<GameObject>();

	public enum Align
	{
		LEFT,
		MIDDLE,
		RIGHT
	}
	public enum Font
	{
		DAMAGE,
		RECOVER,
		AP,
		Will,
		DAMAGEMODIFIER
	}

	void Awake(){
		if (!spritesLoaded) {
			LoadSprites();
			spritesLoaded = true;
		}
	}
	void LoadSprites() {
		Sprite[] APSprites;
		Sprite[] WillSprites;
		Sprite[] damageModifierSprites;
		Sprite[] damageSprites;
		APSprites = Resources.LoadAll<Sprite>("Battle/DamageAndHealFont/number_violet");
		CacheSprites(spriteAP, APSprites, false);
		
		damageModifierSprites = Resources.LoadAll<Sprite>("Battle/DamageAndHealFont/number_white");
		CacheSprites(spriteDamageModifier, damageModifierSprites, false);
		
		WillSprites = Resources.LoadAll<Sprite>("Battle/DamageAndHealFont/number_cyan");
		CacheSprites(spriteWill, WillSprites, false);
		
		damageSprites = Resources.LoadAll<Sprite>("Battle/DamageAndHealFont/damage_font");
		CacheSprites(spriteDamage, damageSprites, true);
	}
	void CacheSprites(Dictionary<char, Sprite> spriteDict, Sprite[] spriteSource, bool onlyNumbers) {
		for (char c = '1'; c <= '9'; c++)
			spriteDict[c] = spriteSource[c - '1'];
		spriteDict['0'] = spriteSource[9];

		if (onlyNumbers) return;
		spriteDict['+'] = spriteSource[10];
		spriteDict['-'] = spriteSource[11];
		spriteDict['.'] = spriteSource[12];
		spriteDict['x'] = spriteSource[13];
	}

	public void ApplyText(Font font, Color color, float modifier = -1, bool critical = false){
		DestroyAllChilds();
		GenerateTextInstances(font, color, modifier, critical);
		RePosition(true);
		
		if(critical)
			TutorialManager.Instance.Activate("Critical");
	}

	private void DestroyAllChilds(){
		foreach(var characterInstance in characterInstances)
			Destroy(characterInstance.gameObject);
		characterInstances.Clear();
	}

	private void GenerateTextInstances(Font font, Color color, float modifier = -1, bool critical = false){
		Material newMaterial = null;
		if (modifier > -1){
			newMaterial = new Material(Shader.Find("Custom/ColorDamage"));
			newMaterial.SetFloat("_Modifier", modifier);
			newMaterial.SetFloat("_Alpha", 1);
		}
		
		foreach(var character in text) {
			var Char = new GameObject(character.ToString(), typeof(RectTransform));
			Char.transform.SetParent(transform);
			float scale = GetComponent<RectTransform>().sizeDelta.y * 0.015f;

			var image = Char.AddComponent<Image>();
			image.sprite = GetSprite(character, font);
			image.color = color;	// grayness == 0 일 때 보라색, grayness == 1 일 때 회색

			Char.GetComponent<RectTransform>().sizeDelta = image.sprite.rect.size * scale;

			characterInstances.Add(Char);
			if (newMaterial != null)
				image.material = newMaterial;
		}
		
		if (!critical) return;
		var critShow = Instantiate(characterInstances.Last(), transform);
		critShow.gameObject.name = "Critical";
		critShow.GetComponent<Image>().material = null;
		critShow.GetComponent<Image>().sprite = Resources.Load<Sprite>("Battle/DamageAndHealFont/Critical");
		characterInstances.Add(critShow);
	}
    public void Update() {
        RePosition();
    }

	private void RePosition(bool initialize = false){
        var parentRectTransform = GetComponent<RectTransform>();

        float accumulatedWidth = 0;
        float totalWidth = 0;

        foreach (GameObject characterInstance in characterInstances) {
            RectTransform rectTransform = characterInstance.GetComponent<RectTransform>();
            totalWidth += rectTransform.rect.width;
        }

        for (int i = 0; i < characterInstances.Count; i++) {
            var rectTransform = characterInstances[i].GetComponent<RectTransform>();
            float parentWidth = parentRectTransform.rect.width;
            if (initialize) {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.localScale = Vector3.one;
            }

            var relativePosition = MakeRelativePosition(accumulatedWidth, totalWidth, parentWidth, Align.MIDDLE);
			if(initialize)
				rectTransform.anchoredPosition = relativePosition;
			else {
				Vector2 position = rectTransform.anchoredPosition;
				position.x = relativePosition.x;
				rectTransform.anchoredPosition = position;
			}

            accumulatedWidth += rectTransform.rect.width;
        }
    }
    private static Vector2 MakeRelativePosition(float accumulatedWidth, float totalWidth, float parentWidth, Align align) {
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

    private Sprite GetSprite(char character, Font font){
		if (font == Font.DAMAGE && '0' <= character && character <= '9'){
			return spriteDamage[character];
		} else if (font == Font.RECOVER && '0' <= character && character <= '9'){
			return spriteDamage[character];
		} else if (font == Font.AP) {
			return spriteAP[character];
		} else if (font == Font.Will) {
			return spriteWill[character];
		} else if (font == Font.DAMAGEMODIFIER) {
			return spriteDamageModifier[character];
		} else {
			Debug.Log("This custom text font doesn't exit (only damage font and recover font exist)");
			return null;
		}
	}
}
