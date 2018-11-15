using GameData;
using UnityEngine;
using UnityEngine.UI;
using UtilityMethods;

public class CenterManager : MonoBehaviour{
    public GameObject SoldierList;
    public GameObject SoldierButtonPrefab;
    
    public enum ProjectType{Training, Recruit, Heal}
    public static ProjectType projType;
    public static UnitInfo selectedSoldier;

    public static CenterManager Instance;

    void Awake(){
        Instance = this;
    }

    public void SelectProjectType(int input){
        projType = (ProjectType)input;
        ShowAvailableSoldiers();
    }
    void ShowAvailableSoldiers(){
        UIManager.Instance.PushUI(SoldierList);
        foreach (var unit in RecordData.AllUnitInfo){
            var soldier = Instantiate(SoldierButtonPrefab, SoldierList.transform);
            soldier.GetComponent<SoldierButton>().info = unit;
            soldier.GetComponentInChildren<Text>().text = unit.codeName;
        }
    }
    public void ClearSoldierList(){
        UI.DestroyAllChildren(SoldierList.transform);
        SoldierList.SetActive(false);
    }
}