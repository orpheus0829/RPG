using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.MTP
{
    [RequireComponent(typeof(Image))]
    public class ImageItem : MonoBehaviour
    {
        [Header("Resources")]
        public string itemID = "Item ID";
        public StyleManager styleManager;     
        public Image imageObject;
        public UIGradient gradientComponent;

        [Header("Color Settings")]
        public bool preferGradient;
        public Color imageColor = new Color(255, 255, 255, 255);
        public Gradient imageGradient;

        [Header("Thickness Settings")]
        public bool enableThickness;
        public bool isHorizontal = true;
        [Range(1, 50)] public float thickness;

        [Header("Twin Settings")]
        public bool isIdentical;
        public bool updateSubTwins;
        public ImageItem twinObject;

        void Start()
        {
            if (imageObject == null)
                imageObject = gameObject.GetComponent<Image>();

            if (styleManager != null && styleManager.forceUpdate == true)
                UpdateAll();
        }

        public void UpdateAll()
        {
            UpdateSprite();
            UpdateColor();

            if (isIdentical == true && updateSubTwins == true)
                twinObject.UpdateAll();
        }

        public void UpdateSprite()
        {
            if (enableThickness == false)
                return;

            if (isHorizontal == true)
            {
                imageObject.rectTransform.sizeDelta = new Vector2(thickness, imageObject.rectTransform.sizeDelta.y);

                if (isIdentical == true)
                    twinObject.thickness = thickness;
            }

            else
            {
                imageObject.rectTransform.sizeDelta = new Vector2(imageObject.rectTransform.sizeDelta.x, thickness);

                if (isIdentical == true)
                    twinObject.thickness = thickness;
            }
        }

        public void UpdateColor()
        {
            if (preferGradient == false)
            {
                imageObject.color = imageColor;

                if (gradientComponent != null)
                    gradientComponent.enabled = false;

                if (isIdentical == true && twinObject != null)
                {
                    twinObject.preferGradient = false;
                    twinObject.imageColor = imageColor;

                    if (twinObject.gradientComponent != null)
                        twinObject.gradientComponent.enabled = false;
                }
            }

            else if (preferGradient == true && gradientComponent != null)
            {
                imageObject.color = new Color(255, 255, 255, 255);
                gradientComponent.enabled = true;
                gradientComponent.EffectGradient = imageGradient;

                if (isIdentical == true && twinObject != null)
                {
                    twinObject.preferGradient = true;
                    twinObject.imageObject.color = new Color(255, 255, 255, 255);
                    twinObject.gradientComponent.enabled = true;
                    twinObject.imageGradient = imageGradient;
                }
            }
        }
    }
}