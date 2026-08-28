using UnityEngine;
using UnityEditor;

namespace Michsky.UI.MTP
{
    [CustomEditor(typeof(StyleManager))]
    public class StyleManagerEditor : Editor
    {
        // Constants
        private const string SKIN_DARK_PATH = "Skin/MTP Skin Dark";
        private const string SKIN_LIGHT_PATH = "Skin/MTP Skin Light";
        
        // References
        private StyleManager sTarget;
        private GUISkin customSkin;
        
        // Animation preview
        private AnimationClip tempAnim;
        private bool playAnim;
        
        // Tab management
        private int currentTab;
        private int currentAnim;
        
        private readonly string[] animOptions = new string[] { "In", "Out" };
        private readonly string[] tabNames = new string[] { "Content", "Animation", "Resources", "Settings" };

        private void OnEnable()
        {
            sTarget = (StyleManager)target;
            sTarget.inspectAnim = false;
            LoadCustomSkin();
        }

        private void OnDisable()
        {
            CleanupAnimationMode();
        }

        public override void OnInspectorGUI()
        {
            if (customSkin == null)
            {
                LoadCustomSkin();
                if (customSkin == null)
                {
                    EditorGUILayout.HelpBox("Custom skin not found. Using default inspector.", MessageType.Warning);
                    DrawDefaultInspector();
                    return;
                }
            }

            MTPEditorHandler.DrawComponentHeader(customSkin, "SM Top Header");
            DrawTabButtons();
            
            serializedObject.Update();
            
            switch (currentTab)
            {
                case 0: DrawContentTab(); break;
                case 1: DrawAnimationTab(); break;
                case 2: DrawResourcesTab(); break;
                case 3: DrawSettingsTab(); break;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void LoadCustomSkin()
        {
            string skinPath = EditorGUIUtility.isProSkin ? SKIN_DARK_PATH : SKIN_LIGHT_PATH;
            customSkin = Resources.Load<GUISkin>(skinPath);
            
            if (customSkin == null)
            {
                Debug.LogWarning($"[MTP] Custom skin not found at Resources/{skinPath}");
            }
        }

        private void CleanupAnimationMode()
        {
            sTarget.tempAnimTime = 0;
            
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.EndSampling();
                AnimationMode.StopAnimationMode();
            }
        }

        private void DrawTabButtons()
        {
            GUIContent[] toolbarTabs = new GUIContent[tabNames.Length];
            for (int i = 0; i < tabNames.Length; i++)
            {
                toolbarTabs[i] = new GUIContent(tabNames[i]);
            }

            currentTab = MTPEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            // Individual tab buttons
            if (GUILayout.Button(new GUIContent("Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Animation"), customSkin.FindStyle("Tab Animation")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 2;
            if (GUILayout.Button(new GUIContent("Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 3;

            GUILayout.EndHorizontal();
        }

        private void DrawContentTab()
        {
            GUILayout.Space(6);

            // Draw text items
            DrawTextItems();
            
            // Draw image items
            DrawImageItems();
            
            // Draw info boxes
            DrawContentInfo();
        }

        private void DrawTextItems()
        {
            if (sTarget.textItems.Count == 0) return;

            bool headerDrawn = false;
            
            for (int i = 0; i < Mathf.Min(4, sTarget.textItems.Count); i++)
            {
                if (sTarget.textItems[i] == null) continue;

                if (!headerDrawn)
                {
                    GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Texts Top Header"));
                    headerDrawn = true;
                }
                else
                {
                    GUILayout.Space(3);
                }

                DrawTextItem(i);
            }
        }

        private void DrawImageItems()
        {
            if (sTarget.imageItems.Count == 0) return;

            bool headerDrawn = false;
            
            for (int i = 0; i < Mathf.Min(4, sTarget.imageItems.Count); i++)
            {
                if (sTarget.imageItems[i] == null) continue;

                if (!headerDrawn)
                {
                    if (sTarget.textItems.Count > 0)
                        GUILayout.Space(14);
                        
                    GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Images Top Header"));
                    headerDrawn = true;
                }
                else
                {
                    GUILayout.Space(3);
                }

                DrawImageItem(i);
            }
        }

        private void DrawContentInfo()
        {
            MTPEditorHandler.DrawHeader(customSkin, "Info Top Header", 10);

            var customContent = serializedObject.FindProperty("customContent");
            var customizableWidth = serializedObject.FindProperty("customizableWidth");
            var customizableHeight = serializedObject.FindProperty("customizableHeight");

            DrawInfoBox(customContent.boolValue,
                "This style supports custom content. You can create your own objects under 'Mask Content'.",
                "This style does not support custom content. You can't add custom objects.");

            DrawInfoBox(customizableWidth.boolValue,
                "This style supports dynamic width. You can change width using Rect Transform freely.",
                "This style does not support dynamic width. Only use 'Scale' parameter to change the size.");

            DrawInfoBox(customizableHeight.boolValue,
                "This style supports dynamic height. You can change height using Rect Transform freely.",
                "This style does not support dynamic height. Only use 'Scale' parameter to change the size.");
        }

        private void DrawInfoBox(bool condition, string successMessage, string warningMessage)
        {
            if (condition)
            {
                EditorGUILayout.HelpBox(successMessage, MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
            }
        }

        private void DrawAnimationTab()
        {
            if (sTarget.inAnim == null || sTarget.outAnim == null)
            {
                EditorGUILayout.HelpBox("Animation variables are missing. Switch to Resources tab and assign the correct variables.", MessageType.Error);
                return;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(-3);
            sTarget.inspectAnim = MTPEditorHandler.DrawTogglePlain(sTarget.inspectAnim, customSkin, "Inspect Animations");
            GUILayout.Space(3);

            UpdateCurrentAnimation();
            HandleAnimationMode();
            DrawAnimationControls();

            GUILayout.EndVertical();

            HandleAnimationPlayback();
        }

        private void UpdateCurrentAnimation()
        {
            tempAnim = currentAnim == 0 ? sTarget.inAnim : sTarget.outAnim;
            UpdateAnimation();
        }

        private void HandleAnimationMode()
        {
            if (sTarget.inspectAnim == false && AnimationMode.InAnimationMode())
            {
                AnimationMode.EndSampling();
                AnimationMode.StopAnimationMode();
            }

            if (sTarget.inspectAnim)
            {
                if (tempAnim == null || sTarget.styleAnimator == null)
                {
                    EditorGUILayout.HelpBox("Animation cannot be found.", MessageType.Error);
                    return;
                }

                if (!AnimationMode.InAnimationMode())
                {
                    sTarget.tempAnimTime = 0;
                    AnimationMode.StartAnimationMode();
                }
            }
        }

        private void DrawAnimationControls()
        {
            if (tempAnim == null) return;

            float startTime = 0.0f;
            float stopTime = tempAnim.length;

            EditorGUI.BeginDisabledGroup(!sTarget.inspectAnim);
            
            GUILayout.Space(2);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Animation selector
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Selected Animation:"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            currentAnim = EditorGUILayout.Popup(currentAnim, animOptions);
            GUILayout.EndHorizontal();
            
            // Time labels
            GUILayout.BeginHorizontal();
            GUILayout.Space(33);
            EditorGUILayout.LabelField(new GUIContent(startTime.ToString("F2") + "s"), customSkin.FindStyle("Text"), GUILayout.Width(35));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(stopTime.ToString("F2") + "s"), customSkin.FindStyle("Text"), GUILayout.Width(35));
            GUILayout.EndHorizontal();
            
            // Animation icon and slider
            GUILayout.Space(-26);
            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("Anim Icon"));
            GUILayout.Space(-25);
            GUILayout.BeginHorizontal();
            GUILayout.Space(35);
            sTarget.tempAnimTime = EditorGUILayout.Slider(sTarget.tempAnimTime, startTime, stopTime);
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            
            // Control buttons
            GUILayout.BeginHorizontal();
            EditorGUI.EndDisabledGroup();

            if (!playAnim && GUILayout.Button("▶ Play"))
            {
                sTarget.inspectAnim = true;
                sTarget.tempAnimTime = 0;
                playAnim = true;
            }

            if (playAnim && GUILayout.Button("■ Stop"))
            {
                playAnim = false;
            }

            if (GUILayout.Button("Inspect via animation window"))
            {
                sTarget.inspectAnim = false;
                sTarget.tempAnimTime = 0;
                EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
            }

            GUILayout.EndHorizontal();

            // Info message
            if (sTarget.inspectAnim)
            {
                GUILayout.Space(3);
                EditorGUILayout.HelpBox("Animation is locked to 30 FPS while inspecting.\nTo see the animation at 60 FPS, use the animation window.", MessageType.Info);
            }

            GUILayout.EndVertical();
        }

        private void HandleAnimationPlayback()
        {
            if (!playAnim) return;

            sTarget.tempAnimTime += Time.deltaTime / 4f;
            Repaint();

            if (sTarget.tempAnimTime >= tempAnim.length)
            {
                playAnim = false;
            }
        }

        private void DrawResourcesTab()
        {
            var styleAnimator = serializedObject.FindProperty("styleAnimator");
            var editMode = serializedObject.FindProperty("editMode");
            var textItems = serializedObject.FindProperty("textItems");
            var imageItems = serializedObject.FindProperty("imageItems");
            var inAnim = serializedObject.FindProperty("inAnim");
            var outAnim = serializedObject.FindProperty("outAnim");
            var customContent = serializedObject.FindProperty("customContent");
            var customizableWidth = serializedObject.FindProperty("customizableWidth");
            var customizableHeight = serializedObject.FindProperty("customizableHeight");

            MTPEditorHandler.DrawProperty(styleAnimator, customSkin, "Style Animator");
            GUILayout.Space(2);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(-3);
            editMode.boolValue = MTPEditorHandler.DrawTogglePlain(editMode.boolValue, customSkin, "Dev Mode");
            GUILayout.Space(3);

            if (editMode.boolValue)
            {
                GUILayout.Space(3);
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(textItems, new GUIContent("Text Items"));
                EditorGUILayout.PropertyField(imageItems, new GUIContent("Image Items"));
                EditorGUI.indentLevel = 0;
                EditorGUILayout.PropertyField(inAnim, new GUIContent("In Anim"));
                EditorGUILayout.PropertyField(outAnim, new GUIContent("Out Anim"));
                EditorGUILayout.PropertyField(customContent, new GUIContent("Custom Content"));
                EditorGUILayout.PropertyField(customizableWidth, new GUIContent("Customizable Width"));
                EditorGUILayout.PropertyField(customizableHeight, new GUIContent("Customizable Height"));
            }

            GUILayout.EndVertical();
        }

        private void DrawSettingsTab()
        {
            var forceUpdate = serializedObject.FindProperty("forceUpdate");
            var playOnEnable = serializedObject.FindProperty("playOnEnable");
            var playOutAnimation = serializedObject.FindProperty("playOutAnimation");
            var disableOnOut = serializedObject.FindProperty("disableOnOut");
            var loopAnimations = serializedObject.FindProperty("loopAnimations");
            var useUnscaledTime = serializedObject.FindProperty("useUnscaledTime");
            var animationSpeed = serializedObject.FindProperty("animationSpeed");
            var showFor = serializedObject.FindProperty("showFor");
            var onEnable = serializedObject.FindProperty("onEnable");
            var onDisable = serializedObject.FindProperty("onDisable");

            MTPEditorHandler.DrawHeader(customSkin, "Style Top Header", 6);
            forceUpdate.boolValue = MTPEditorHandler.DrawToggle(forceUpdate.boolValue, customSkin, "Force To Update At Start");
            playOnEnable.boolValue = MTPEditorHandler.DrawToggle(playOnEnable.boolValue, customSkin, "Play On Enable");
            playOutAnimation.boolValue = MTPEditorHandler.DrawToggle(playOutAnimation.boolValue, customSkin, "Play Out Animation");
            disableOnOut.boolValue = MTPEditorHandler.DrawToggle(disableOnOut.boolValue, customSkin, "Disable On Out");
            loopAnimations.boolValue = MTPEditorHandler.DrawToggle(loopAnimations.boolValue, customSkin, "Loop Animations");
            useUnscaledTime.boolValue = MTPEditorHandler.DrawToggle(useUnscaledTime.boolValue, customSkin, "Use Unscaled Time");
            
            MTPEditorHandler.DrawProperty(animationSpeed, customSkin, "Animation Speed");
            MTPEditorHandler.DrawProperty(showFor, customSkin, "Show For (s)");

            // Info box for unscaled time
            if (useUnscaledTime.boolValue)
            {
                EditorGUILayout.HelpBox("Unscaled Time: Animations will play at normal speed regardless of Time.timeScale. Useful for pause menus and UI that should animate during paused gameplay.", MessageType.Info);
            }

            MTPEditorHandler.DrawHeader(customSkin, "Events Top Header", 10);
            EditorGUILayout.PropertyField(onEnable);
            EditorGUILayout.PropertyField(onDisable);
        }

        private void UpdateAnimation()
        {
            if (sTarget == null || sTarget.styleAnimator == null || 
                sTarget.styleAnimator.runtimeAnimatorController == null || tempAnim == null)
                return;

            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(sTarget.gameObject, tempAnim, sTarget.tempAnimTime);
                AnimationMode.EndSampling();
            }
        }

        private void DrawTextItem(int index)
        {
            if (index >= sTarget.textItems.Count || sTarget.textItems[index] == null)
                return;

            SerializedObject tempSerializedObj = new SerializedObject(sTarget.textItems[index]);
            var _text = tempSerializedObj.FindProperty("text");
            var _selectedFont = tempSerializedObj.FindProperty("selectedFont");
            var _fontSize = tempSerializedObj.FindProperty("fontSize");
            var _textColor = tempSerializedObj.FindProperty("textColor");

            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent(sTarget.textItems[index].itemID), customSkin.FindStyle("Text"));
            EditorGUILayout.PropertyField(_text, new GUIContent(""));

            GUILayout.Space(3);
            GUILayout.BeginHorizontal();

            if (sTarget.textItems[index].textObject != null && 
                !sTarget.textItems[index].textObject.enableAutoSizing)
            {
                EditorGUILayout.PropertyField(_fontSize, new GUIContent(""), GUILayout.Width(40));
                GUILayout.Space(3);
            }

            EditorGUILayout.PropertyField(_selectedFont, new GUIContent(""));
            GUILayout.Space(3);
            EditorGUILayout.PropertyField(_textColor, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.Space(3);

            if (GUILayout.Button("Select Object"))
            {
                Selection.activeObject = sTarget.textItems[index].textObject;
            }

            GUILayout.EndVertical();

            tempSerializedObj.ApplyModifiedProperties();
            sTarget.textItems[index].UpdateAll();
        }

        private void DrawImageItem(int index)
        {
            if (index >= sTarget.imageItems.Count || sTarget.imageItems[index] == null)
                return;

            SerializedObject tempSerializedObj = new SerializedObject(sTarget.imageItems[index]);
            var _imageColor = tempSerializedObj.FindProperty("imageColor");
            var _preferGradient = tempSerializedObj.FindProperty("preferGradient");
            var _imageGradient = tempSerializedObj.FindProperty("imageGradient");
            var _thickness = tempSerializedObj.FindProperty("thickness");

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(new GUIContent(sTarget.imageItems[index].itemID), customSkin.FindStyle("Text"));

            if (sTarget.imageItems[index].enableThickness)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent("Thickness"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                EditorGUILayout.PropertyField(_thickness, new GUIContent(""));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            _preferGradient.boolValue = GUILayout.Toggle(_preferGradient.boolValue, new GUIContent("Use Gradient"), customSkin.FindStyle("Toggle"), GUILayout.Width(125));
            _preferGradient.boolValue = GUILayout.Toggle(_preferGradient.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            if (_preferGradient.boolValue)
            {
                EditorGUILayout.PropertyField(_imageGradient, new GUIContent(""));
            }
            else
            {
                EditorGUILayout.PropertyField(_imageColor, new GUIContent(""));
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Select Object"))
            {
                Selection.activeObject = sTarget.imageItems[index].imageObject;
            }

            GUILayout.EndVertical();

            tempSerializedObj.ApplyModifiedProperties();
            sTarget.imageItems[index].UpdateAll();
        }
    }
}