using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Shape", 11)]
    public class Shape : Graphic
    {
        public enum ShapeType
        {
            Square,
            Circle,
        }
        
        [SerializeField] private ShapeType m_ShapeType = ShapeType.Square;
        [SerializeField] private float m_Roundness = 0.0f;
        [SerializeField] private int m_RoundVertexCount = 12;
        
        public ShapeType Type
        {
            get => m_ShapeType;
            set
            {
                if (m_ShapeType != value)
                {
                    m_ShapeType = value;
                    SetVerticesDirty();
                }
            }
        }
        
        public float Roundness
        {
            get => m_Roundness;
            set
            {
                if (m_Roundness != value)
                {
                    m_Roundness = value;
                    SetVerticesDirty();
                }
            }
        }

        public int RoundVertexCount
        {
            get => m_RoundVertexCount;
            set
            {
                if (m_RoundVertexCount != value)
                {
                    m_RoundVertexCount = value;
                    SetVerticesDirty();
                }
            }
        }


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            
            vh.Clear();
            switch (m_ShapeType)
            {
                case ShapeType.Square:
                    {
                        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
                        if (m_Roundness <= 0f)
                        {
                            vh.AddVert(new Vector3(v.x, v.y), color, new Vector2(0f, 0f));
                            vh.AddVert(new Vector3(v.x, v.w), color, new Vector2(0f, 1f));
                            vh.AddVert(new Vector3(v.z, v.w), color, new Vector2(1f, 1f));
                            vh.AddVert(new Vector3(v.z, v.y), color, new Vector2(1f, 0f));
                            vh.AddTriangle(0, 1, 2);
                            vh.AddTriangle(2, 3, 0);
                        }
                        else
                        {
                            var vertexPerCorner = Mathf.Max(Mathf.CeilToInt(m_RoundVertexCount / 4f), 3);
                            var roundness = Mathf.Min(m_Roundness, r.width * 0.5f, r.height * 0.5f);

                            for (var cornerIdx = 0; cornerIdx < 4; cornerIdx ++)
                            {
                                var center = Vector2.zero;
                                var x = 0f;
                                var y = 0f;
                                switch (cornerIdx)
                                {
                                    case 0:
                                        center = new Vector2(v.x, v.y) + new Vector2(1f, 1f) * roundness;
                                        x = -roundness;
                                        y = 0f;
                                        break;
                                    case 1:
                                        center = new Vector2(v.z, v.y) + new Vector2(-1f, 1f) * roundness;
                                        x = 0f;
                                        y = -roundness;
                                        break;
                                    case 2:
                                        center = new Vector2(v.z, v.w) + new Vector2(-1f, -1f) * roundness;
                                        x = roundness;
                                        y = 0f;
                                        break;
                                    case 3:
                                        center = new Vector2(v.x, v.w) + new Vector2(1f, -1f) * roundness;
                                        x = 0f;
                                        y = roundness;
                                        break;
                                }
                                float angle = Mathf.PI * 0.5f / vertexPerCorner;
                                float cos = Mathf.Cos(angle);
                                float sin = Mathf.Sin(angle);
                                
                                vh.AddVert(center, color, Vector2.zero);
                                vh.AddVert(center + new Vector2(x, y), color, Vector2.zero);
                                for (int i = 0; i < vertexPerCorner + 1; i++)
                                {
                                    float x1 = x;
                                    x = cos * x - sin * y;
                                    y = sin * x1 + cos * y;
                                    vh.AddVert(center + new Vector2(x, y), color, Vector2.zero);
                                }
                                for (int i = 0; i < vertexPerCorner + 1; i++)
                                {
                                    vh.AddTriangle(
                                        (vertexPerCorner + 3) * cornerIdx, 
                                        (vertexPerCorner + 3) * cornerIdx + i , 
                                        (vertexPerCorner + 3) * cornerIdx + i + 1);
                                }
                            }

                            for (var cornerIdx = 0; cornerIdx < 4; cornerIdx ++)
                            {
                                var nextCornetIdx = cornerIdx + 1 >= 4 ? 0 : cornerIdx + 1;
                                vh.AddTriangle(
                                    (vertexPerCorner + 3) * cornerIdx + 0,
                                    (vertexPerCorner + 3) * cornerIdx + vertexPerCorner + 1,
                                    (vertexPerCorner + 3) * nextCornetIdx);
                                vh.AddTriangle(
                                    (vertexPerCorner + 3) * nextCornetIdx,
                                    (vertexPerCorner + 3) * nextCornetIdx + 1,
                                    (vertexPerCorner + 3) * cornerIdx + vertexPerCorner + 1);
                            }
                            
                            vh.AddTriangle(
                                (vertexPerCorner + 3) * 0, 
                                (vertexPerCorner + 3) * 1, 
                                (vertexPerCorner + 3) * 2);
                            vh.AddTriangle(
                                (vertexPerCorner + 3) * 2, 
                                (vertexPerCorner + 3) * 3, 
                                (vertexPerCorner + 3) * 0);
                        }
                    }
                    break;
                case ShapeType.Circle:
                    {
                        var radius = Mathf.Max(r.width, r.height) * 0.5f;
                        var widthRate = r.width * 0.5f / radius;
                        var heightRate = r.height * 0.5f / radius;
                        
                        int segments = Mathf.Max(m_RoundVertexCount, 8);
                        float angle = 2.0f * Mathf.PI / segments;
                        float cos = Mathf.Cos(angle);
                        float sin = Mathf.Sin(angle);
                        float x = radius;
                        float y = 0.0f;
                        
                        vh.AddVert(r.center, color, Vector2.zero);
                        vh.AddVert(r.center + new Vector2(x * widthRate, y * heightRate), color, Vector2.zero);
                        for (int i = 0; i < segments + 1; i++)
                        {
                            float x1 = x;
                            x = cos * x - sin * y;
                            y = sin * x1 + cos * y;
                            vh.AddVert(r.center + new Vector2(x * widthRate, y * heightRate), color, Vector2.zero);
                        }
                        for (int i = 0; i < segments + 2; i++)
                            vh.AddTriangle(0, i , (i + 1) >= (segments + 1) ? 1 : i + 1);
                    }
                    break;
            }
        }
    }
}