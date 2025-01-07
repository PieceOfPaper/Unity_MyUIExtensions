using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TMPro
{
    [RequireComponent(typeof(TMP_Text))]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class TextSimpleMaterialGenerator : MonoBehaviour
    {
        [Header("Material")]
        [SerializeField] private Material m_OriginMaterial;
        
        [Header("Outline")]
        [SerializeField] private bool m_Outline = false;
        public bool outline { get => m_Outline; set { if (m_Outline != value) { m_Outline = value; UpdateMaterial(); } } }
        [ColorUsage(true, true)] [SerializeField] private Color m_OutlineColor = new Color(0f, 0f, 0f, 1f);
        public Color outlineColor { get => m_OutlineColor; set { if (m_OutlineColor != value) { m_OutlineColor = value; UpdateMaterial(); } } }
        [Range(0, 100)] [SerializeField] private byte m_OutlineWidth = 0;
        public byte outlineWidth { get => m_OutlineWidth; set { if (m_OutlineWidth != value) { m_OutlineWidth = value; UpdateMaterial(); } } }
        [Range(0, 100)] [SerializeField] private byte m_OutlineSoftness = 0;
        public byte outlineSoftness { get => m_OutlineSoftness; set { if (m_OutlineSoftness != value) { m_OutlineSoftness = value; UpdateMaterial(); } } }
        
        [Header("Underlay")]
        [SerializeField] private bool m_Underlay = false;
        public bool underlay { get => m_Underlay; set { if (m_Underlay != value) { m_Underlay = value; UpdateMaterial(); } } }
        [ColorUsage(true, true)] [SerializeField] private Color m_UnderlayColor = new Color(0f, 0f, 0f, 1f);
        public Color underlayColor { get => m_UnderlayColor; set { if (m_UnderlayColor != value) { m_UnderlayColor = value; UpdateMaterial(); } } }
        [Range(-100, 100)] [SerializeField] private sbyte m_UnderlayOffsetX = 0;
        public sbyte underlayOffsetX { get => m_UnderlayOffsetX; set { if (m_UnderlayOffsetX != value) { m_UnderlayOffsetX = value; UpdateMaterial(); } } }
        [Range(-100, 100)] [SerializeField] private sbyte m_UnderlayOffsetY = 0;
        public sbyte underlayOffsetY { get => m_UnderlayOffsetY; set { if (m_UnderlayOffsetY != value) { m_UnderlayOffsetY = value; UpdateMaterial(); } } }
        [Range(-100, 100)] [SerializeField] private sbyte m_UnderlayDilate = 0;
        public sbyte underlayDilate { get => m_UnderlayDilate; set { if (m_UnderlayDilate != value) { m_UnderlayDilate = value; UpdateMaterial(); } } }
        [Range(0, 100)] [SerializeField] private byte m_UnderlaySoftness = 0;
        public byte underlaySoftness { get => m_UnderlaySoftness; set { if (m_UnderlaySoftness != value) { m_UnderlaySoftness = value; UpdateMaterial(); } } }

        
        private TMP_Text m_Text;
        public TMP_Text text
        {
            get
            {
                if (Application.isPlaying == false)
                    return GetComponent<TMP_Text>();
                
                if (m_Text == null)
                    m_Text = GetComponent<TMP_Text>();
                return m_Text;
            }
        }


        
        private void OnEnable()
        {
            UpdateMaterial();
        }

        private void OnDisable()
        {
            ApplyMaterial(this, m_OriginMaterial, m_GeneratedMaterialData, default);
            m_GeneratedMaterialData = default;
        }

        private void OnValidate()
        {
            UpdateMaterial();
        }
        
        private void Reset()
        {
            if (m_OriginMaterial == null)
                m_OriginMaterial = text.fontSharedMaterial;
        }


        private MaterialData m_GeneratedMaterialData;
        private Material m_GeneratedMaterial = null;
        
        private void UpdateMaterial()
        {
            if (m_OriginMaterial == null)
                return;
            
            var materialData = new MaterialData()
            {
                isValid = true,
                
                outline = outline,
                outlineWidth = outline ? outlineWidth : (byte)0,
                outlineSoftness = outline ? outlineSoftness : (byte)0,
                
                underlay = underlay,
                underlayOffsetX = underlay ? underlayOffsetX : (sbyte)0,
                underlayOffsetY = underlay ? underlayOffsetY : (sbyte)0,
                underlayDilate = underlay ? underlayDilate : (sbyte)0,
                underlaySoftness = outline ? underlaySoftness : (byte)0,
            };

            if (m_GeneratedMaterialData.Equals(materialData) == true)
                return;

            if (Application.isPlaying == true)
            {
                ApplyMaterial(this, m_OriginMaterial, m_GeneratedMaterialData, materialData);
            }
            else
            {
                if (m_GeneratedMaterial != null)
                    DestroyImmediate(m_GeneratedMaterial);
                m_GeneratedMaterial = GenerateMaterial(m_OriginMaterial, materialData);
                text.fontMaterial = m_GeneratedMaterial;
            }
            
            m_GeneratedMaterialData = materialData;
        }

        public struct MaterialData : IEquatable<MaterialData>
        {
            public bool isValid;
            
            public bool outline;
            public byte outlineWidth;
            public byte outlineSoftness;
            
            public bool underlay;
            public sbyte underlayOffsetX;
            public sbyte underlayOffsetY;
            public sbyte underlayDilate;
            public byte underlaySoftness;

            public bool Equals(MaterialData other)
            {
                if (isValid != other.isValid) return false;
                if (outline != other.outline) return false;
                if (outlineWidth != other.outlineWidth) return false;
                if (outlineSoftness != other.outlineSoftness) return false;
                if (underlay != other.underlay) return false;
                if (underlayOffsetX != other.underlayOffsetX) return false;
                if (underlayOffsetY != other.underlayOffsetY) return false;
                if (underlayDilate != other.underlayDilate) return false;
                if (underlaySoftness != other.underlaySoftness) return false;
                return true;
            }
        }

        public class CachedMaterialData
        {
            public Material material;
            public List<TextSimpleMaterialGenerator> usageList = new List<TextSimpleMaterialGenerator>();
        }

        private static Dictionary<Material, Dictionary<MaterialData, CachedMaterialData>> s_Materials = new Dictionary<Material, Dictionary<MaterialData, CachedMaterialData>>();

        private static Material GenerateMaterial(Material originMaterial, MaterialData data)
        {
            var mat = new Material(originMaterial);
            if (data.outline) mat.EnableKeyword("OUTLINE_ON");
            else mat.DisableKeyword("OUTLINE_ON");
            if (data.outline == true)
            {
                mat.SetColor("_OutlineColor", Color.black);
                mat.SetFloat("_OutlineWidth", data.outlineWidth * 0.01f);
                mat.SetFloat("_OutlineSoftness", data.outlineSoftness * 0.01f);
            }
            if (data.underlay) mat.EnableKeyword("UNDERLAY_ON");
            else mat.DisableKeyword("UNDERLAY_ON");
            if (data.underlay == true)
            {
                mat.SetColor("_UnderlayColor", Color.black);
                mat.SetFloat("_UnderlayOffsetX", data.underlayOffsetX * 0.01f);
                mat.SetFloat("_UnderlayOffsetY", data.underlayOffsetY * 0.01f);
                mat.SetFloat("_UnderlayDilate", data.underlayDilate * 0.01f);
                mat.SetFloat("_UnderlaySoftness", data.underlaySoftness * 0.01f);
            }
            return mat;
        }
        
        private static CachedMaterialData GetCachedMaterialData(Material originMaterial, MaterialData data)
        {
            if (originMaterial == null)
                return null;
            
            if (s_Materials.ContainsKey(originMaterial) == false)
                s_Materials.Add(originMaterial, new Dictionary<MaterialData, CachedMaterialData>());
            
            if (s_Materials[originMaterial].ContainsKey(data) == false)
            {
                var cachedData = new CachedMaterialData();
                cachedData.material = GenerateMaterial(originMaterial, data);
                s_Materials[originMaterial].Add(data, cachedData);
            }
            return s_Materials[originMaterial][data];
        }

        private static void ApplyMaterial(TextSimpleMaterialGenerator generator, Material originMaterial, MaterialData prevData, MaterialData nextData)
        {
            if (prevData.isValid == true)
            {
                var prevCachedData = GetCachedMaterialData(originMaterial, prevData);
                prevCachedData.usageList.Remove(generator);
            }
            
            if (nextData.isValid == true)
            {
                var nextCachedData = GetCachedMaterialData(originMaterial, nextData);
                nextCachedData.usageList.Add(generator);
                generator.text.fontMaterial = nextCachedData.material;
            }
            else
            {
                generator.text.fontMaterial = originMaterial;
            }
        }
    }
}
