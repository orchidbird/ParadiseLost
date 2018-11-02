using System.Collections;
using GameData;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour{
	public Image TitleNameImage;
	public void SetNameImage(){
		TitleNameImage.sprite = Resources.Load<Sprite>("Title/Title" + VolatileData.language);
	}
	
	void Start () {
		SetNameImage();
	}

	public IEnumerator ShowTitleName(){
		TitleNameImage.material = new Material(Shader.Find("Custom/LeftToRight"));
		TitleNameImage.material.SetFloat("_Value", 0);
		float duration = 1;
		float elapsedTime = 0;
		while (elapsedTime < duration){
			elapsedTime += Time.deltaTime;
			TitleNameImage.material.SetFloat("_Value", elapsedTime / duration);
			yield return null;
		}
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.T))
			StartCoroutine(FindObjectOfType<SceneLoader>().LoadScene("Test", false));
	}
}
