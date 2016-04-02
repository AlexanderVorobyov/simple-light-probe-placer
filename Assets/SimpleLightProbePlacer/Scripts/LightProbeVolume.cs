using UnityEngine;
using System.Collections.Generic;

namespace SimpleLightProbePlacer
{
    public enum LightProbeVolumeType { Fixed, Float }

    [AddComponentMenu("Rendering/Light Probe Volume")]
    public class LightProbeVolume : TransformVolume
    {
        public static Color EditorColor = new Color(1, 0.9f, 0.25f);

        [Space(5)]
        public LightProbeVolumeType Type = LightProbeVolumeType.Fixed;

        [SerializeField] [HideInInspector] private Vector3 m_densityFixed = Vector3.one;
        [SerializeField] [HideInInspector] private Vector3 m_densityFloat = Vector3.one;

        public Vector3 Density
        {
            get { return Type == LightProbeVolumeType.Fixed ? m_densityFixed : m_densityFloat; }
            set
            {
                if (Type == LightProbeVolumeType.Fixed) m_densityFixed = value;
                else m_densityFloat = value;
            }
        }
        
        public List<Vector3> CreatePositions()
        {
            return CreatePositions(Type);
        }

        public List<Vector3> CreatePositions(LightProbeVolumeType type)
        {
            return type == LightProbeVolumeType.Fixed
                ? CreatePositionsFixed(transform, Origin, Size, Density)
                : CreatePositionsFloat(transform, Origin, Size, Density);
        }

        public static List<Vector3> CreatePositionsFixed(Transform volumeTransform, Vector3 origin, Vector3 size, Vector3 density)
        {
            List<Vector3> posList = new List<Vector3>();
            var offset = origin;

            var moveX = size.x / Mathf.FloorToInt(density.x);
            var moveY = size.y / Mathf.FloorToInt(density.y);
            var moveZ = size.z / Mathf.FloorToInt(density.z);

            offset -= size * 0.5f;

            for (int x = 0; x <= density.x; x++)
            {
                for (int y = 0; y <= density.y; y++)
                {
                    for (int z = 0; z <= density.z; z++)
                    {
                        var probePos = offset + new Vector3(x * moveX, y * moveY, z * moveZ);

                        probePos = volumeTransform.TransformPoint(probePos);

                        posList.Add(probePos);
                    }
                }
            }

            return posList;
        }

        public static List<Vector3> CreatePositionsFloat(Transform volumeTransform, Vector3 origin, Vector3 size, Vector3 density)
        {
            List<Vector3> posList = new List<Vector3>();
            var offset = origin;
            
            var stepX = Mathf.FloorToInt(size.x / density.x);
            var stepY = Mathf.FloorToInt(size.y / density.y);
            var stepZ = Mathf.FloorToInt(size.z / density.z);

            offset -= size * 0.5f;

            offset.x += (size.x - stepX * density.x) * 0.5f;
            offset.y += (size.y - stepY * density.y) * 0.5f;
            offset.z += (size.z - stepZ * density.z) * 0.5f;

            for (int x = 0; x <= stepX; x++)
            {
                for (int y = 0; y <= stepY; y++)
                {
                    for (int z = 0; z <= stepZ; z++)
                    {
                        var probePos = offset + new Vector3(x * density.x, y * density.y, z * density.z);

                        probePos = volumeTransform.TransformPoint(probePos);

                        posList.Add(probePos);
                    }
                }
            }

            return posList;
        }
    }
}
