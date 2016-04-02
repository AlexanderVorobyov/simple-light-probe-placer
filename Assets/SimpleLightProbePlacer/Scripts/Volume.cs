using UnityEngine;

namespace SimpleLightProbePlacer
{
    [System.Serializable]
    public struct Volume
    {
        [SerializeField] private Vector3 m_origin;
        [SerializeField] private Vector3 m_size;

        public Vector3 Origin { get { return m_origin; } }
        public Vector3 Size { get { return m_size; } }
        
        public Volume(Vector3 origin, Vector3 size)
        {
            m_origin = origin;
            m_size = size;
        }

        public static bool operator ==(Volume left, Volume right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Volume left, Volume right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Volume other)
        {
            return Origin == other.Origin && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Volume && Equals((Volume) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Origin.GetHashCode() * 397) ^ Size.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("Origin: {0}, Size: {1}", Origin, Size);
        }
    }
}
