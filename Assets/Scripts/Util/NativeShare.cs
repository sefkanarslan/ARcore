using UnityEngine;

namespace ArSpacePlanner.Util
{
    /// <summary>
    /// Minimal Android share sheet (ACTION_SEND). Shares the report text and, when the
    /// app's FileProvider is available, attaches the report image. Everything is wrapped
    /// so a missing FileProvider degrades to a text-only share instead of crashing.
    /// On non-Android platforms this is a no-op.
    /// </summary>
    public static class NativeShare
    {
        public static bool Share(string title, string text, string imagePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var intentClass = new AndroidJavaClass("android.content.Intent");
                using var intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);

                AndroidJavaObject uri = TryGetContentUri(imagePath);
                if (uri != null)
                {
                    intent.Call<AndroidJavaObject>("setType", "image/png");
                    intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uri);
                    intent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION"));
                }
                else
                {
                    intent.Call<AndroidJavaObject>("setType", "text/plain");
                }

                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, title);
                activity.Call("startActivity", chooser);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("NativeShare failed: " + e.Message);
                return false;
            }
#else
            Debug.Log("NativeShare (editor stub):\n" + text + "\nImage: " + imagePath);
            return false;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject TryGetContentUri(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                string packageName = activity.Call<string>("getPackageName");
                using var file = new AndroidJavaObject("java.io.File", imagePath);
                using var providerClass = new AndroidJavaClass("androidx.core.content.FileProvider");
                return providerClass.CallStatic<AndroidJavaObject>(
                    "getUriForFile", activity, packageName + ".fileprovider", file);
            }
            catch (System.Exception)
            {
                return null;
            }
        }
#endif
    }
}
