using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_IOS
public class ForceFirstFrame : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Force first Metal frame");
        RenderPipelineManager.beginContextRendering += ForceDraw;
    }

    private void ForceDraw(ScriptableRenderContext context, List<Camera> cameras)
    {
        GL.Clear(true, true, Color.black);
        RenderPipelineManager.beginContextRendering -= ForceDraw; // one-time
    }
}
#endif