using ArBlokEvren.Blocks;
using ArBlokEvren.Core;
using ArBlokEvren.Themes;
using ArBlokEvren.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ArBlokEvren.UI
{
    /// <summary>Builds and drives the on-screen controls at runtime.</summary>
    public sealed class GameUI : MonoBehaviour
    {
        private AppState _state;
        private ThemeManager _themes;
        private VoxelWorld _world;

        private Text _statusText;
        private Text _themeLabel;
        private Text _scanLabel;
        private Text _modeLabel;
        private Text _blockLabel;

        private static readonly Color PanelColor = new(0f, 0f, 0f, 0.55f);
        private static readonly Color ButtonColor = new(0.16f, 0.42f, 0.62f, 0.95f);
        private static readonly Color AccentColor = new(0.55f, 0.30f, 0.10f, 0.95f);

        public void Init(AppState state, ThemeManager themes, VoxelWorld world)
        {
            _state = state;
            _themes = themes;
            _world = world;
            Build();
            Refresh();
        }

        private void Build()
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            BuildTopBar(canvasGo.transform);
            BuildBottomBar(canvasGo.transform);
        }

        private void BuildTopBar(Transform canvas)
        {
            RectTransform bar = UIFactory.CreatePanel(canvas, "TopBar", PanelColor);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.sizeDelta = new Vector2(0f, 180f);
            bar.anchoredPosition = Vector2.zero;

            _statusText = UIFactory.CreateText(bar, "Hazir", 34, TextAnchor.MiddleLeft);
            var st = _statusText.rectTransform;
            st.anchorMin = new Vector2(0f, 0f);
            st.anchorMax = new Vector2(0.6f, 1f);
            st.offsetMin = new Vector2(30f, 0f);
            st.offsetMax = new Vector2(0f, 0f);

            Button themeBtn = UIFactory.CreateButton(bar, "Tema", AccentColor, OnThemeClicked, out _themeLabel);
            var tb = themeBtn.GetComponent<RectTransform>();
            tb.anchorMin = new Vector2(0.62f, 0.15f);
            tb.anchorMax = new Vector2(0.98f, 0.85f);
            tb.offsetMin = Vector2.zero;
            tb.offsetMax = Vector2.zero;
        }

        private void BuildBottomBar(Transform canvas)
        {
            RectTransform bar = UIFactory.CreatePanel(canvas, "BottomBar", PanelColor);
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(0f, 220f);
            bar.anchoredPosition = Vector2.zero;

            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            UIFactory.CreateButton(bar, "Tara: Kapali", ButtonColor, OnScanClicked, out _scanLabel);
            UIFactory.CreateButton(bar, "Mod: Kir", ButtonColor, OnModeClicked, out _modeLabel);
            UIFactory.CreateButton(bar, "Blok: Tas", ButtonColor, OnBlockClicked, out _blockLabel);
            UIFactory.CreateButton(bar, "Temizle", AccentColor, OnClearClicked, out _);
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.transform.SetParent(transform, false);
        }

        private void OnThemeClicked()
        {
            _themes.NextTheme();
            Refresh();
        }

        private void OnScanClicked()
        {
            _state.Scanning = !_state.Scanning;
            Refresh();
        }

        private void OnModeClicked()
        {
            _state.Mode = _state.Mode == InteractionMode.Break ? InteractionMode.Add : InteractionMode.Break;
            Refresh();
        }

        private void OnBlockClicked()
        {
            int next = (int)_state.SelectedBlock + 1;
            if (next >= (int)BlockType.Count)
            {
                next = (int)BlockType.Stone;
            }

            _state.SelectedBlock = (BlockType)next;
            Refresh();
        }

        private void OnClearClicked()
        {
            _world.Clear();
            Refresh();
        }

        private void Refresh()
        {
            if (_themeLabel != null)
            {
                _themeLabel.text = $"Evren: {_themes.Current.DisplayName}";
            }

            if (_scanLabel != null)
            {
                _scanLabel.text = _state.Scanning ? "Tara: Acik" : "Tara: Kapali";
            }

            if (_modeLabel != null)
            {
                _modeLabel.text = _state.Mode == InteractionMode.Break ? "Mod: Kir" : "Mod: Koy";
            }

            if (_blockLabel != null)
            {
                _blockLabel.text = $"Blok: {TurkishName(_state.SelectedBlock)}";
            }

            if (_statusText != null)
            {
                _statusText.text = _state.Scanning
                    ? "Cevre taraniyor... telefonu yavasca gezdir"
                    : "Tara'ya bas ve cevreyi tara";
            }
        }

        private static string TurkishName(BlockType type)
        {
            return type switch
            {
                BlockType.Stone => "Tas",
                BlockType.Dirt => "Toprak",
                BlockType.Grass => "Cimen",
                BlockType.Wood => "Odun",
                BlockType.Leaves => "Yaprak",
                BlockType.Sand => "Kum",
                BlockType.Water => "Su",
                BlockType.Metal => "Metal",
                BlockType.Glow => "Isik",
                _ => type.ToString()
            };
        }
    }
}
