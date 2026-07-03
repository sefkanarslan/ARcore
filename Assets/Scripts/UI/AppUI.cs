using ArSpacePlanner.Core;
using ArSpacePlanner.Measure;
using ArSpacePlanner.Planner;
using ArSpacePlanner.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ArSpacePlanner.UI
{
    /// <summary>
    /// Builds and drives the whole on-screen interface from code: a status line, a
    /// mode toggle, a unit toggle, and context toolbars for measuring and planning.
    /// </summary>
    public sealed class AppUI : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color ButtonColor = new Color(0.16f, 0.18f, 0.24f, 0.95f);
        private static readonly Color AccentColor = new Color(0.15f, 0.55f, 0.95f, 0.98f);

        private AppState _state;
        private MeasurementManager _measure;
        private FurniturePlacer _planner;
        private ScreenshotService _screenshots;

        private Text _statusText;
        private Text _modeButtonLabel;
        private Text _unitButtonLabel;
        private RectTransform _measureBar;
        private RectTransform _planBar;

        public void Init(AppState state, MeasurementManager measure, FurniturePlacer planner, ScreenshotService screenshots)
        {
            _state = state;
            _measure = measure;
            _planner = planner;
            _screenshots = screenshots;

            EnsureEventSystem();
            Transform canvas = BuildCanvas();
            BuildStatusBar(canvas);
            BuildTopButtons(canvas);
            BuildMeasureBar(canvas);
            BuildPlanBar(canvas);

            _state.ModeChanged += OnModeChanged;
            _state.UnitChanged += OnUnitChanged;
            _state.StatusChanged += OnStatusChanged;

            OnModeChanged(_state.Mode);
            OnUnitChanged(_state.Unit);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(go);
        }

        private Transform BuildCanvas()
        {
            var go = new GameObject("AppCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            return go.transform;
        }

        private void BuildStatusBar(Transform canvas)
        {
            RectTransform panel = UIFactory.CreatePanel(canvas, "StatusBar", PanelColor);
            UIFactory.SetAnchoredRect(panel, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(20f, -150f), new Vector2(-20f, -30f));

            _statusText = UIFactory.CreateText(panel, "", 34, TextAnchor.MiddleCenter);
            UIFactory.StretchToParent(_statusText.rectTransform);
        }

        private void BuildTopButtons(Transform canvas)
        {
            Button modeButton = UIFactory.CreateButton(canvas, "Mod", AccentColor, ToggleMode, out _modeButtonLabel);
            UIFactory.SetAnchoredRect((RectTransform)modeButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -300f), new Vector2(340f, -170f));

            Button unitButton = UIFactory.CreateButton(canvas, "Birim", ButtonColor, ToggleUnit, out _unitButtonLabel);
            UIFactory.SetAnchoredRect((RectTransform)unitButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-340f, -300f), new Vector2(-20f, -170f));
        }

        private void BuildMeasureBar(Transform canvas)
        {
            _measureBar = CreateBottomBar(canvas, "MeasureBar");
            AddBarButton(_measureBar, "Geri Al", ButtonColor, () => _measure.Undo());
            AddBarButton(_measureBar, "Alan", AccentColor, () => _measure.CloseArea());
            AddBarButton(_measureBar, "Temizle", ButtonColor, () => _measure.Clear());
        }

        private void BuildPlanBar(Transform canvas)
        {
            _planBar = CreateBottomBar(canvas, "PlanBar", 260f);

            var catalogRow = new GameObject("Catalog", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            catalogRow.transform.SetParent(_planBar, false);
            var catalogRect = catalogRow.GetComponent<RectTransform>();
            UIFactory.SetAnchoredRect(catalogRect, new Vector2(0f, 0.5f), new Vector2(1f, 1f),
                new Vector2(10f, 5f), new Vector2(-10f, -5f));
            ConfigureLayout(catalogRow.GetComponent<HorizontalLayoutGroup>());

            for (int i = 0; i < FurnitureCatalog.Items.Count; i++)
            {
                int index = i;
                UIFactory.CreateButton(catalogRow.transform, FurnitureCatalog.Items[i].DisplayName, ButtonColor,
                    () => _planner.SelectCatalog(index), out _);
            }

            var actionRow = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            actionRow.transform.SetParent(_planBar, false);
            var actionRect = actionRow.GetComponent<RectTransform>();
            UIFactory.SetAnchoredRect(actionRect, new Vector2(0f, 0f), new Vector2(1f, 0.5f),
                new Vector2(10f, 5f), new Vector2(-10f, -5f));
            ConfigureLayout(actionRow.GetComponent<HorizontalLayoutGroup>());

            UIFactory.CreateButton(actionRow.transform, "Sil", ButtonColor, () => _planner.DeleteSelected(), out _);
            UIFactory.CreateButton(actionRow.transform, "Kaydet", ButtonColor, () => _planner.Save(), out _);
            UIFactory.CreateButton(actionRow.transform, "Y\u00fckle", ButtonColor, () => _planner.Load(), out _);
            UIFactory.CreateButton(actionRow.transform, "Temizle", ButtonColor, () => _planner.ClearAll(), out _);
            UIFactory.CreateButton(actionRow.transform, "Foto", AccentColor, CaptureScreenshot, out _);
        }

        private RectTransform CreateBottomBar(Transform canvas, string name, float height = 150f)
        {
            RectTransform panel = UIFactory.CreatePanel(canvas, name, PanelColor);
            UIFactory.SetAnchoredRect(panel, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(20f, 30f), new Vector2(-20f, 30f + height));

            if (name == "MeasureBar")
            {
                var layout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
                ConfigureLayout(layout);
            }
            return panel;
        }

        private static void ConfigureLayout(HorizontalLayoutGroup layout)
        {
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.spacing = 12f;
            layout.padding = new RectOffset(12, 12, 12, 12);
        }

        private void AddBarButton(RectTransform bar, string label, Color color, System.Action onClick)
        {
            UIFactory.CreateButton(bar, label, color, onClick, out _);
        }

        // ---- events ------------------------------------------------------------

        private void ToggleMode()
        {
            _state.Mode = _state.Mode == AppMode.Measure ? AppMode.Plan : AppMode.Measure;
        }

        private void ToggleUnit()
        {
            _state.Unit = Units.Next(_state.Unit);
        }

        private void CaptureScreenshot()
        {
            if (_screenshots == null)
            {
                return;
            }
            _screenshots.Capture(path => _state.SetStatus("Foto kaydedildi: " + System.IO.Path.GetFileName(path)));
        }

        private void OnModeChanged(AppMode mode)
        {
            bool measuring = mode == AppMode.Measure;
            _measureBar.gameObject.SetActive(measuring);
            _planBar.gameObject.SetActive(!measuring);
            _modeButtonLabel.text = measuring ? "Mod: \u00d6l\u00e7\u00fcm" : "Mod: Plan";

            _measure.Active = measuring;
            _planner.SetActiveTool(!measuring);

            _state.SetStatus(measuring
                ? "\u00d6l\u00e7\u00fcm modu \u2014 noktalar\u0131 se\u00e7mek i\u00e7in y\u00fczeye dokun."
                : "Plan modu \u2014 katalogdan se\u00e7ip zemine dokun.");
        }

        private void OnUnitChanged(MeasureUnit unit)
        {
            _unitButtonLabel.text = "Birim: " + UnitLabel(unit);
        }

        private void OnStatusChanged(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }

        private static string UnitLabel(MeasureUnit unit)
        {
            switch (unit)
            {
                case MeasureUnit.Centimeters: return "cm";
                case MeasureUnit.Feet: return "ft";
                case MeasureUnit.Inches: return "in";
                default: return "m";
            }
        }
    }
}
