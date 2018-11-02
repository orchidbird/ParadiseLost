using UnityEngine;

namespace BattleUI{
	public class DefaultPosition : MonoBehaviour{
		Vector3 savedPosition = Vector3.zero;

		private void Awake(){
			RectTransform rectTransform = GetComponent<RectTransform>();
			savedPosition = rectTransform.anchoredPosition3D;
		}

		public void ResetPosition(){
			RectTransform rectTransform = GetComponent<RectTransform>();
			rectTransform.anchoredPosition3D = savedPosition;
		}
	}
}
