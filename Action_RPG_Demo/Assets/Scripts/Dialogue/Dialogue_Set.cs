using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Dialogue_Set : BaseActor
{
    public Dialogue_SO Story_Dialogue;
    public Dialogue_SO Chat_Dialogue;
    public Dialogue_SO Cur_Dialogue;
    public PlayableDirector director;
    [EntityId]
    public string CharacterId;
    public void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }
    public void Start()
    {
        Switch_DialogueSO();
    }
    public void Update()
    {
    }
    public void Switch_DialogueSO()
    {
        Panel_Mgr.instance.HideAllPanel();
        Cur_Dialogue = Story_Dialogue ? Story_Dialogue : Chat_Dialogue;
        DialogueWriter writer = Panel_Mgr.instance.DialoguePanel.GetComponent<DialogueWriter>();
        writer.ClearAllChoice();
    }
    public void PlayNpcAction(ActionSO action)
    {
        director.Stop();
        director.playableAsset = action.timeline;
        director.Play();
    }
    //public void OnNpcCameraEnd()
    //{
    //    CameraPivot camPivot = CameraPivot.instance;
    //    if (camPivot == null)
    //    {
    //        return;
    //    }
    //    camPivot.rotX = camRotX;
    //    camPivot.rotY = camRotY;
    //    camPivot.distance = camDist;
    //    camPivot.height = camHeight;
    //    camPivot.TargetDistance = camDist;
    //    camPivot.transform.rotation = camRot;
    //}
}
