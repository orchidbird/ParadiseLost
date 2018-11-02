using UnityEngine;

public class TemporaryObject : MonoBehaviour{
	public float lifespan;
	public bool toDestroy;
	float remainTime;

	void OnEnable(){
		remainTime = lifespan;
	}

	void Update(){
		remainTime -= Time.deltaTime;
		if (remainTime >= 0) return;
		if(toDestroy)
			Destroy(gameObject);
		else
			gameObject.SetActive(false);
	}
}
