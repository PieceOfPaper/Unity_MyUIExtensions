using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class Blur : MonoBehaviour
    {
        [Range(0f, 0.01f)] public float blurSize = 0.005f;
        [Range(1, 10)] public int blurSampling = 4;

        private Graphic m_Graphic;


        private void OnEnable()
        {
            m_Graphic = GetComponent<Graphic>();
            m_Graphic.material = new Material(Shader.Find("UI/Blur"));
        }

        private void OnDisable()
        {
            if (m_Graphic != null) m_Graphic.material = null;
        }

        private void OnValidate()
        {
            if (enabled == false)
                return;
            
            var graphic = GetComponent<Graphic>();
            if (graphic == null)
                return;

            if (graphic.material == null)
                graphic.material = new Material(Shader.Find("UI/Blur"));
            
            graphic.material.SetFloat("_BlurSize", blurSize);
            graphic.material.SetFloat("_BlurSampling", blurSampling);
        }
    }
}