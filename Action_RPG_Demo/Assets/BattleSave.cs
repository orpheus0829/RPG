using System.Collections;
using UnityEngine;

public class BattleSave : MonoBehaviour
{
    public static BattleSave instance { get; private set; }

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    public void Start()
    {
        StartCoroutine(DelaySpawnEnemy(2f));
    }
    IEnumerator DelaySpawnEnemy(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        if (Story_Mgr.instance.CurQuest is FightQuest_SO fight)
        {
            //Story_Mgr.instance.Refresh_StoryProgress();
            NavPathMgr.instance.OpenNavPath(Story_Mgr.instance.CurQuestPos);
        }
    }
}