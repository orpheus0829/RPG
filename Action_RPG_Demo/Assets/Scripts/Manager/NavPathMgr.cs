using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavPathMgr : Base_mgr<NavPathMgr>
{
    public bool IsGuiding;
    [Header("µº∫Ω¬∑æ∂…Ë÷√")]
    public Material navFlowMat;
    public float lineWidth = 0.4f;
    public float groundOffset = 0.06f;
    public float flowSpeed = 1.2f;
    public float refreshInterval = 0.15f;
    public Vector2 GuideSize;

    private LineRenderer pathLine;
    private NavMeshPath navPath;
    public Transform player;
    public Vector3 targetPoint;
    private float lastRefreshTime;
    private Material runtimeMat;
    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(this.gameObject);
        }

        //if (!player)
        //{
        //    player = GameObject.FindGameObjectWithTag("Player").transform;
        //}
        navPath = new NavMeshPath();
        GameObject pathObj = new GameObject("FlowNavPath");
        pathObj.transform.SetParent(this.transform);
        pathLine = pathObj.AddComponent<LineRenderer>();
        runtimeMat = new Material(navFlowMat);
        pathLine.textureMode = LineTextureMode.Tile;
        pathLine.alignment = LineAlignment.View;
        pathLine.textureScale = new Vector2(GuideSize.y, GuideSize.x);
        pathLine.material = runtimeMat;
        pathLine.startWidth = lineWidth;
        pathLine.endWidth = lineWidth;
        pathLine.useWorldSpace = true;
        pathLine.loop = false;
        pathLine.positionCount = 0;
        pathLine.gameObject.SetActive(false);
    }
    public void Start()
    {
        targetPoint = Story_Mgr.instance.CurQuestPos;
        //OpenNavPath(targetPoint);
    }
    public void Update()
    {
        IsGuiding = targetPoint != new Vector3(0, 0, 0);
        if (targetPoint == null || !pathLine.gameObject.activeSelf)
        {
            return;
        }
        Vector2 currentOffset = runtimeMat.mainTextureOffset;
        currentOffset.x -= flowSpeed * Time.deltaTime;
        runtimeMat.mainTextureOffset = currentOffset;
        if (Time.time - lastRefreshTime > refreshInterval)
        {
            lastRefreshTime = Time.time;
            if (targetPoint != null)
            {
                RefreshPath();
            }
        }
    }
    public void OpenNavPath(Vector3 target)
    {
        targetPoint = target;
        pathLine.gameObject.SetActive(true);
        RefreshPath();
    }

    public void CloseNavPath()
    {
        if (pathLine)
        {
            pathLine.positionCount = 0;
            pathLine.gameObject.SetActive(false);
        }
    }
    public void SwitchNavTarget(Vector3 target)
    {
        targetPoint = target;
    }
    private void RefreshPath()
    {
        if (!player || targetPoint == Vector3.zero || navPath == null)
        {
            return;
        }
        bool findPath = NavMesh.CalculatePath(player.position, targetPoint, NavMesh.AllAreas, navPath);
        if (!findPath || navPath.corners.Length < 2)
        {
            pathLine.positionCount = 0;
            return;
        }
        Vector3[] pathPoints = navPath.corners;
        pathLine.positionCount = pathPoints.Length;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i].y += groundOffset;
            pathLine.SetPosition(i, pathPoints[i]);
        }
    }
}
