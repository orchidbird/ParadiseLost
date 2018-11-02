using UnityEngine; 
using System.Collections; 
using UnityEngine.UI; 
using UnityEngine.EventSystems; 
using System.Collections.Generic;

public class RaycastUI : MonoBehaviour {
	public Canvas mycanvas; // raycast가 될 캔버스 
    GraphicRaycaster gr; 
    PointerEventData ped; 
    
    // Use this for initialization 
    void Start () { 
        gr = mycanvas.GetComponent<GraphicRaycaster>(); 
        ped =  new PointerEventData(null); 
    } 
  
 	/*void Update () { 
        ped.position = Input.mousePosition; 
        List<RaycastResult> results = new List<RaycastResult>(); // 여기에 히트 된 개체 저장 
        gr.Raycast(ped, results); 
        if (results.Count > 0){
			Debug.Log("Mouse is on : " + results[0].gameObject.name); // 디버그 창에 너무 많이 떠서 비활성화해놓음
        } 
    }*/
} 
