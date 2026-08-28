using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#else
using UnityEngine.EventSystems;
#endif

namespace Michsky.UI.MTP
{
    public class InputSystemChecker : MonoBehaviour
    {
        void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (!gameObject.TryGetComponent<InputSystemUIInputModule>(out var _))
            {
                gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (!gameObject.TryGetComponent<StandaloneInputModule>(out var _))
            {
                gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}