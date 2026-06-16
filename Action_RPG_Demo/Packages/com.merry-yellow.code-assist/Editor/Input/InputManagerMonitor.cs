using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


#pragma warning disable IDE0005
using Serilog = Meryel.Serilog;
#pragma warning restore IDE0005


#nullable enable


namespace Meryel.UnityCodeAssist.Editor.Input
{

    public class InputManagerMonitor : Monitors.FileMonitor
    {
        private static readonly Lazy<InputManagerMonitor> _instance = new Lazy<InputManagerMonitor>(() => new InputManagerMonitor());
        public static InputManagerMonitor Instance => _instance.Value;

        //UnityInputManager inputManager;


        public InputManagerMonitor()
            : base(CommonTools.GetInputManagerFilePath())
        {
        }

#if !ENABLE_LEGACY_INPUT_MANAGER
        protected override bool IsDisabled => true;
#endif


        protected override void BumpAux()
        {
            Serilog.Log.Debug("InputMonitor {Event}", nameof(BumpAux));

            var inputManagerFilePath = CommonTools.GetInputManagerFilePath();

            if (!System.IO.File.Exists(inputManagerFilePath))
            {
                Serilog.Log.Error("InputManager file not found at {location}", inputManagerFilePath);
                return;
            }

            var inputManager = new UnityInputManager();
            inputManager.ReadFromPath(inputManagerFilePath);
            inputManager.SendData();
        }

    }


    public static partial class Extensions
    {
        public static string GetInfo(this List<InputAxis> axes, string? name)
        {
            if (name == null || string.IsNullOrEmpty(name))
                return string.Empty;

            //axis.descriptiveName
            var axesWithName = axes.Where(a => a.Name == name);

            int threshold = 80;

            var sb = new System.Text.StringBuilder();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.descriptiveName))
                    sb.Append($"{axis.descriptiveName} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.descriptiveNegativeName))
                    sb.Append($"{axis.descriptiveNegativeName} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.positiveButton))
                    sb.Append($"[{axis.positiveButton}] ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.altPositiveButton))
                    sb.Append($"{{{axis.altPositiveButton}}} ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.negativeButton))
                    sb.Append($"-[{axis.negativeButton}] ");

            if (sb.Length > threshold)
                return sb.ToString();

            foreach (var axis in axesWithName)
                if (!string.IsNullOrEmpty(axis.altNegativeButton))
                    sb.Append($"-{{{axis.altNegativeButton}}} ");

            return sb.ToString();
        }
    }

}
