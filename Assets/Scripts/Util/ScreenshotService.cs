using System.Collections;
using System.IO;
using UnityEngine;

namespace ArSpacePlanner.Util
{
    /// <summary>
    /// Captures the current AR view (camera feed + overlaid furniture/measures) to a
    /// PNG in persistent storage so the user can share their plan.
    /// </summary>
    public sealed class ScreenshotService : MonoBehaviour
    {
        public void Capture(System.Action<string> onSaved)
        {
            StartCoroutine(CaptureRoutine(onSaved));
        }

        private IEnumerator CaptureRoutine(System.Action<string> onSaved)
        {
            yield return new WaitForEndOfFrame();

            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] png = texture.EncodeToPNG();
            Destroy(texture);

            string fileName = "plan_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(path, png);

            onSaved?.Invoke(path);
        }
    }
}
