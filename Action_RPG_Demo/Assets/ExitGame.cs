using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
    public Button btn;
    public void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
    }
    public void OnEnable()
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(LoadingMgr.instance.StartQuitTransition);
    }
    public void OnDisable()
    {
        btn.onClick.RemoveAllListeners();
    }
}
