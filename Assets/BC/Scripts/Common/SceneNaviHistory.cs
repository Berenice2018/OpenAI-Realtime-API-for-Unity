
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class SceneNaviHistory : ScriptableObject
{
    private Stack<string> sceneStack = new Stack<string>();

    public void PushSceneToStack(string name)
    {
        sceneStack.Push(name);
    }

    public string PopSceneFromStack()
    {
        if(sceneStack.Count > 1)
            sceneStack.Pop();
        return sceneStack.Peek();
    }

    public void ClearSceneStack()
    {
        sceneStack.Clear();
    }

    public string PeekSceneStack()
    {
        return sceneStack.Count >0 ? sceneStack.Peek() : string.Empty;
    }

    public int StackCount()
    {
        return sceneStack.Count;
    }


    public void PrintStackValues()
    {
        Debug.Log("***** --->");
        foreach (string name in sceneStack)
        {
            Debug.Log($"\t {name}");
        }
        Debug.Log("<--- *****");
    }
}
