using Enums;
using UnityEngine;
using UtilityMethods;

public class ChangeColor : MonoBehaviour{
	void OnEnable(){
		StartCoroutine(_Color.CycleColor(ColorList.CMY, 0.5f, GetComponent<SpriteRenderer>()));
	}
}
