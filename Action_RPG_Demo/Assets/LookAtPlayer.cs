using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Canvas Canvas;
    public GameObject Target;
    public void Awake()
    {
        Canvas = GetComponent<Canvas>();
        if (!Target)
        {
            Target = CameraPivot.instance.transform.GetChild(0).gameObject;
        }
    }
    public void LateUpdate()
    {
        if (!Target)
        {
            Target = CameraPivot.instance.camTrans.gameObject;
            return;
        }
        Vector3 dir = Target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-dir);
    }
}
