using UnityEngine;
using GameData;

public class BGLoader : MonoBehaviour{
	public GameObject background;
	void Start(){
		background.GetComponent<SpriteRenderer>().sprite = VolatileData.GetStageBackground(VolatileData.progress.stageNumber);
	}
}
