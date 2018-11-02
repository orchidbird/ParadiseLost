using Enums;
using GameData;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;
using System.Collections;

public class ChainCutScene : MonoBehaviour{
	public Image Illust;
	public RectTransform rectTransform;
	public static readonly float moveDuration = 0.15f;
	public static readonly float waitTime = 0.4f;

	public IEnumerator Act(Unit unit){
		UI.SetIllustPosition(Illust, unit.CodeName);
		yield return UI.MoveWithAcceleration(rectTransform, new Vector2(rectTransform.rect.width + 640, 0), moveDuration);
		yield return new WaitForSeconds(waitTime);
		yield return UI.MoveWithAcceleration(rectTransform, new Vector2(rectTransform.rect.width + 640, 0), moveDuration);
	}
}
