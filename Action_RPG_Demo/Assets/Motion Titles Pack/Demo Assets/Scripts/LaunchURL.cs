using UnityEngine;

namespace Michsky.UI.MTP
{
    public class LaunchURL : MonoBehaviour
    {
        public void OpenURL(string goURL)
        {
            Application.OpenURL(goURL);
        }
    }
}