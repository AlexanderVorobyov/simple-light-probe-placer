using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleLightProbePlacer
{
    [AddComponentMenu("")]
    public class TransformVolume : MonoBehaviour
    {
        [SerializeField] private Volume m_volume = new Volume(Vector3.zero, Vector3.one);

        public Volume Volume { get { return m_volume; } set { m_volume = value; } }
        public Vector3 Origin { get { return m_volume.Origin; } }
        public Vector3 Size { get { return m_volume.Size; } }

        public bool IsInBounds(Vector3[] points)
        {
            return GetBounds().Intersects(GetBounds(points));
        }

        public bool IsOnBorder(Vector3[] points)
        {
            if (points.All(x => !IsInVolume(x))) return false;

            return !points.All(IsInVolume);
        }

        public bool IsInVolume(Vector3[] points)
        {
            return points.All(IsInVolume);
        }

        public bool IsInVolume(Vector3 position)
        {
            for (int i = 0; i < 6; i++)
            {
                var plane = new Plane(GetSideDirection(i), GetSidePosition(i));

                if (plane.GetSide(position)) return false;
            }

            return true;
        }

        public Vector3[] GetCorners()
        {
            Vector3[] corners =
            {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i].x *= m_volume.Size.x;
                corners[i].y *= m_volume.Size.y;
                corners[i].z *= m_volume.Size.z;

                corners[i] = transform.TransformPoint(m_volume.Origin + corners[i]);
            }

            return corners;
        }

        public Bounds GetBounds()
        {
            return GetBounds(GetCorners());
        }

        public Bounds GetBounds(Vector3[] points)
        {
            var center = points.Aggregate(Vector3.zero, (result, point) => result + point) / points.Length;
            var bounds = new Bounds(center, Vector3.zero);

            for (int i = 0; i < points.Length; i++)
            {
                bounds.Encapsulate(points[i]);
            }

            return bounds;
        }

        public GameObject[] GetGameObjectsInBounds(LayerMask layerMask)
        {
            MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();

            List<GameObject> list = new List<GameObject>();

            Bounds bounds = GetBounds();

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i].gameObject == transform.gameObject) continue;
                if (meshRenderers[i].GetComponent<TransformVolume>() != null) continue;
                if ((1 << meshRenderers[i].gameObject.layer & layerMask.value) == 0) continue;

                if (bounds.Intersects(meshRenderers[i].bounds))
                {
                    list.Add(meshRenderers[i].gameObject);
                }
            }

            return list.ToArray();
        }

        public Vector3 GetSideDirection(int side)
        {
            Vector3[] sides = new Vector3[6];

            var right = Vector3.right;
            var up = Vector3.up;
            var forward = Vector3.forward;

            sides[0] = right;
            sides[1] = -right;
            sides[2] = up;
            sides[3] = -up;
            sides[4] = forward;
            sides[5] = -forward;

            return transform.TransformDirection(sides[side]);
        }

        public Vector3 GetSidePosition(int side)
        {
            Vector3[] sides = new Vector3[6];

            var right = Vector3.right;
            var up = Vector3.up;
            var forward = Vector3.forward;

            sides[0] = right;
            sides[1] = -right;
            sides[2] = up;
            sides[3] = -up;
            sides[4] = forward;
            sides[5] = -forward;

            return transform.TransformPoint(sides[side] * GetSizeAxis(side) + m_volume.Origin);
        }

        public float GetSizeAxis(int side)
        {
            switch (side)
            {
                case 0:
                case 1: return m_volume.Size.x * 0.5f;
                case 2:
                case 3: return m_volume.Size.y * 0.5f;
                default: return m_volume.Size.z * 0.5f;
            }
        }

#if UNITY_EDITOR
        public static Volume EditorVolumeControl(TransformVolume transformVolume, float handleSize, Color color)
        {
            Vector3 origin, size;
            Vector3[] controlHandles = new Vector3[6];
            var transform = transformVolume.transform;

            Handles.color = color;

            for (int i = 0; i < controlHandles.Length; i++)
            {
                controlHandles[i] = transformVolume.GetSidePosition(i);
            }

            controlHandles[0] = Handles.Slider(controlHandles[0], transform.right, handleSize, Handles.DotHandleCap, 1);
            controlHandles[1] = Handles.Slider(controlHandles[1], transform.right, handleSize, Handles.DotHandleCap, 1);
            controlHandles[2] = Handles.Slider(controlHandles[2], transform.up, handleSize, Handles.DotHandleCap, 1);
            controlHandles[3] = Handles.Slider(controlHandles[3], transform.up, handleSize, Handles.DotHandleCap, 1);
            controlHandles[4] = Handles.Slider(controlHandles[4], transform.forward, handleSize, Handles.DotHandleCap, 1);
            controlHandles[5] = Handles.Slider(controlHandles[5], transform.forward, handleSize, Handles.DotHandleCap, 1);

            origin.x = transform.InverseTransformPoint((controlHandles[0] + controlHandles[1]) * 0.5f).x;
            origin.y = transform.InverseTransformPoint((controlHandles[2] + controlHandles[3]) * 0.5f).y;
            origin.z = transform.InverseTransformPoint((controlHandles[4] + controlHandles[5]) * 0.5f).z;
            
            size.x = transform.InverseTransformPoint(controlHandles[0]).x - transform.InverseTransformPoint(controlHandles[1]).x;
            size.y = transform.InverseTransformPoint(controlHandles[2]).y - transform.InverseTransformPoint(controlHandles[3]).y;
            size.z = transform.InverseTransformPoint(controlHandles[4]).z - transform.InverseTransformPoint(controlHandles[5]).z;

            return new Volume(origin, size);
        }
#endif
    }
}
