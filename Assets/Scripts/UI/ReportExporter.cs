using System.Collections;
using System.IO;
using ArSpacePlanner.Core;
using ArSpacePlanner.Measure;
using ArSpacePlanner.Planner;
using ArSpacePlanner.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ArSpacePlanner.UI
{
    /// <summary>
    /// Produces a shareable "plan report": overlays a summary card on the current AR
    /// view, captures it to a PNG, and opens the Android share sheet (falling back to a
    /// saved file with a status message when sharing is unavailable).
    /// </summary>
    public sealed class ReportExporter : MonoBehaviour
    {
        private AppState _state;
        private MeasurementManager _measure;
        private FurniturePlacer _planner;

        public void Init(AppState state, MeasurementManager measure, FurniturePlacer planner)
        {
            _state = state;
            _measure = measure;
            _planner = planner;
        }

        public void ExportAndShare()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            string report = ReportBuilder.Build(_measure, _planner, _state.Unit);
            GameObject overlay = BuildOverlay(report);

            yield return new WaitForEndOfFrame();

            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] png = texture.EncodeToPNG();
            Destroy(texture);
            Destroy(overlay);

            string fileName = "rapor_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string path = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(path, png);

            bool shared = NativeShare.Share("Plan Raporu", report, path);
            _state.SetStatus(shared ? "Rapor payla\u015f\u0131ld\u0131." : "Rapor kaydedildi: " + fileName);
        }

        private static GameObject BuildOverlay(string report)
        {
            var go = new GameObject("ReportOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            RectTransform card = UIFactory.CreatePanel(go.transform, "Card", new Color(0f, 0f, 0f, 0.82f));
            UIFactory.SetAnchoredRect(card, new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.86f),
                Vector2.zero, Vector2.zero);

            Text text = UIFactory.CreateText(card, report, 40, TextAnchor.UpperLeft);
            UIFactory.SetAnchoredRect(text.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(40f, 40f), new Vector2(-40f, -40f));

            return go;
        }
    }
}
