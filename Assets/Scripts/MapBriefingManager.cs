using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Enums;
using GameData;

public class MapBriefingManager : MonoBehaviour {
    public GameObject tileImagePrefab;
    public GameObject unitImagePrefab;
    public Dictionary<Vector2, GameObject> tileImagesDict = new Dictionary<Vector2, GameObject>();
    public List<GameObject> unitImages = new List<GameObject>();
    Dictionary<string, bool> clickableObjects = new Dictionary<string, bool>();
    Vector3 viewportCenter;
    Vector2 viewportSize;
    Vector2 centerTileVirtualPos;
    float scale;

    public void GenerateUnitAndTileImages(RectTransform viewport, GameObject parent) {
        viewportCenter = viewport.position + new Vector3(0, 0, -50);
        viewportSize = viewport.rect.size;
        var tileInfos = VolatileData.stageData.GetTileInfos();
        AdjustScaleAndCalculatePos(tileInfos);
        GenerateTileImages(tileInfos, parent);
		if (!VolatileData.stageData.IsTwoSideStage ())
			GenerateUnitImages (parent);
    }

    void AdjustScaleAndCalculatePos(List<TileInfo> tileInfoList) {
        float MARGIN = viewportSize.y * 0.2f;
        Func<TileInfo, float> realX = (tileInfo => tileInfo.GetTilePosition().x + tileInfo.GetTilePosition().y);
        Func<TileInfo, float> realY = (tileInfo => tileInfo.GetTilePosition().y - tileInfo.GetTilePosition().x + tileInfo.GetTileHeight());
        TileInfo realXMaximizer = Utility.Maximizer(tileInfoList, realX);
        TileInfo realXMinimizer = Utility.Minimizer(tileInfoList, realX);
        TileInfo realYMaximizer = Utility.Maximizer(tileInfoList, realY);
        TileInfo realYMinimizer = Utility.Minimizer(tileInfoList, realY);
        float width = realX(realXMaximizer) - realX(realXMinimizer);
        float height = realY(realYMaximizer) - realY(realYMinimizer);
        Vector2 centerTileRealPos = new Vector2((realX(realXMaximizer) + realX(realXMinimizer)) / 2,
                                                    (realY(realYMaximizer) + realY(realYMinimizer)) / 2);
        centerTileVirtualPos = new Vector2((centerTileRealPos.x - centerTileRealPos.y) / 2,
                                        (centerTileRealPos.x + centerTileRealPos.y) / 2);
        width *= Setting.tileImageWidth;
        height *= Setting.tileImageHeight;

        scale = Math.Min((viewportSize.x - MARGIN) / width, (viewportSize.y - MARGIN) / height);
    }

    void GenerateTileImages(List<TileInfo> tileInfoList, GameObject parent){
        EraseAllTileImages();
        foreach (var tileInfo in tileInfoList) {
            GameObject tileImage = GenerateTileImage(tileInfo);
            tileImagesDict.Add(tileInfo.GetTilePosition(), tileImage);
            if (parent != null) tileImage.transform.SetParent(parent.transform);
        }
    }
    GameObject GenerateTileImage(TileInfo tileInfo) {
        Vector2 tilePosition = tileInfo.GetTilePosition();
        int tileHeight = tileInfo.GetTileHeight();
        int typeIndex = tileInfo.GetTileIndex();
        string typeIndexString = typeIndex.ToString();
        if (typeIndex < 10)
            typeIndexString = "0" + typeIndexString;

        string imageName = tileInfo.GetTileElement() + "_" + typeIndexString;
        SpriteRenderer tileImage = Instantiate(tileImagePrefab, CalculateTileImagePosition(tilePosition, tileHeight),
                Quaternion.identity).GetComponent<SpriteRenderer>();
        tileImage.name = tileInfo.GetDisplayName();
        tileImage.sprite = VolatileData.GetSpriteOf(SpriteType.TileImage, imageName);
        tileImage.transform.localScale = new Vector3(2 * scale, 2 * scale, 1);
	    if(tileInfo.GetFogType() != FogType.None)
		    tileImage.GetComponent<TileInBriefing>().FogOfWar.SetActive(true);
        return tileImage.gameObject;
    }

    Vector3 CalculateTileImagePosition(Vector2 tilePosition, int height){ 
        var x = tilePosition.x - centerTileVirtualPos.x;
        var y = tilePosition.y - centerTileVirtualPos.y;
	    float realX = x + y, realZ = y - x;
        var result = viewportCenter + new Vector3(realX * scale, (realZ + height) * scale * Setting.tileImageHeight / Setting.tileImageWidth,
                                    realZ * 0.1f * scale / 10);
	    return result;
    }

