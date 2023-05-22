using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake m_Instance;
    public static CameraShake Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = FindObjectOfType<CameraShake>();
            return m_Instance;
        }
    }
    [SerializeField] private CinemachineVirtualCamera[] m_VitualCamera;
    [SerializeField] private float m_ShakeAmplitude = 1.2f;
    [SerializeField] private float m_ShakeFrequency = 2.0f;

    private float m_ShakeDuration;
    private CinemachineBasicMultiChannelPerlin[] m_VitualCameraNoise;

    private void Awake()
    {
        if (m_Instance == null)
            m_Instance = this;
        else if (m_Instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        m_VitualCameraNoise = new CinemachineBasicMultiChannelPerlin[m_VitualCamera.Length];
        for (int i = 0; i < m_VitualCamera.Length; i++)
            m_VitualCameraNoise[i] = m_VitualCamera[i].GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
    public void Shake(float pDuration)
    {
        m_ShakeDuration = pDuration;
    }

    // Update is called once per frame
   private void Update()
   {
        if(m_ShakeDuration > 0)
        {
            m_ShakeDuration -= Time.deltaTime;
            for(int i = 0; i < m_VitualCameraNoise.Length; i++)
            {
                m_VitualCameraNoise[i].m_AmplitudeGain = m_ShakeAmplitude;
                m_VitualCameraNoise[i].m_FrequencyGain = m_ShakeFrequency;
            }
            if (m_ShakeDuration <= 0)
            {
                for(int i = 0; i < m_VitualCameraNoise.Length; i++)
                {
                    m_VitualCameraNoise[i].m_AmplitudeGain = 0;
                    m_VitualCameraNoise[i].m_FrequencyGain = 0;
                }
            }
        }
        
   }
}
