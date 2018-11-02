using UnityEngine;
using UnityEngine.UI;
using Enums;
using UtilityMethods;

public class HighlightBorder : MonoBehaviour {
	public SpriteRenderer spriteRenderer;
	Image image;
	public Material highlightBorderMaterial;
	Material defaultMaterial;

	Coroutine playingCoroutine;

	readonly float delay = 1.0f;

	public void HighlightWithBlackAndWhite(){
		Activate(ColorList.BW, 0.33f);
	}
	public void Activate(ColorList listType, float delayFactor){
		if(spriteRenderer != null)
			spriteRenderer.material = highlightBorderMaterial;
		if(image != null)
			image.material = highlightBorderMaterial;

		if(gameObject != null && gameObject.activeSelf)
			StartCoroutine (_Color.CycleColor (listType, delay*delayFactor, spriteRenderer, image));
	}
	public void InactiveBorder(){
		StopAllCoroutines();
		spriteRenderer.material = defaultMaterial;
	}

	// Use this for initialization
	void Start () {
		if(spriteRenderer != null)
			defaultMaterial = spriteRenderer.material;
		else{
			image = GetComponent<Image>();
			defaultMaterial = image.material;
		}
	}
}
