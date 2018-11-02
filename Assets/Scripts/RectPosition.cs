using UnityEngine;

public class RectPosition {
    public Transform anchorTransform;
    public Vector2 pivot;
    public Vector2 anchor;
    public Vector3 anchoredPos;
    public bool inScreen;

    public RectPosition(Transform anchorTransform, Vector2 pivot, Vector2 anchor, Vector3 anchoredPos, bool inScreen = false) {
        this.anchorTransform = anchorTransform;
        this.pivot = pivot;
        this.anchor = anchor;
        this.anchoredPos = anchoredPos;
        this.inScreen = inScreen;
    }

    public void PlaceRect(RectTransform rect) {
        rect.pivot = pivot;
        RectTransform anchorRect = anchorTransform.GetComponent<RectTransform>();
        //Debug.Log("anchorRect 위치 : " + anchorRect.position);
        Vector3 anchorPos;
        if(anchorRect == null)
            anchorPos = anchorTransform.position;
        else anchorPos = Utility.RectLocalPositionToWorldPosition(anchorRect, anchor);
		
		rect.position = anchorPos + anchoredPos;

        /*if (inScreen) {
            Vector3[] v = new Vector3[4];
            // LeftDown, LeftUp, RightUp, RightDown
            rect.GetWorldCorners(v);
            Vector3 position = rect.position;
            if(v[0].x < 0)
                position.x -= v[0].x;
            if(v[0].y < 0)
                position.y -= v[0].y;
            if(v[2].x > Screen.width)
                position.x -= v[2].x - Screen.width;
            if(v[2].y > Screen.height)
                position.y -= v[2].y - Screen.height;
            rect.position = position;
        }*/
    }
}
