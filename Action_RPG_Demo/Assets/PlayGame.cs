using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayGame : MonoBehaviour
{
    public Button Btn;
    public bool IsNewGame;

    public float DelayLoadTime = 4f;
    public Coroutine DelayCoroutine;

    public void Awake()
    {
        Btn = GetComponent<Button>();
    }

    public void OnEnable()
    {
        Btn.onClick.RemoveAllListeners();
        Btn.onClick.AddListener(StartGame);
    }

    public void OnDisable()
    {
        Btn.onClick.RemoveAllListeners();
        if (DelayCoroutine != null)
        {
            StopCoroutine(DelayCoroutine);
            DelayCoroutine = null;
        }
    }

    public void StartGame()
    {
        if (DelayCoroutine != null)
        {
            return;
        }
        DelayCoroutine = StartCoroutine(DelayStartLoad());
    }

    private IEnumerator DelayStartLoad()
    {
        if (IsNewGame)
        {
            string TargetDir = Application.persistentDataPath;
            DirectoryInfo DirInfo = new DirectoryInfo(TargetDir);
            foreach (FileInfo File in DirInfo.GetFiles())
            {
                try
                {
                    File.Delete();
                    Debug.Log($"ÒÑÉ¾³ýÎÄ¼þ£º{File.FullName}");
                }
                catch (System.Exception E)
                {
                    Debug.LogError($"É¾³ýÊ§°Ü {File.Name}£¬Òì³££º{E.Message}");
                }
            }
        }

        HallShow.instance.CameraLerpSpeed = 1;
        HallShow.instance.RequestEnter(HallShow.instance.ReadyToPlay);
        yield return new WaitForSecondsRealtime(DelayLoadTime);
        LoadingMgr.instance.StartTransition("City", true);
        DelayCoroutine = null;
    }
}