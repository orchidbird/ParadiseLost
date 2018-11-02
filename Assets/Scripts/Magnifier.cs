using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

class Magnifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        ArchiveManager AM = FindObjectOfType<ArchiveManager>();
        StartCoroutine(AM.OnMagnifierMouseOver());
    }
    
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        ArchiveManager AM = FindObjectOfType<ArchiveManager>();
        StartCoroutine(AM.OnMagnifierMouseExit());
    }
}