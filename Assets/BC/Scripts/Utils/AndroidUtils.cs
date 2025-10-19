using UnityEngine;

public class AndroidUtils : MonoBehaviour
{
    public static int GetAndroidApiLevel()
    {
        // Get Android SDK version at runtime
        int sdkVersion = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            sdkVersion = version.GetStatic<int>("SDK_INT");
        }
#endif
        return sdkVersion;
    }
}
