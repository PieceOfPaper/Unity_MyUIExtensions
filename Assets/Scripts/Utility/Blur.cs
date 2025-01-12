using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class Blur : MonoBehaviour
    {
        [Range(0f, 0.01f)] [SerializeField] private float m_BlurSize = 0.005f;
        [Range(1, 10)] [SerializeField] private int m_BlurSampling = 4;
        [Range(0f, 1f)] [SerializeField] private float m_ColorFill = 0.0f;

        public float blurSize
        {
            get => m_BlurSize;
            set
            {
                if (m_BlurSize == value)
                    return;

                m_BlurSize = value;
                UpdateMaterial();
            }
        }

        public int blurSampling
        {
            get => m_BlurSampling;
            set
            {
                if (m_BlurSampling == value)
                    return;

                m_BlurSampling = value;
                UpdateMaterial();
            }
        }

        public float colorFill
        {
            get => m_ColorFill;
            set
            {
                if (m_ColorFill == value)
                    return;

                m_ColorFill = value;
                UpdateMaterial();
            }
        }

        private Graphic m_Graphic;


        private void OnEnable()
        {
            m_Graphic = GetComponent<Graphic>();
            m_Graphic.material = new Material(Shader.Find("UI/Blur"));
            UpdateMaterial();
        }

        private void OnDisable()
        {
            if (m_Graphic != null) m_Graphic.material = null;
        }

        private void OnValidate()
        {
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            if (enabled == false)
                return;
            
            var graphic = GetComponent<Graphic>();
            if (graphic == null)
                return;

            if (graphic.material == null)
                graphic.material = new Material(Shader.Find("UI/Blur"));
            
            graphic.material.SetFloat("_BlurSize", m_BlurSize);
            graphic.material.SetFloat("_BlurSampling", m_BlurSampling);
            graphic.material.SetFloat("_ColorFill", m_ColorFill);
        }
    }
}