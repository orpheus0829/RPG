using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Michsky.UI.MTP
{
    public class StyleCreator : EditorWindow
    {
        // Constants
        const string WINDOW_TITLE = "MTP - Style Creator";
        const string MENU_PATH = "Tools/Motion Titles Pack/Style Creator";
        const int WINDOW_WIDTH = 560;
        const int WINDOW_HEIGHT = 534;
        const int SCROLL_HEIGHT = 432;
        const int BANNER_HEIGHT = 90;

        // Resource paths
        const string STYLE_DATA_PATH = "StyleCreator/StyleData";
        const string BANNER_PATH = "StyleCreator/Banner";
        const string SKIN_DARK_PATH = "Skin/MTP Skin Dark";
        const string SKIN_LIGHT_PATH = "Skin/MTP Skin Light";

        // Window references
        static StyleCreator window;
        StyleVideoPreview tempWindow;

        // Styles
        GUIStyle panelStyle;
        GUIStyle lipStyle;

        // Scroll
        Vector2 scrollPosition = Vector2.zero;

        // Resources
        static Texture2D bannerTexture;
        static StyleCreatorList styleCreatorList;

        // Colors
        static readonly Color32 LipBGColorDark = new(47, 47, 47, 255);
        static readonly Color32 LipAltBGColorDark = new(42, 42, 42, 255);
        static readonly Color32 LipBGColorLight = new(47, 47, 47, 255);
        static readonly Color32 LipAltBGColorLight = new(42, 42, 42, 255);

        [MenuItem(MENU_PATH, false, 0)]
        public static void ShowWindow()
        {
            window = GetWindow<StyleCreator>(WINDOW_TITLE);
            window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
        }

        void OnEnable()
        {
            LoadResources();
        }

        void OnDisable()
        {
            CleanupTempWindow();
        }

        void OnGUI()
        {
            GUISkin customSkin = LoadCustomSkin();
            Color defaultColor = GUI.color;

            if (customSkin == null)
            {
                EditorGUILayout.HelpBox("Custom skin not found. " +
                    "Please ensure MTP skin files are in the Resources/Skin folder.", MessageType.Error);
                return;
            }

            InitializeStyles(customSkin);
            DrawBanner();

            // Check for style list
            if (styleCreatorList == null)
            {
                EditorGUILayout.HelpBox($"StyleData not found at Resources/{STYLE_DATA_PATH}. " +
                    $"Please ensure StyleData.asset is located there.", MessageType.Error);
                if (GUILayout.Button("Reload Resources")) { LoadResources(); }
                return;
            }

            // Get background colors
            Color32 lipBGColor = EditorGUIUtility.isProSkin ? LipBGColorDark : LipBGColorLight;
            Color32 lipAltBGColor = EditorGUIUtility.isProSkin ? LipAltBGColorDark : LipAltBGColorLight;

            DrawStyleGrid(customSkin, defaultColor, lipBGColor, lipAltBGColor);

            if (GUI.enabled)
            {
                Repaint();
            }
        }

        void LoadResources()
        {
            // Load banner from Resources folder
            if (bannerTexture == null)
            {
                bannerTexture = Resources.Load<Texture2D>(BANNER_PATH);
                if (bannerTexture == null)
                {
                    Debug.LogWarning($"[MTP] Banner texture not found.");
                }
            }

            // Load style data from Resources folder
            if (styleCreatorList == null)
            {
                styleCreatorList = Resources.Load<StyleCreatorList>(STYLE_DATA_PATH);
                if (styleCreatorList == null)
                {
                    Debug.LogError($"[MTP] StyleData not found at Resources/{STYLE_DATA_PATH}. " +
                        $"Please ensure StyleData.asset is located in Resources/StyleCreator/");
                }
            }
        }

        void CleanupTempWindow()
        {
            if (tempWindow != null)
            {
                tempWindow.Close();
                tempWindow = null;
            }
        }

        GUISkin LoadCustomSkin()
        {
            string skinPath = EditorGUIUtility.isProSkin ? SKIN_DARK_PATH : SKIN_LIGHT_PATH;
            GUISkin skin = Resources.Load<GUISkin>(skinPath);
            if (skin == null) { Debug.LogWarning($"[MTP] Custom skin not found at Resources/{skinPath}"); }
            return skin;
        }

        void InitializeStyles(GUISkin customSkin)
        {
            // Custom panel
            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { textColor = GUI.skin.label.normal.textColor },
                margin = new RectOffset(15, 15, 0, 15),
                padding = new RectOffset(0, 0, 0, 0)
            };

            // List item panel
            lipStyle = customSkin.FindStyle("Style Creator LIP");
        }

        void DrawBanner()
        {
            if (bannerTexture != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(bannerTexture, GUILayout.Width(WINDOW_WIDTH), GUILayout.Height(BANNER_HEIGHT));
                GUILayout.EndHorizontal();
            }
        }

        void DrawStyleGrid(GUISkin customSkin, Color defaultColor, Color32 lipBGColor, Color32 lipAltBGColor)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, 
                GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(SCROLL_HEIGHT));
            GUILayout.BeginVertical(panelStyle);

            bool drawAltBG = false;
            int lineIndex = 0;

            for (int i = 0; i < styleCreatorList.styles.Count; i++)
            {
                lineIndex++;

                // Alternate background colors for grid layout
                GUI.backgroundColor = drawAltBG ? lipBGColor : lipAltBGColor;
                drawAltBG = !drawAltBG;

                GUILayout.BeginHorizontal(lipStyle);
                GUI.backgroundColor = defaultColor;
                GUILayout.BeginVertical();

                DrawStyleItem(styleCreatorList.styles[i], i, customSkin);

                GUILayout.EndVertical();

                // Create new row after 2 items
                if (lineIndex == 2)
                {
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    lineIndex = 0;
                    drawAltBG = !drawAltBG; // Flip again for next row
                }
            }

            // Close any remaining open horizontal group
            if (lineIndex != 0)
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        void DrawStyleItem(StyleCreatorList.StyleItem style, int index, GUISkin customSkin)
        {
            // Preview image
            GUILayout.Box(style.stylePreview, customSkin.FindStyle("Style Creator Preview"));

            // Title
            EditorGUILayout.LabelField(style.styleTitle, customSkin.FindStyle("Style Creator Title"));

            // Indicators
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(85);
                DrawIndicator(style.customContent, styleCreatorList.ccEnabled, styleCreatorList.ccDisabled, customSkin);
                DrawIndicator(style.customizableWidth, styleCreatorList.cwEnabled, styleCreatorList.cwDisabled, customSkin);
                DrawIndicator(style.customizableHeight, styleCreatorList.chEnabled, styleCreatorList.chDisabled, customSkin);
            }
            GUILayout.EndHorizontal();

            // Buttons
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("", customSkin.FindStyle("Style Creator Play"))) { PlayPreviewVideo(index); }
                if (GUILayout.Button("", customSkin.FindStyle("Style Creator Create"))) { CreateObject(index); }
            }
            GUILayout.EndHorizontal();
        }

        void DrawIndicator(bool condition, Texture enabledTexture, Texture disabledTexture, GUISkin customSkin)
        {
            Texture texture = condition ? enabledTexture : disabledTexture;
            GUILayout.Box(texture, customSkin.FindStyle("Style Creator Indicator"));
        }

        public void PlayPreviewVideo(int styleIndex)
        {
            if (styleIndex < 0 || styleIndex >= styleCreatorList.styles.Count)
                return;

            CleanupTempWindow();

            StyleVideoPreview videoWindow = GetWindow<StyleVideoPreview>();
            GUIContent titleContent = new($"MTP Preview: {styleCreatorList.styles[styleIndex].styleTitle}");
            videoWindow.UpdateVideo(styleCreatorList.styles[styleIndex].videoPreviewURL, titleContent);
            tempWindow = videoWindow;
        }

        public void CreateObject(int styleIndex)
        {
            if (styleIndex < 0 || styleIndex >= styleCreatorList.styles.Count)
            {
                Debug.LogError($"[MTP] Invalid style index: {styleIndex}");
                return;
            }

            GameObject prefab = styleCreatorList.styles[styleIndex].stylePrefab;

            if (prefab == null)
            {
                Debug.LogError($"[MTP] Style prefab is null for: {styleCreatorList.styles[styleIndex].styleTitle}");
                return;
            }

            GameObject clone = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            clone.name = prefab.name; // Use prefab name directly
            Selection.activeObject = clone;

            // Try to parent to canvas
            TryParentToCanvas(clone);

            // Register undo and mark scene dirty
            Undo.RegisterCreatedObjectUndo(clone, $"Create {clone.name}");

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        void TryParentToCanvas(GameObject obj)
        {
            Canvas canvas = FindCanvas();

            if (canvas != null) { obj.transform.SetParent(canvas.transform, false); }
            else { Debug.Log("<b>[MTP]</b> Canvas not found, creating the object outside of a canvas."); }
        }

        Canvas FindCanvas()
        {
#if UNITY_2023_2_OR_NEWER
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            return canvases.Length > 0 ? canvases[0] : null;
#else
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            return canvases.Length > 0 ? canvases[0] : null;
#endif
        }
    }
}