using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolMgr : Base_mgr<ObjectPoolMgr>
{
    private GameObject PoolRoot;
    public Dictionary<string, Queue<GameObject>> PoolDic = new Dictionary<string, Queue<GameObject>>();
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
            PoolRoot = new GameObject("PoolRoot");
            PoolRoot.transform.SetParent(this.transform);
        }
    }
    public GameObject GetObj(GameObject gameObj, Transform parent)
    {
        return GetObj(gameObj, parent.position, parent.rotation, parent);
    }
    public GameObject GetObj(GameObject gameObj, Vector3 pos)
    {
        return GetObj(gameObj, pos, Quaternion.identity, null);
    }
    public GameObject GetObj(GameObject gameObj, Vector3 pos, Quaternion rot)
    {
        return GetObj(gameObj, pos, rot, null);
    }
    private GameObject GetObj(GameObject gameObj, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject obj;
        string name = gameObj.name;
        if (PoolDic.ContainsKey(name) && PoolDic[name].Count > 0)
        {
            obj = PoolDic[name].Dequeue();
            obj.transform.SetParent(null);
            obj.transform.SetPositionAndRotation(pos, rot);
            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(gameObj, pos, rot, parent);
            obj.name = name;
        }
        return obj;
    }
    public void PushObj(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        string name = obj.name;
        obj.SetActive(false);
        obj.transform.SetParent(null, true);
        obj.transform.SetParent(PoolRoot.transform);
        if (!PoolDic.ContainsKey(name))
        {
            PoolDic.Add(name, new Queue<GameObject>());
        }
        PoolDic[name].Enqueue(obj);
    }
}
