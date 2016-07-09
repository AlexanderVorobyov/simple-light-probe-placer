using UnityEditor;
using UnityEngine;

namespace SimpleLightProbePlacer.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(LightProbeVolume))]
    public class LightProbeVolumeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var volume = (LightProbeVolume)target;

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Volume", EditorStyles.boldLabel);
            var origin = EditorGUILayout.Vector3Field("Origin", volume.Origin);
            var size = EditorGUILayout.Vector3Field("Size", volume.Size);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Density", EditorStyles.boldLabel);
            var type = (LightProbeVolumeType)EditorGUILayout.EnumPopup("Density Type", volume.Type);
            
            float densityMin = volume.Type == LightProbeVolumeType.Fixed ? 1 : 0.1f;
            float densityMax = volume.Type == LightProbeVolumeType.Fixed ? 100 : 50;
        
            var density = volume.Density;
            density.x = EditorGUILayout.Slider("DensityX", volume.Density.x, densityMin, densityMax);
            density.y = EditorGUILayout.Slider("DensityY", volume.Density.y, densityMin, densityMax);
            density.z = EditorGUILayout.Slider("DensityZ", volume.Density.z, densityMin, densityMax);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Light Probe Volume changes");

                volume.Density = density;
                volume.Type = type;
                volume.Volume = new Volume(origin, size);

                EditorUtility.SetDirty(target);
            }
        }
    
        private void OnSceneGUI()
        {
            var lightProbeVolume = (LightProbeVolume)target;

            var volume = TransformVolume.EditorVolumeControl(lightProbeVolume, 0.1f, LightProbeVolume.EditorColor);

            if (volume != lightProbeVolume.Volume)
            {
                Undo.RecordObject(target, "Light Probe Volume changes");
                lightProbeVolume.Volume = volume;
                EditorUtility.SetDirty(target);
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.Active)]
        private static void DrawGizmoVolume(LightProbeVolume volume, GizmoType gizmoType)
        {
            var color = LightProbeVolume.EditorColor;
            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(volume.transform.position, volume.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(volume.Origin, volume.Size);
            
            if (gizmoType != (GizmoType.Selected | GizmoType.InSelectionHierarchy | GizmoType.Active)) return;
            
            color.a = 0.25f;
            Gizmos.color = color;
            Gizmos.DrawCube(volume.Origin, volume.Size);

            var probes = volume.CreatePositions();

            for (int i = 0; i < probes.Count; i++)
            {
                Gizmos.DrawIcon(probes[i], "NONE", false);
            }
        }

        [MenuItem("GameObject/Light/Light Probe Volume")]
        private static void CreateLightProbeVolume(MenuCommand menuCommand)
        {
            var go = new GameObject("Light Probe Volume");

            go.AddComponent<LightProbeVolume>();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Light Probe Volume");

            Selection.activeGameObject = go;
        }
    }
}
