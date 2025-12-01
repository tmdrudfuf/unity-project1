// Assets/Scripts/Debug/DestroyWatcher.cs
using UnityEngine;

public class DestroyWatcher : MonoBehaviour
{
    void OnDestroy()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DestroyWatcher] {name} destroyed. Callstack:\n{System.Environment.StackTrace}");
#endif
    }
}
