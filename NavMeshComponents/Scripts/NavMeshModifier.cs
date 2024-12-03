using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Extensions;

namespace NavMeshPlus.Components
{
    [ExecuteInEditMode]
    [AddComponentMenu("Navigation/Navigation Modifier", 32)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshModifier : MonoBehaviour
    {
        [SerializeField]
        bool m_OverrideArea;
        public bool overrideArea { get => m_OverrideArea; set { m_OverrideArea = value; } }

        [SerializeField, NavMeshArea]
        int m_Area;
        public int area { get => m_Area; set { m_Area = value; } }

        [SerializeField]
        bool m_IgnoreFromBuild;
        public bool ignoreFromBuild { get => m_IgnoreFromBuild; set { m_IgnoreFromBuild = value; } }

        // List of agent types the modifier is applied for.
        // Special values: empty == None, m_AffectedAgents[0] =-1 == All.
        [SerializeField]
        List<int> m_AffectedAgents = new(new int[] { -1 });    // Default value is All

        static readonly List<NavMeshModifier> s_NavMeshModifiers = new();

        public static List<NavMeshModifier> activeModifiers => s_NavMeshModifiers;

        void OnEnable()
        {
            if (!s_NavMeshModifiers.Contains(this))
                s_NavMeshModifiers.Add(this);
        }

        void OnDisable()
        {
            s_NavMeshModifiers.Remove(this);
        }

        public bool AffectsAgentType(int agentTypeID)
        {
            if (m_AffectedAgents.Count == 0)
                return false;
            if (m_AffectedAgents[0] == -1)
                return true;
            return m_AffectedAgents.IndexOf(agentTypeID) != -1;
        }
    }
}
