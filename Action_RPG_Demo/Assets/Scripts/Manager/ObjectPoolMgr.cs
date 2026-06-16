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
    public GameObject GetObj(GameObject gameObj, Vector3 pos)
    {
        return GetObj(gameObj, pos, Quaternion.identity);
    }
    public GameObject GetObj(GameObject gameObj,Vector3 pos, Quaternion rot)
    {
        GameObject obj;
        string name = gameObj.name;
        if (PoolDic.ContainsKey(name) && PoolDic[name].Count > 0)
        {
            obj = PoolDic[name].Dequeue();
            obj.transform.SetParent(null);
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(gameObj, pos, rot, null);
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
        obj.transform.SetParent(PoolRoot.transform);
        if (!PoolDic.ContainsKey(name))
        {
            PoolDic.Add(name, new Queue<GameObject>());
        }
        PoolDic[name].Enqueue(obj);
    }
}
