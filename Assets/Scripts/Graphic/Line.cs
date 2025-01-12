using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    [System.Obsolete("개발중")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class Line : MaskableGraphic
    {
        [SerializeField] private float m_Thickness = 2f;
        [SerializeField] private List<Vector2> m_Positions = new List<Vector2>(new []{ new Vector2(-10f, 0f), new Vector2(10f, 0f) });


        public int GetPositionCount() => m_Positions.Count;
        
        public Vector2 GetPosition(int index) => index < 0 || index >= m_Positions.Count ? default : m_Positions[index];
        
        public void AddPosition(Vector2 pos)
        {
            m_Positions.Add(pos);
            SetVerticesDirty();
        }
        
        public void RemovePosition(int index)
        {
            m_Positions.RemoveAt(index);
            SetVerticesDirty();
        }

        public void ClearPositions()
        {
            m_Positions.Clear();
            SetVerticesDirty();
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            
            vh.Clear();

            if (m_Positions.Count < 2)
                return;
            
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

            var lengths = ListPool<float>.Get();
            var lengthSum = 0f;
            for (var i = 0; i < m_Positions.Count - 1; i ++)
            {
                var length = Vector2.Distance(m_Positions[i + 1], m_Positions[i]);
                lengths.Add(length);
                lengthSum += length;
            }

            var currentLength = 0f;
            var vertexIndex = 0;
            for (var i = 0; i < m_Positions.Count - 1; i ++)
            {
                var pos1 = m_Positions[i];
                var pos2 = m_Positions[i + 1];
                var posDiff = pos2 - pos1;
                var radian = -Mathf.Atan2(posDiff.y, posDiff.x);
                var sin = Mathf.Sin(radian);
                var cos = Mathf.Cos(radian);
                var prevLength = currentLength;
                currentLength += lengths[i];
                
                vh.AddVert(new Vector3(pos1.x + sin * m_Thickness, pos1.y + cos * m_Thickness), color, new Vector2(prevLength / lengthSum, 0f));
                vh.AddVert(new Vector3(pos2.x + sin * m_Thickness, pos2.y + cos * m_Thickness), color, new Vector2(prevLength / lengthSum, 1f));
                vh.AddVert(new Vector3(pos2.x - sin * m_Thickness, pos2.y - cos * m_Thickness), color, new Vector2(currentLength / lengthSum, 1f));
                vh.AddVert(new Vector3(pos1.x - sin * m_Thickness, pos1.y - cos * m_Thickness), color, new Vector2(currentLength / lengthSum, 0f));
                vh.AddTriangle(vertexIndex + 0, vertexIndex + 1, vertexIndex + 2);
                vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 0);
                vertexIndex += 4;
            }

            ListPool<float>.Release(lengths);
        }
    }
}
