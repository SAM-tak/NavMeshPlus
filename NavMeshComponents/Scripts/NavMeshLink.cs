using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Extensions;

namespace NavMeshPlus.Components
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-101)]
    [AddComponentMenu("Navigation/Navigation Link", 33)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshPlus#documentation-draft")]
    public class NavMeshLink : MonoBehaviour
    {
        [SerializeField, NavMeshAgent]
        int m_AgentTypeID;
        public int agentTypeID { get => m_AgentTypeID; set { m_AgentTypeID = value; UpdateLink(); } }

        [SerializeField]
        Vector3 m_StartPoint = new(0.0f, 0.0f, -2.5f);
        public Vector3 startPoint { get => m_StartPoint; set { m_StartPoint = value; UpdateLink(); } }

        [SerializeField]
        Vector3 m_EndPoint = new(0.0f, 0.0f, 2.5f);
        public Vector3 endPoint { get => m_EndPoint; set { m_EndPoint = value; UpdateLink(); } }

        [SerializeField]
        float m_Width;
        public float width { get => m_Width; set { m_Width = value; UpdateLink(); } }

        [SerializeField]
        int m_CostModifier = -1;
        public int costModifier { get => m_CostModifier; set { m_CostModifier = value; UpdateLink(); } }

        [SerializeField]
        bool m_Bidirectional = true;
        public bool bidirectional { get => m_Bidirectional; set { m_Bidirectional = value; UpdateLink(); } }

        [SerializeField]
        bool m_AutoUpdatePosition;
        public bool autoUpdate { get => m_AutoUpdatePosition; set { SetAutoUpdate(value); } }

        [SerializeField, NavMeshArea]
        int m_Area;
        public int area { get => m_Area; set { m_Area = value; UpdateLink(); } }

        NavMeshLinkInstance m_LinkInstance = new();

        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;

        static readonly List<NavMeshLink> s_Tracked = new();

        void OnEnable()
        {
            AddLink();
            if (m_AutoUpdatePosition && NavMesh.IsLinkValid(m_LinkInstance))
                AddTracking(this);
        }

        void OnDisable()
        {
            RemoveTracking(this);
            NavMesh.RemoveLink(m_LinkInstance);
        }

        public void UpdateLink()
        {
            NavMesh.RemoveLink(m_LinkInstance);
            AddLink();
        }

        static void AddTracking(NavMeshLink link)
        {
#if UNITY_EDITOR
            if (s_Tracked.Contains(link))
            {
                Debug.LogError("Link is already tracked: " + link);
                return;
            }
#endif

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate += UpdateTrackedInstances;

            s_Tracked.Add(link);
        }

        static void RemoveTracking(NavMeshLink link)
        {
            s_Tracked.Remove(link);

            if (s_Tracked.Count == 0)
                NavMesh.onPreUpdate -= UpdateTrackedInstances;
        }

        void SetAutoUpdate(bool value)
        {
            if (m_AutoUpdatePosition == value)
                return;
            m_AutoUpdatePosition = value;
            if (value)
                AddTracking(this);
            else
                RemoveTracking(this);
        }

        void AddLink()
        {
#if UNITY_EDITOR
            if (NavMesh.IsLinkValid(m_LinkInstance))
            {
                Debug.LogError("Link is already added: " + this);
                return;
            }
#endif

            var link = new NavMeshLinkData
            {
                startPosition = m_StartPoint,
                endPosition = m_EndPoint,
                width = m_Width,
                costModifier = m_CostModifier,
                bidirectional = m_Bidirectional,
                area = m_Area,
                agentTypeID = m_AgentTypeID
            };
            m_LinkInstance = NavMesh.AddLink(link, transform.position, transform.rotation);
            if (NavMesh.IsLinkValid(m_LinkInstance))
                NavMesh.SetLinkOwner(m_LinkInstance, this);

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position) return true;
            if (m_LastRotation != transform.rotation) return true;
            return false;
        }

        void OnDidApplyAnimationProperties()
        {
            UpdateLink();
        }

        static void UpdateTrackedInstances()
        {
            foreach (var instance in s_Tracked)
            {
                if (instance.HasTransformChanged())
                    instance.UpdateLink();
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_Width = Mathf.Max(0.0f, m_Width);

            if (!NavMesh.IsLinkValid(m_LinkInstance))
                return;

            UpdateLink();

            if (!m_AutoUpdatePosition)
            {
                RemoveTracking(this);
            }
            else if (!s_Tracked.Contains(this))
            {
                AddTracking(this);
            }
        }
#endif
    }
}
