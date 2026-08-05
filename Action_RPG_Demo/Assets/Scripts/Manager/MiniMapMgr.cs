using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MapStyle
{
    Min,
    Max,
}
public class MiniMapMgr : Base_mgr<MiniMapMgr>
{
    [Header("绑定对象")]
    public RectTransform SelfRoot;
    public Transform player;
    public Camera miniMapCam;
    public Image mapRenderImg;
    public Image playerArrow;
    public Transform MarksParent;

    [Header("切换大小")]
    public Vector3 MinPos;
    public float MinScale;
    public Vector3 MaxPos;
    public float MaxScale;

    [Header("图标素材与预制体")]
    public Image MarkPrefab;
    public Sprite NPCSprite;
    public Sprite MonsterSprite;

    [Header("地图可视配置")]
    public float mapViewHalf = 60f;
    public float uiMapSize = 200f;

    [Header("图标显示控制")]
    public float showMaxDistance = 120f;
    public float markRefreshInterval = 0.05f;
    public Vector2 markIconSize = new Vector2(12, 12);

    [Header("追踪功能")]
    public GameObject trackingTarget;

    public RectTransform _arrowRt;
    public List<Image> npcMarkList = new List<Image>();
    public List<Image> monsterMarkList = new List<Image>();
    private float _refreshTimer;

    protected override void Awake()
    {
        base.Awake();
        _arrowRt = playerArrow.rectTransform;
        SelfRoot = GetComponent<RectTransform>();
        if (mapRenderImg != null)
        {
            mapRenderImg.raycastTarget = false;
        }
    }

    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Update()
    {
        if (!miniMapCam)
        {
            miniMapCam = GameObject.FindGameObjectWithTag("MapCam").GetComponent<Camera>();
        }
        FollowPlayerCamera();
        SyncPlayerIcon();
        //追踪
        if (trackingTarget != null)
        {
            if (trackingTarget == null || !trackingTarget.activeSelf)
            {
                trackingTarget = null;
                NavPathMgr.instance.CloseNavPath();
            }
            else
            {
                NavPathMgr.instance.SwitchNavTarget(trackingTarget.transform.position);
                NavPathMgr.instance.OpenNavPath(NavPathMgr.instance.targetPoint);
            }
        }

        _refreshTimer += Time.deltaTime;
        if (_refreshTimer >= markRefreshInterval)
        {
            RefreshAllEntityMark();
            _refreshTimer = 0;
        }
        if (Panel_Mgr.instance.CurMapStyle == MapStyle.Max)
        {
            SelfRoot.anchoredPosition = MaxPos;
            SelfRoot.localScale = new Vector2(MaxScale, MaxScale);
        }
        else
        {
            SelfRoot.anchoredPosition = MinPos;
            SelfRoot.localScale = new Vector2(MinScale, MinScale);
        }
    }

    public void FollowPlayerCamera()
    {
        if (!player)
        {
            return;
        }
        Vector3 targetPos = player.position;
        targetPos.y = miniMapCam.transform.position.y;
        miniMapCam.transform.position = Vector3.Lerp(miniMapCam.transform.position, targetPos, Time.deltaTime * 8f);
    }

    public void SyncPlayerIcon()
    {
        if (player == null || miniMapCam == null)
        {
            return;
        }
        Vector3 camWorldPos = miniMapCam.transform.position;
        Vector3 worldPos = player.position;
        float relativeX = worldPos.x - camWorldPos.x;
        float relativeZ = worldPos.z - camWorldPos.z;
        float distance = Mathf.Max(Mathf.Abs(relativeX), Mathf.Abs(relativeZ));
        if (distance > mapViewHalf)
        {
            _arrowRt.gameObject.SetActive(false);
            return;
        }
        _arrowRt.gameObject.SetActive(true);
        float uiX = (relativeX / mapViewHalf) * uiMapSize;
        float uiY = (relativeZ / mapViewHalf) * uiMapSize;
        _arrowRt.anchoredPosition = new Vector2(uiX, uiY);
        float yAngle = player.eulerAngles.y;
        _arrowRt.rotation = Quaternion.Euler(0, 0, -yAngle);
    }

