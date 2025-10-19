
using UnityEngine;

public class QuitApp : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Exit if escape (or back, on mobile) is pressed.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            Application.Quit();
            ResetVars();
        }
    }

    private void ResetVars()
    {
        //ArInstructEventBroker.CallArHelpStateChanged(HelpAr.HelpArState.UNDEFINED);
    }
}
