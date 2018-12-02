using System.Collections;
using Enums;
using GameData;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        ClearSoldierList();
        UIManager.Instance.PushUI(SoldierList);
        foreach (var unit in RecordData.AllUnitInfo){
            var soldier = Instantiate(SoldierButtonPrefab, SoldierList.transform);
            soldier.GetComponent<SoldierButton>().info = unit;
            soldier.GetComponentInChildren<Text>().text = unit.codeName + ": 레벨 " + unit.baseStats[Stat.Level]
                                                          + " 체력 " + unit.baseStats[Stat.CurrentHP] + "/" + unit.baseStats[Stat.MaxHealth]
                                                          + " 경험치 " + unit.baseStats[Stat.Exp] + "/" + unit.baseStats[Stat.MaxExp];
        }
    }
    public void ClearSoldierList(){
        UI.DestroyAllChildren(SoldierList.transform);
        SoldierList.SetActive(false);
    }

    public void ElapseTime(){
        StartCoroutine(Elapse());
    }
    IEnumerator Elapse(){
        var wait = new WaitForSeconds(0.5f);
        for (int i = 0; i < 6; i++){
            if (projType == ProjectType.Training)
                selectedSoldier.baseStats[Stat.Exp] += 1;
            yield return wait;
        }
        SceneManager.LoadScene("Battle");
    }
}