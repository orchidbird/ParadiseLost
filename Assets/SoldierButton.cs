using UnityEngine;

public class SoldierButton : MonoBehaviour{
    public UnitInfo info;

    public void OnSelected(){
        CenterManager.selectedSoldier = info;
        CenterManager.Instance.ClearSoldierList();
        CenterManager.Instance.SoldierList.transform.parent.Find("Projects").gameObject.SetActive(false);
    }
}