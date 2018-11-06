using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using GameData;
using DG.Tweening;

public class CameraMover : MonoBehaviour{
	public bool Movable = true;
    public Dictionary<Direction, float> boundary = new Dictionary<Direction, float>();
    const float MARGIN = 1;
	public GameObject backGroundImageGO;

	float speed = 4f;
	public Vector3 fixedPosition = Vector3.zero;

	public void SetFixedPosition(Vector3 position){
		fixedPosition = new Vector3(position.x, position.y, transform.position.z);
	}

    bool isAtBoundary(Direction direction) {
        switch(direction) {
        case Direction.Left:
            return Camera.main.transform.position.x <= boundary[direction];
        case Direction.Right:
            return Camera.main.transform.position.x >= boundary[direction];
        case Direction.Up:
            return Camera.main.transform.position.y >= boundary[direction];
        case Direction.Down:
            return Camera.main.transform.position.y <= boundary[direction];
        }
        return true;
    }
	
    void Start(){
		FindObjectOfType<CameraMover>().CalculateBoundary();
		SetFixedPosition (transform.position);
    }
    public void CalculateBoundary(){
        List<Tile> tiles = new List<Tile>();
        Dictionary<Vector2, Tile> tilesDict = FindObjectOfType<TileManager>().GetAllTiles();
        foreach (var kv in tilesDict) tiles.Add(kv.Value);
        boundary[Direction.Left] = tiles.Min(tile => tile.transform.position.x) - MARGIN;
        boundary[Direction.Right] = tiles.Max(tile => tile.transform.position.x) + MARGIN;
        boundary[Direction.Up] = tiles.Max(tile => tile.transform.position.y) + MARGIN;
        boundary[Direction.Down] = tiles.Min(tile => tile.transform.position.y) - MARGIN;
    }

    int currentZoomCorutineID = 0;
    float originalCameraSize = 3.6f;
    public bool zoomedOut;
    IEnumerator ChangeCameraSize(float size, float duration, int id) {
        float startSize = Camera.main.orthographicSize;
        float time = 0;
        float x;
        while (true) {
            time += Time.deltaTime;
            if (time > duration)
                break;
            if(currentZoomCorutineID != id)
                yield break;
            Camera.main.orthographicSize = Mathf.Lerp(startSize, size, time / duration);
            x = Camera.main.orthographicSize / 3.6f;
            backGroundImageGO.transform.localScale = new Vector3(x, x, x);
            BattleManager.Instance.clickEffect.transform.localScale = new Vector3(x, x, x);
            yield return null;
        }
        Camera.main.orthographicSize = size;
        x = size / 3.6f;
	    backGroundImageGO.transform.localScale = new Vector3(x, x, x);
    }
    public IEnumerator Zoom(float size, float duration, bool relative = false) {
        if(relative)
            size *= Camera.main.orthographicSize;
        size = Mathf.Clamp(size, Setting.minCameraSize, Setting.maxCameraSize);
        currentZoomCorutineID ++;
        yield return StartCoroutine(ChangeCameraSize(size, duration, currentZoomCorutineID));
    }
    public IEnumerator ZoomOutCameraToViewMap(float duration) {
        const float MARGIN = 5;
        List<Tile> tiles = TileManager.Instance.GetAllTiles().Values.ToList();
        originalCameraSize = Camera.main.orthographicSize;

        float width = tiles.Max(tile => tile.transform.position.x) - tiles.Min(tile => tile.transform.position.x) + MARGIN;
        float height = tiles.Max(tile => tile.transform.position.y) - tiles.Min(tile => tile.transform.position.y) + MARGIN;
        StartCoroutine(Slide(Utility.AveragePos(tiles), duration));
        zoomedOut = true;
        float size = Math.Max(height / 2, width * Screen.height / (Screen.width * 2));
        yield return Zoom(size, duration);
    }
    public IEnumerator ZoomInBack(float duration) {
        zoomedOut = false;
        StartCoroutine(Slide(fixedPosition, duration));
        yield return Zoom(originalCameraSize, duration);
    }
    
    int currentSlideCorutineID = 0;
    IEnumerator SlideCameraToPosition(Vector2 position, float duration, int id) {
        Vector3 destPos = new Vector3(position.x, position.y, -10);
        Vector3 currentPos = Camera.main.transform.position;
        Vector3 direction = (destPos - currentPos);
        float time = 0;
        while (true) {
            time += Time.deltaTime;
            if (time > duration) {
                break;
            }
            //등가속도
            if(currentSlideCorutineID != id)
                yield break;
            Camera.main.transform.position += 2 * (duration - time) * direction * Time.deltaTime / (duration * duration);
            //등속도
            //Camera.main.transform.position += direction * Time.deltaTime / duration;
            yield return null;
        }
        Camera.main.transform.position = destPos;
    }

    public IEnumerator Slide(Vector2 position, float duration) {
	    if (zoomedOut) yield break;
	    
	    currentSlideCorutineID++;
	    yield return SlideCameraToPosition(position, duration, currentSlideCorutineID);
    }


    public void MoveCameraToPosition(Vector2 position){
	    if (zoomedOut) return;
	    Camera.main.transform.position = new Vector3(position.x, position.y, -10);
    }

    // 즉시 옮기지 않고 로그를 남김
    public static void MoveCameraToUnit(Unit unit) {
        MoveCameraToObject(unit);
    }
    public static void MoveCameraToTile(Tile tile) {
        MoveCameraToObject(tile);
    }
    private static void MoveCameraToObject(MonoBehaviour obj) {
        if (obj == null)
            return;
        LogManager.Instance.Record(new CameraMoveLog(obj.gameObject.transform.position, Setting.basicCameraMoveDuration));
    }

    void Update (){
	    if(!BattleManager.Instance.AllowCameraChange) return;
	    
	    // move by mouse.
	    var mousePosition = Input.mousePosition;
	    if (mousePosition.x < Screen.width*0.01f && !isAtBoundary(Direction.Left))
		    MoveCameraToDirection(Vector3.left);
	    else if (mousePosition.x > Screen.width* 0.99f && !isAtBoundary(Direction.Right))
		    MoveCameraToDirection(Vector3.right);

	    if (mousePosition.y < Screen.height* 0.01f && !isAtBoundary(Direction.Down))
		    MoveCameraToDirection(Vector3.down);
	    else if (mousePosition.y > Screen.height* 0.99f && !isAtBoundary(Direction.Up))
		    MoveCameraToDirection(Vector3.up);

		// move by keyboard.
	    if (Input.GetKey(KeyCode.LeftArrow) && !isAtBoundary(Direction.Left))
		    MoveCameraToDirection(Vector3.left);
	    else if (Input.GetKey(KeyCode.RightArrow) && !isAtBoundary(Direction.Right))
		    MoveCameraToDirection(Vector3.right);

	    if (Input.GetKey(KeyCode.DownArrow) && !isAtBoundary(Direction.Down))
		    MoveCameraToDirection(Vector3.down);
	    else if (Input.GetKey(KeyCode.UpArrow) && !isAtBoundary(Direction.Up))
		    MoveCameraToDirection(Vector3.up);
    }

	void MoveCameraToDirection(Vector3 direction){
		Camera.main.transform.position += direction * speed * Time.deltaTime;
	}

	public void MoveCameraToAveragePosition<T>(List<T> list){
		if(list.Count == 0) {return;}
		var averagePos = Utility.AveragePos(list);
		MoveCameraToPosition(new Vector3(averagePos.x, averagePos.y, -10));
	}
	
	void MoveCameraToPosition(Vector3 position){
		Camera.main.transform.position = position;
	}
}
