using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [ExecuteAlways]
    public class GraphicVertexGizmos : BaseMeshEffect
    {
        public Color gizmosColor = Color.green;
        public float sphereRadius = 2.0f;

        private readonly List<UIVertex> m_Vertices = new List<UIVertex>();

        public override void ModifyMesh(VertexHelper vh)
        {
            m_Vertices.Clear();
            vh.GetUIVertexStream(m_Vertices);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!IsActive())
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = gizmosColor;
            for (var verIdx = 0; verIdx < m_Vertices.Count; verIdx ++)
            {
                var v = m_Vertices[verIdx].position;
                Gizmos.DrawSphere(v, sphereRadius);
            }
            var triCnt = m_Vertices.Count / 3;
            for (var triIdx = 0; triIdx < triCnt; triIdx ++)
            {
                var v0 = m_Vertices[triIdx * 3 + 0].position;
                var v1 = m_Vertices[triIdx * 3 + 1].position;
                var v2 = m_Vertices[triIdx * 3 + 2].position;
                Gizmos.DrawLine(v0, v1);
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v2, v0);
            }
        }
#endif
    }
}