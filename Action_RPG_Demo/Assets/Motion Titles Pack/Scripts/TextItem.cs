using UnityEngine;
using TMPro;

namespace Michsky.UI.MTP
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextItem : MonoBehaviour
    {
        [Header("Resources")]
        public string itemID = "Item ID";
        public string text;
        public StyleManager styleManager;

        [Header("Text Settings")]
        public TextMeshProUGUI textObject;
        public TMP_FontAsset selectedFont;
        public Color textColor = new Color(255, 255, 255, 255);
        public float fontSize;

        [Header("Twin Settings")]
        public bool isIdentical;
        public bool updateSubTwins;
        public TextItem twinObject;

        void Start()
        {
            if (textObject == null)
                textObject = gameObject.GetComponent<TextMeshProUGUI>();
        }

        void OnEnable()
        {
            if (styleManager != null && styleManager.forceUpdate == true)
                UpdateAll();
        }

        public void UpdateAll()
        {
            UpdateColor();
            UpdateFont();
            UpdateSize();
            UpdateText();

            if (isIdentical == true && updateSubTwins == true)
                twinObject.UpdateAll();
        }

        public void UpdateColor()
        {
            textObject.color = textColor;

            if (isIdentical == true && twinObject != null)
            {
                twinObject.textColor = textColor;
                twinObject.UpdateColor();
            }
        }

        public void UpdateFont()
        {
            textObject.font = selectedFont;

            if (isIdentical == true && twinObject != null)
                twinObject.textObject.font = selectedFont;
        }

        public void UpdateSize()
        {
            textObject.fontSize = fontSize;

            if (isIdentical == true && twinObject != null)
            {
                twinObject.fontSize = fontSize;
                twinObject.UpdateSize();
            }
        }

        public void UpdateText()
        {
            textObject.text = text;

            if (isIdentical == true && twinObject != null && twinObject.text != text)
            {
                twinObject.text = text;
                twinObject.UpdateText();
            }
        }
    }
}