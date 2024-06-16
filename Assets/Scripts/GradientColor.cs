using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityEngine.UI
{
    public class GradientColor : BaseMeshEffect
    {
        public Gradient gradient;
        public Direction dir;
        
        public enum Direction
        {
            Horizontal,
            Vertical,
        }
        
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0 || graphic == null)
                return;
            
            var input = ListPool<UIVertex>.Get();
            var output = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(input);
            
            var posXList = ListPool<float>.Get();
            var posYList = ListPool<float>.Get();
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            for (var i = 0; i < input.Count; i ++)
            {
                var vertex = input[i];
                var pos = vertex.position;
                if (posXList.Contains(pos.x) == false) posXList.Add(pos.x);
                if (posYList.Contains(pos.y) == false) posYList.Add(pos.y);
                
                if (pos.x < min.x) min.x = pos.x;
                if (pos.x > max.x) max.x = pos.x;
                if (pos.y < min.y) min.y = pos.y;
                if (pos.y > max.y) max.y = pos.y;
            }
            
            switch (dir)
            {
                case Direction.Horizontal:
                    {
                        for (var i = 0; i < gradient.alphaKeys.Length; i ++)
                        {
                            var alphaKey = gradient.alphaKeys[i];
                            var index = posXList.FindIndex(x => Mathf.Abs((x - min.x) / (max.x - min.x) - alphaKey.time) < 0.0001f);
                            if (index < 0)
                                posXList.Add(min.x + (max.x - min.x) * alphaKey.time);
                        }
                        for (var i = 0; i < gradient.colorKeys.Length; i ++)
                        {
                            var colorKey = gradient.colorKeys[i];
                            var index = posXList.FindIndex(x => Mathf.Abs((x - min.x) / (max.x - min.x) - colorKey.time) < 0.0001f);
                            if (index < 0)
                                posXList.Add(min.x + (max.x - min.x) * colorKey.time);
                        }
                        posXList.Sort();
                    }
                    break;
                case Direction.Vertical:
                    {
                        for (var i = 0; i < gradient.alphaKeys.Length; i ++)
                        {
                            var alphaKey = gradient.alphaKeys[i];
                            var index = posYList.FindIndex(y => Mathf.Abs((y - min.y) / (max.y - min.y) - alphaKey.time) < 0.0001f);
                            if (index < 0)
                                posYList.Add(min.y + (max.y - min.y) * alphaKey.time);
                        }
                        for (var i = 0; i < gradient.colorKeys.Length; i ++)
                        {
                            var colorKey = gradient.colorKeys[i];
                            var index = posYList.FindIndex(y => Mathf.Abs((y - min.y) / (max.y - min.x) - colorKey.time) < 0.0001f);
                            if (index < 0)
                                posYList.Add(min.y + (max.y - min.y) * colorKey.time);
                        }
                        posYList.Sort();
                    }
                    break;
            }
            for (var y = 1; y < posYList.Count; y ++)
            {
                for (var x = 1; x < posXList.Count; x ++)
                {
                    var v0 = new Vector3(posXList[x - 1], posYList[y - 1], 0f);
                    var v1 = new Vector3(posXList[x], posYList[y - 1], 0f);
                    var v2 = new Vector3(posXList[x - 1], posYList[y], 0f);
                    var v3 = new Vector3(posXList[x], posYList[y], 0f);

                    var color0 = Color.white;
                    var color1 = Color.white;
                    var color2 = Color.white;
                    var color3 = Color.white;
                    switch (dir)
                    {
                        case Direction.Horizontal:
                            color0 = ColorScalar(graphic.color, gradient.Evaluate((posXList[x - 1] - min.x) / (max.x - min.x)));
                            color1 = ColorScalar(graphic.color, gradient.Evaluate((posXList[x] - min.x) / (max.x - min.x)));
                            color2 = ColorScalar(graphic.color, gradient.Evaluate((posXList[x - 1] - min.x) / (max.x - min.x)));
                            color3 = ColorScalar(graphic.color, gradient.Evaluate((posXList[x] - min.x) / (max.x - min.x)));
                            break;
                        case Direction.Vertical:
                            color0 = ColorScalar(graphic.color, gradient.Evaluate((posYList[y - 1] - min.y) / (max.y - min.y)));
                            color1 = ColorScalar(graphic.color, gradient.Evaluate((posYList[y - 1] - min.y) / (max.y - min.y)));
                            color2 = ColorScalar(graphic.color, gradient.Evaluate((posYList[y] - min.y) / (max.y - min.y)));
                            color3 = ColorScalar(graphic.color, gradient.Evaluate((posYList[y] - min.y) / (max.y - min.y)));
                            break;
                    }
                    
                    output.Add(new UIVertex() { position = v0, color = color0, uv0 = new Vector4((v0.x - min.x) / (max.x - min.x), (v0.y - min.y) / (max.y - min.y), 0f, 0f) });
                    output.Add(new UIVertex() { position = v1, color = color1, uv0 = new Vector4((v1.x - min.x) / (max.x - min.x), (v1.y - min.y) / (max.y - min.y), 0f, 0f) });
                    output.Add(new UIVertex() { position = v2, color = color2, uv0 = new Vector4((v2.x - min.x) / (max.x - min.x), (v2.y - min.y) / (max.y - min.y), 0f, 0f) });
                    output.Add(new UIVertex() { position = v1, color = color1, uv0 = new Vector4((v1.x - min.x) / (max.x - min.x), (v1.y - min.y) / (max.y - min.y), 0f, 0f) });
                    output.Add(new UIVertex() { position = v2, color = color2, uv0 = new Vector4((v2.x - min.x) / (max.x - min.x), (v2.y - min.y) / (max.y - min.y), 0f, 0f) });
                    output.Add(new UIVertex() { position = v3, color = color3, uv0 = new Vector4((v3.x - min.x) / (max.x - min.x), (v3.y - min.y) / (max.y - min.y), 0f, 0f) });
                }
            }
            
            vh.Clear();
            vh.AddUIVertexTriangleStream(output);
            ListPool<UIVertex>.Release(input);
            ListPool<UIVertex>.Release(output);
            ListPool<float>.Release(posXList);
            ListPool<float>.Release(posYList);
        }
        
        private Color ColorScalar(Color color1, Color color2)
        {
            return new Color(color1.r * color2.r, color1.g * color2.g, color1.b * color2.b, color1.a * color2.a);
        }
    }

}