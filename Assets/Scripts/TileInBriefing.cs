using UnityEngine;

public class TileInBriefing : MonoBehaviour {
    public GameObject highlightWall;
	public GameObject FogOfWar;

    public void ActivateHighlightWall(bool active) {
        highlightWall.SetActive(active);
    }
}
