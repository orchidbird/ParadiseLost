using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Enums;
using UtilityMethods;

public class MaterialManager : MonoBehaviour{
	public enum MaterialState{Default, Dissolve, HighlightElse, HighlightCMY}
	public MaterialState state;
	public SpriteRenderer spriteRenderer;
	public Image image;
	public Material highlightBorderMaterial;
	Material defaultMaterial;

	readonly float delay = 1.0f;

	void SetSpriteMaterial(Material mat){
		GetComponent<SpriteRenderer>().material = mat;
	}
	public void Reset(bool forced = false){ //if(forced) 무조건 강제로 초기화.
		if (state == MaterialState.Dissolve && !forced) return;
		
		state = MaterialState.Default;
		StopAllCoroutines();
		if (spriteRenderer != null)
			SetSpriteMaterial(defaultMaterial);
		else
			image.material = defaultMaterial;
	}
	public IEnumerator DissolveByKill(){
		state = MaterialState.Dissolve;
		StopAllCoroutines();
		SetSpriteMaterial(new Material(Shader.Find("Custom/2D/Dissolve")));
		yield return GetComponent<SpriteRenderer>().material.DOFloat(1, "_Threshold", Setting.unitFadeOutDuration).WaitForCompletion();
		Reset(true);
		spriteRenderer.color = new Color(1, 1, 1, 0);
	}
	public void HighlightBorderWithBlackAndWhite(){
		Activate(ColorList.BW, 0.33f);
	}
	public void Activate(ColorList listType, float delayFactor){
		if (state == MaterialState.Dissolve) return;
		if (listType == ColorList.CMY){
			Reset();
			state = MaterialState.HighlightCMY;
		}else if(state == MaterialState.HighlightCMY)
			return;

		state = MaterialState.HighlightElse;
		if(spriteRenderer != null)
			SetSpriteMaterial(highlightBorderMaterial);
		if(image != null)
			image.material = highlightBorderMaterial;

		if(gameObject != null && gameObject.activeSelf)
			StartCoroutine (_Color.CycleColor (listType, delay*delayFactor, spriteRenderer, image));
	}

	void Start () {
		if(spriteRenderer != null)
			defaultMaterial = spriteRenderer.material;
		else{
			image = GetComponent<Image>();
			defaultMaterial = image.material;
		}
	}
}