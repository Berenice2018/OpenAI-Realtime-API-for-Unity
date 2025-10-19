using System;
using UnityEngine;

public class GlobalEventBroker : MonoBehaviour
{
    public static event Action<bool> JsonForLocalizationIsParsed;
    public static void CallJsonForLocalizationIsParsed(bool isParsed)
    {
        JsonForLocalizationIsParsed?.Invoke(isParsed);
    }
    
    /// <summary>
    /// Display a message to user
    /// </summary>
    public static event Action<string> DisplayMessage;
    public static void CallDisplayMessage(string keyOfLocalizedText)
    {
        DisplayMessage?.Invoke(keyOfLocalizedText);
    }
    
    public static event Action<int, bool> EnteredPoiRegion;
    public static void CallEnteredPoiRegion(int currentPoi, bool enteredRegion)
    {
        EnteredPoiRegion?.Invoke(currentPoi, enteredRegion);
    }
    
    public static event Action<bool> HasGotNetwork;
    public static void CallHasGotNetwork(bool hasGotNetwork)
    {
        HasGotNetwork?.Invoke(hasGotNetwork);
    }

    public static event Action PoIPrefabLoaded;
    public static void CallPoIPrefabLoaded()
    {
        PoIPrefabLoaded?.Invoke();
    }
    
    /// <summary>
    /// Raise and listen to event, when a mesh of Heidenturm model was clicked
    /// </summary>
    public static event Action<GameObject> MeshWasTapped;
    public static void CallMeshWasTapped(GameObject tappedGO)
    {
        MeshWasTapped?.Invoke(tappedGO);
    }
    
    public static event Action SceneReset;
    public static void CallSceneReset()
    {
        SceneReset?.Invoke();
    }
}