    public void RefreshAllEntityMark()
    {
        HideAllMark();
        if (player == null) return;
        Vector3 playerWorldPos = player.position;
        // NPC标记处理
        GameObject[] allNpc = GameObject.FindGameObjectsWithTag("NPC");
        int activeNpcCount = 0;
        foreach (var npc in allNpc)
        {
            if (npc == null) continue;
            float dist = Vector3.Distance(npc.transform.position, playerWorldPos);
            if (dist <= showMaxDistance) activeNpcCount++;
        }
        while (npcMarkList.Count < activeNpcCount)
        {
            SpawnNewMark(ref npcMarkList);
        }
        int usableIndex = 0;
        for (int i = 0; i < allNpc.Length; i++)
        {
            GameObject npcObj = allNpc[i];
            if (npcObj == null) continue;
            Transform target = npcObj.transform;
            float distToPlayer = Vector3.Distance(target.position, playerWorldPos);
            if (distToPlayer > showMaxDistance) continue;
            UpdateSingleMark(target, ref npcMarkList, NPCSprite, usableIndex);
            usableIndex++;
        }
        //怪物标记处理
        GameObject[] allMonster = GameObject.FindGameObjectsWithTag("Enemy");
        int activeMonsterCount = 0;
        foreach (var monster in allMonster)
        {
            if (monster == null)
            {
                continue;
            }
            float dist = Vector3.Distance(monster.transform.position, playerWorldPos);
            if (dist <= showMaxDistance)
            {
                activeMonsterCount++;
            }
        }
        while (monsterMarkList.Count < activeMonsterCount)
        {
            SpawnNewMark(ref monsterMarkList);
        }
        usableIndex = 0;
        for (int i = 0; i < allMonster.Length; i++)
        {
            GameObject monsterObj = allMonster[i];
            if (monsterObj == null) continue;
            Transform target = monsterObj.transform;
            float distToPlayer = Vector3.Distance(target.position, playerWorldPos);
            if (distToPlayer > showMaxDistance) continue;
            UpdateSingleMark(target, ref monsterMarkList, MonsterSprite, usableIndex);
            usableIndex++;
        }
    }
    public void SpawnNewMark(ref List<Image> markPool)
    {
        GameObject poolObj = ObjectPoolMgr.instance.GetObj(MarkPrefab.gameObject, MarksParent);
        MapPointer pointer = poolObj.GetComponent<MapPointer>();
        if (pointer == null)
        {
            pointer = poolObj.AddComponent<MapPointer>();
        }
        Image markImg = poolObj.GetComponent<Image>();
        markImg.raycastTarget = true;
        markPool.Add(markImg);
    }
    public void UpdateSingleMark(Transform target, ref List<Image> markPool, Sprite iconSprite, int index)
    {
        if (target == null || iconSprite == null || MarksParent == null || miniMapCam == null || MarkPrefab == null)
        {
            return;
        }
        Image markImg = markPool[index];
        MapPointer pointer = markImg.GetComponent<MapPointer>();
        GameObject targetObj = target.gameObject;
        pointer.PointTo = targetObj;
        markImg.gameObject.SetActive(true);
        markImg.sprite = iconSprite;
        RectTransform rt = markImg.rectTransform;
        rt.sizeDelta = markIconSize;
        Vector3 worldPos = target.position;
        Vector3 camWorldPos = miniMapCam.transform.position;
        float relativeX = worldPos.x - camWorldPos.x;
        float relativeZ = worldPos.z - camWorldPos.z;
        float uiX = (relativeX / mapViewHalf) * uiMapSize;
        float uiY = (relativeZ / mapViewHalf) * uiMapSize;
        rt.anchoredPosition = new Vector2(uiX, uiY);
        rt.rotation = Quaternion.Euler(0, 0, -target.eulerAngles.y);
    }

    public void HideAllMark()
    {
        foreach (var img in npcMarkList)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
        foreach (var img in monsterMarkList)
        {
            if (img != null) img.gameObject.SetActive(false);
        }
    }
    public void ClearAllMark()
    {
        foreach (Image img in npcMarkList)
        {
            if (img != null)
            {
                ObjectPoolMgr.instance.PushObj(img.gameObject);
            }
        }
        foreach (Image img in monsterMarkList)
        {
            if (img != null)
            {
                ObjectPoolMgr.instance.PushObj(img.gameObject);
            }
        }
        npcMarkList.Clear();
        monsterMarkList.Clear();
        trackingTarget = null;
        NavPathMgr.instance.CloseNavPath();
    }

    private void OnDestroy()
    {
        //ClearAllMark();
    }
}