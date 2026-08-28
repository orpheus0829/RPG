using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace Michsky.UI.MTP
{
    public class StyleVideoPreview : EditorWindow
    {
        static StyleVideoPreview window;
        private VideoPlayer player;
        private Texture currentRT;
        public VideoClip videoClip;
        public GameObject tempGO;

        // Cache the style so we don't create it every frame
        private GUIStyle loadingStyle;

        void OnEnable()
        {
            window = GetWindow<StyleVideoPreview>();
            window.minSize = new Vector2(480, 320);
        }

        void OnDisable()
        {
            if (player != null)
                DestroyImmediate(player);

            if (tempGO != null)
                DestroyImmediate(tempGO);
        }

        void OnGUI()
        {
            // Force constant repainting so the video updates smoothly
            Repaint();

            // 1. If we have a texture from the player, draw it
            if (currentRT != null)
            {
                EditorGUI.DrawPreviewTexture(new Rect(0, 0, position.width, position.height), currentRT);
            }
            // 2. If no texture yet, draw the Loading UI
            else
            {
                // Draw a black background
                EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), Color.black);

                // Initialize style if null
                if (loadingStyle == null)
                {
                    loadingStyle = new GUIStyle(GUI.skin.label);
                    loadingStyle.alignment = TextAnchor.MiddleCenter;
                    loadingStyle.fontSize = 14;
                    loadingStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f); // Light gray
                }

                // Draw centered text
                GUI.Label(new Rect(0, 0, position.width, position.height), "Loading Video...", loadingStyle);
            }
        }

        private void PlayerFrameReady(VideoPlayer source, long frameIdx)
        {
            // Helper: Once the first frame is ready, this fills the texture
            currentRT = source.texture;
        }

        public void UpdateVideo(string URL, GUIContent title = null)
        {
            // IMPORTANT: Reset the texture to null to trigger the "Loading..." screen
            currentRT = null;

            if (tempGO == null)
            {
                var newTempGO = new GameObject("[MTP - Temp Object]");
                tempGO = newTempGO;
                // Hide the temp object from the hierarchy to keep it clean
                tempGO.hideFlags = HideFlags.HideAndDontSave;
            }

            if (player == null)
                player = tempGO.AddComponent<VideoPlayer>();

            // Set the window title if provided
            if (title != null)
                titleContent = title;

            player.url = URL;
            player.audioOutputMode = VideoAudioOutputMode.None;
            player.playOnAwake = false;
            player.clip = videoClip;
            player.isLooping = true;

            player.Prepare();

            player.sendFrameReadyEvents = true;

            // Ensure we don't subscribe multiple times if UpdateVideo is called repeatedly
            player.frameReady -= PlayerFrameReady;
            player.frameReady += PlayerFrameReady;

            player.Play();
        }
    }
}