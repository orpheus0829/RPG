using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseActor : MonoBehaviour
{
    public virtual void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        SoundMgr.instance.PlaySingleSound(clip, this.transform.root.gameObject);
    }
    public virtual GameObject SpawnEffect(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null)
        {
            return null;
        }
        return ObjectPoolMgr.instance.GetObj(prefab, pos, rot);
    }
}
