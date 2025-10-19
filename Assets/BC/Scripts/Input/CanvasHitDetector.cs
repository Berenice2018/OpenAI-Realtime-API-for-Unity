using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// https://stackoverflow.com/questions/70636241/unity3d-new-input-system-is-it-really-so-hard-to-stop-ui-clickthroughs-or-figu 

public class CanvasHitDetector : MonoBehaviour
{
    private static GraphicRaycaster graphicRaycaster;
    private static PointerEventData pointerEventData;
    private static List<RaycastResult> results = new List<RaycastResult>();

    private void Awake()
    {
        // This instance is needed to compare between UI interactions and
        // game interactions with the pointer.
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        pointerEventData = new PointerEventData(EventSystem.current);
    }

    public static bool IsPointerOverUI()
    {
        if (Pointer.current == null) return false;
        results.Clear();

        // Create a pointer event data structure with the current pointer position.
        pointerEventData.position = Pointer.current.position.ReadValue();

        // how many UI items did the pointer event hit?. 
        graphicRaycaster.Raycast(pointerEventData, results);
        return results.Count > 0;
    }
}
