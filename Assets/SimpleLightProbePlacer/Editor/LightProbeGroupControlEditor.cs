using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleLightProbePlacer.Editor
{
    [CustomEditor(typeof(LightProbeGroupControl))]
    public class LightProbeGroupControlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var control = (LightProbeGroupControl)target;

            if (GUILayout.Button("Delete All Light Probes"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - delete all");
                control.DeleteAll();
            }

            if (control.LightProbeGroup != null)
            {
                string message = "Light Probes count: {0}\nMerged Probes: {1}";
                message = string.Format(message, control.LightProbeGroup.probePositions.Length, control.MergedProbes);

                EditorGUILayout.HelpBox(message, MessageType.Info);
            }

            if (GUILayout.Button("Create Light Probes"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - create");
                control.Create();
            }
        
            GUILayout.Space(10);

            if (GUILayout.Button("Merge Closest Light Probes"))
            {
                Undo.RecordObject(control.LightProbeGroup, "Light Probe Group - merge");
                control.Merge();
            }

            EditorGUI.BeginChangeCheck();

            var mergeDist = EditorGUILayout.Slider("Merge distance", control.MergeDistance, 0, 10);
        
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Point Light Settings", EditorStyles.boldLabel);

            var useLights = EditorGUILayout.Toggle("Use Point Lights", control.UsePointLights);
        
            GUI.enabled = control.UsePointLights;
            var lightRange = EditorGUILayout.FloatField("Range", control.PointLightRange);
            GUI.enabled = true;
        
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(control, "Light Probe Group Control changes");
            
                control.MergeDistance = mergeDist;
                control.UsePointLights = useLights;
                control.PointLightRange = lightRange;

                EditorUtility.SetDirty(target);
            }
        }

        [MenuItem("GameObject/Light/Light Probe Group Control")]
        private static void CreateLightProbeGroupControl(MenuCommand menuCommand)
        {
            var go = new GameObject("Light Probe Group Control");

            go.AddComponent<LightProbeGroupControl>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Light Probe Group Control");

            Selection.activeGameObject = go;
        }
        
        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.Active)]
        private static void DrawGizmoPointLight(Light light, GizmoType gizmoType)
        {
            var control = FindObjectOfType<LightProbeGroupControl>();
        
            if (control == null || !control.UsePointLights || light.type != LightType.Point) return;
        
            List<Vector3> probes = LightProbeGroupControl.CreatePositionsAround(light.transform, control.PointLightRange);

            for (int i = 0; i < probes.Count; i++)
            {
                Gizmos.DrawIcon(probes[i], "NONE", false);
            }
        }
    }
}