    void GenerateUnitImages(GameObject parent) {
        EraseAllUnitImages();
	    
	    var genInfos = VolatileData.stageData.GetUnitGenInfos();
        foreach (var genInfo in genInfos){
            var unitImage = GenerateUnitImage(genInfo);
	        if (unitImage == null) continue;
	        
	        unitImages.Add(unitImage);
	        if (parent != null) unitImage.transform.SetParent(parent.transform);
        }
	    
        foreach(var genInfo in VolatileData.stageData.GetUnitGenInfos()){
	        if (genInfo.CodeName != "selected") continue;
	        if(!tileImagesDict.ContainsKey(genInfo.Position)) Debug.Log(genInfo.Position + "에 존재하지 않음!");
	        tileImagesDict[genInfo.Position].GetComponent<TileInBriefing>().ActivateHighlightWall(true);
        }

    }
    GameObject GenerateUnitImage(UnitGenInfo genInfo){
	    var unitInfo = UnitInfo.FindByGenInfo(genInfo);
        var position = genInfo.Position;
        if(genInfo.IsNonFixedPosPC || !tileImagesDict.ContainsKey(position) || tileImagesDict[position].GetComponent<TileInBriefing>().FogOfWar.activeSelf) return null;
            
        Direction direction = genInfo.Direction;
		string unitName;
		Sprite[] sprites;
		if (genInfo.IsFixedPosPC) {
			unitName = genInfo.CodeName.Substring (2);
			sprites = VolatileData.GetUnitSprite (unitName, true);
		} else {
			unitName = genInfo.CodeName;
			sprites = VolatileData.GetUnitSprite (unitName, unitInfo.side == Side.Ally);
		}
        Sprite sprite = null;
        switch (direction) {
        	case Direction.LeftUp: sprite = sprites[0]; break;
        	case Direction.LeftDown: sprite = sprites[2]; break;
        	case Direction.RightUp: sprite = sprites[3]; break;
        	case Direction.RightDown: sprite = sprites[1]; break;
        }

	    var unitImagePosition = Vector3.zero;
	    var leastZ = tileImagesDict[position].transform.position.z;
	    //Unit.TilesUnderUnit 구하는 부분과 알고리즘이 일치해야 하므로, 가능하면 추후에 통합할 것
	    if (unitInfo != null && unitInfo.IsLarge){ // 스킬은 고를 수 있지만 고정위치인 PC는 unitInfo가 null
		    for (var x = 0; x < unitInfo.size.x; x++){
			    for (var y = 0; y < unitInfo.size.y; y++){
				    if (direction == Direction.RightDown || direction == Direction.LeftUp){
					    unitImagePosition += tileImagesDict[position + new Vector2(x, y)].transform.position;
					    leastZ = Math.Min(leastZ, tileImagesDict[position + new Vector2(x, y)].transform.position.z);
				    }else{
					    unitImagePosition += tileImagesDict[position + new Vector2(y, x)].transform.position;
					    leastZ = Math.Min(leastZ, tileImagesDict[position + new Vector2(y, x)].transform.position.z);
				    }
			    }
		    }

		    unitImagePosition /= (unitInfo.size.x * unitInfo.size.y);
		    unitImagePosition.z = leastZ;
	    }else
		    unitImagePosition = tileImagesDict[position].transform.position;
	    
        SpriteRenderer unitImage = Instantiate(unitImagePrefab, unitImagePosition+ new Vector3(0, 0, -0.01f), Quaternion.identity).GetComponent<SpriteRenderer>();
        unitImage.name = genInfo.CodeName;
        
        // ReadyManager에서는 오브젝트가 아닌 것만 클릭 가능함
		if(unitInfo != null && RM != null && unitInfo.isObject)
            unitImage.gameObject.GetComponent<BoxCollider>().enabled = false;
        else if(!clickableObjects.ContainsKey(unitImage.name)) 
            clickableObjects.Add(unitImage.name, true);
        unitImage.sprite = sprite;
        unitImage.transform.localScale = new Vector2(2 * scale, 2 * scale);
        return unitImage.gameObject;
    }
	
    public void EraseAllTileImages() {
        foreach (var tileImage in tileImagesDict)
            Destroy(tileImage.Value);
        tileImagesDict = new Dictionary<Vector2, GameObject>();
    }
    public void EraseAllUnitImages() {
        foreach (var unitImage in unitImages)
            Destroy(unitImage);
        unitImages = new List<GameObject>();
    }
    public void ToggleAllImages(bool hide) {
        foreach (var tileImage in tileImagesDict)
            tileImage.Value.SetActive(!hide);
        foreach (var unitImage in unitImages)
            unitImage.SetActive(!hide);
    }

    public GameObject mouseOverObject = null;
    ReadyManager RM = null;
    void Start() {
        if(SceneManager.GetActiveScene().name == "BattleReady")
            RM = FindObjectOfType<ReadyManager>();
    }
    void Update() { 
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool gotHit = false;
        if (Physics.Raycast(ray, out hit, 1000f)) {
            GameObject hitObject = hit.collider.gameObject;
			// clickableObject 중 하나에 마우스오버했을 경우
            if (clickableObjects.ContainsKey(hitObject.name) && clickableObjects[hitObject.name]) {
				// 이전에 마우스오버되어있던 유닛의 하이라이트를 끈다.
                if (RM != null && mouseOverObject != null && mouseOverObject != hitObject) {
					mouseOverObject.GetComponent<HighlightBorder>().InactiveBorder();
                    mouseOverObject = hitObject;
                }
				// 마우스오버된 유닛의 하이라이트를 켠다.
                if (RM != null && mouseOverObject != hitObject) {
                    RM.OnMouseOverUnit(hitObject.name);
					hitObject.GetComponent<HighlightBorder>().Activate(ColorList.BW, 0.33f);
					mouseOverObject = hitObject;
                }
                gotHit = true;
            }
        }
		// 마우스오버된 유닛으로부터 마우스를 치웠을 경우
        if (mouseOverObject != null && !gotHit && RM != null) {
			mouseOverObject.GetComponent<HighlightBorder>().InactiveBorder();
            mouseOverObject = null;
        }
        if (Input.GetMouseButtonDown(0) && mouseOverObject != null) {
            if (RM != null)
                RM.OnMouseOverUnitClicked(mouseOverObject.name);
        }
    }
}
