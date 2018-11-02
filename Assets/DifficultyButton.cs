using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour{
	public int difficulty;
	Button button;
	public Warning warning;
	
	// Use this for initialization
	void Start (){
		button = GetComponent<Button>();
		button.onClick.AddListener(Act);
	}

	void Act(){
		if (SceneManager.GetActiveScene().name == "Title"){
			FindObjectOfType<SaveSlotPanel>().SelectDifficulty(difficulty);
		}else if (SceneManager.GetActiveScene().name == "Battle"){
			BattleUIManager.Instance.Warn(1);
			warning.SetDifficulty(difficulty);
		}
	}
}
