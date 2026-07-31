using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenEmoji : MonoBehaviour
{
    public GameObject OpenObj;
    public Button Btn;
    public void Awake()
    {
        Btn = GetComponent<Button>();
        OpenObj.SetActive(false);
    }
    public void OnEnable()
    {
        Game_Event.instance.ShutEmoji += ShutDown;

        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(() =>
        {
            OpenObj.SetActive(!OpenObj.activeSelf);
        });
    }
    public void OnDisable()
    {
        Game_Event.instance.ShutEmoji -= ShutDown;

        Btn.onClick.RemoveAllListeners();
    }
    public void ShutDown()
    {
        OpenObj.SetActive(false);
    }
}
