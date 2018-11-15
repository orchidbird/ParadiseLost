using GameData;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class TutorialReviewPanel : MonoBehaviour{
	public GameObject SelectButtonPrefab;
	public GameObject BackGround;
	
	private void OnEnable(){
		if (RecordData.alreadyReadTutorials.Count == 0){
			BattleUIManager.Instance.Notify(Language.Select("확인한 도움말이 없습니다!", "You've seen no tutorial!"));
			gameObject.SetActive(false);
		}
		foreach (var content in RecordData.alreadyReadTutorials){
			var button = Instantiate(SelectButtonPrefab, BackGround.transform);
			button.GetComponentInChildren<Text>().text = Language.Find("Tutorial_" + content);
			button.GetComponent<Button>().onClick.AddListener(delegate{TutorialManager.Instance.Activate(content, false, true);});
		}
	}

	private void OnDisable(){
		UI.DestroyAllChildren(BackGround.transform);
	}
}
