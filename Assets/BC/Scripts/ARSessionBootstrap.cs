using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class ARSessionBootstrap : MonoBehaviour
{
    private XRManagerSettings _xrManager;
    private bool _initialized;

    IEnumerator Start()
    {
        // Wait until the XRGeneralSettings asset is loaded
        while (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null)
        {
            Debug.Log("⏳ Waiting for XRGeneralSettings...");
            yield return null;
        }

        _xrManager = XRGeneralSettings.Instance.Manager;

        if (_xrManager.activeLoader == null)
        {
            Debug.Log("➡ Initializing ARKit loader...");
            yield return _xrManager.InitializeLoader();
        }

        if (_xrManager.activeLoader != null)
        {
            Debug.Log("Starting AR subsystems...");
            _xrManager.StartSubsystems();
            _initialized = true;
        }
        else
        {
            Debug.LogError("Failed to initialize XR loader (ARKit not found).");
        }
    }

    void OnDisable()
    {
        if (_initialized && _xrManager != null)
        {
            Debug.Log("Stopping AR subsystems...");
            _xrManager.StopSubsystems();
            _xrManager.DeinitializeLoader();
            _initialized = false;
        }
    }
}