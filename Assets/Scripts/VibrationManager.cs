using UnityEngine;

public static class VibrationManager
{
    public static void Vibrate()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            if (vibrator.Call<bool>("hasVibrator"))
            {
                vibrator.Call("vibrate", 500); // Vibrate for 500 milliseconds
            }
        #elif UNITY_IOS
            Handheld.Vibrate(); // Still use Handheld.Vibrate for iOS
        #endif
    }
}
