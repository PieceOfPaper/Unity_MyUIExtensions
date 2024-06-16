using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    public class SimpleGradientColor : BaseMeshEffect
    {
        public Color startColor = Color.white;
        public Color endColor = Color.white;
        public Direction dir;
        
        private RectTransform m_MyRectTransform;
        public RectTransform MyRectTransform
        {
            get
            {
                if (Application.isPlaying == false)
                    return GetComponent<RectTransform>();
                
                if (m_MyRectTransform == null)
                    m_MyRectTransform = GetComponent<RectTransform>();
                return m_MyRectTransform;
            }
        }
        
        public enum Direction
        {
            Horizontal,
            Vertical,
        }
        
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0 || graphic == null)
                return;
            
            var output = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(output);

            var rect = MyRectTransform.rect;
            for (var i = 0; i < output.Count; i ++)
            {
                var vertex = output[i];
                vertex.color = ColorScalar(Color.Lerp(startColor, endColor, dir == Direction.Horizontal ? (vertex.position.x - rect.x) / rect.width : (vertex.position.y - rect.y) / rect.height), vertex.color);
                output[i] = vertex;
            }
            
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
            ListPool<UIVertex>.Release(output);
        }
        
        private Color ColorScalar(Color color1, Color color2)
        {
            return new Color(color1.r * color2.r, color1.g * color2.g, color1.b * color2.b, color1.a * color2.a);
        }
    }
}