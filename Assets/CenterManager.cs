using GameData;
using UnityEngine;
using UnityEngine.UI;

public class CenterManager : MonoBehaviour{
    public GameObject SoldierList;
    public GameObject SoldierButtonPrefab;
    public void ShowAvailableSoldiers(){
        UIManager.Instance.PushUI(SoldierList);
        foreach (var unit in RecordData.AllUnitInfo){
            var soldier = Instantiate(SoldierButtonPrefab, SoldierList.transform);
            soldier.GetComponentInChildren<Text>().text = unit.codeName;
        }
    }
}