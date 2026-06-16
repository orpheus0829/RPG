using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


#pragma warning disable IDE0005
using Serilog = Meryel.Serilog;
#pragma warning restore IDE0005


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Monitors
{
    public abstract class FileMonitor
    {
        readonly string filePath;
        DateTime previousLastWrite;

        public const double CheckInterval = 0.1;
        double lastCheckTime;

        protected virtual bool IsDisabled => false;

        protected FileMonitor(string filePath)
        {
            this.filePath = filePath;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            if (!System.IO.File.Exists(filePath))
            {
                Serilog.Log.Error("File not found at {location}", filePath);
                return;
            }

            try
            {
                previousLastWrite = System.IO.File.GetLastWriteTime(filePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug(ex, "Exception at {Location}", nameof(System.IO.File.GetLastWriteTime));
            }
        }

        void Update()
        {
            if (IsDisabled)
                return;

            // ⬇ throttle to 0.1s
            var now = EditorApplication.timeSinceStartup;
            if (now - lastCheckTime < CheckInterval)
                return;

            lastCheckTime = now;

            var currentLastWrite = previousLastWrite;

            try
            {
                if (System.IO.File.Exists(filePath))
                    currentLastWrite = System.IO.File.GetLastWriteTime(filePath);
                else
                    return;
            }
            catch (Exception ex)
            {
                Serilog.Log.Debug(ex, "Exception at {Location}", nameof(System.IO.File.GetLastWriteTime));
                return;
            }

            if (currentLastWrite != previousLastWrite)
            {
                previousLastWrite = currentLastWrite;
                Bump();
            }
        }


        public void Bump()
        {
            if (IsDisabled)
                return;

            BumpAux();
        }

        protected abstract void BumpAux();
    }

    public class NavMeshAreasMonitor : FileMonitor
    {
        private static readonly Lazy<NavMeshAreasMonitor> _instance = new Lazy<NavMeshAreasMonitor>(() => new NavMeshAreasMonitor());
        public static NavMeshAreasMonitor Instance => _instance.Value;

       

        private NavMeshAreasMonitor()
            : base(CommonTools.GetNavMeshAreasFilePath())
        {
        }



        protected override void BumpAux()
        {
#if UNITY_6000_0_OR_NEWER
            var areas = UnityEngine.AI.NavMesh.GetAreaNames();
#else
            var areas = GameObjectUtility.GetNavMeshAreaNames();
#endif
            var indices = new string[areas.Length];

            for (int i = 0; i < areas.Length; i++)
            {
                var area = areas[i];

#if UNITY_6000_0_OR_NEWER
                var index = UnityEngine.AI.NavMesh.GetAreaFromName(area);
#else
                var index = GameObjectUtility.GetNavMeshAreaFromName(area);
#endif

                indices[i] = index.ToString();
            }


            var agentCount = UnityEngine.AI.NavMesh.GetSettingsCount();
            var navMeshAgents = new string[agentCount];
            var navMeshAgentIndices = new string[agentCount];
            for (int i = 0; i < agentCount; i++)
            {
                var s = UnityEngine.AI.NavMesh.GetSettingsByIndex(i);
                var id = UnityEngine.AI.NavMesh.GetSettingsNameFromID(s.agentTypeID);

                navMeshAgents[i] = id.ToString();
                navMeshAgentIndices[i] = i.ToString();
            }


            MQTTnetInitializer.Publisher?.SendNavMeshAreas(areas, indices, navMeshAgents, navMeshAgentIndices);
        }

    }

}
