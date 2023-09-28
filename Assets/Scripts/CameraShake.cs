using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    //Este script se utiliza junto a CineMachine, necesitas darle en Noise a la opcion de Basic Multi Channel Perlin y luego darle 6D Shake, la amplitud y la frecuencia sirven para la fuerza de sacudida y la frecuencia de sacudida respectivamente
    public static CameraShake instance {  get; private set; }

    private CinemachineVirtualCamera mCam;
    private float ShakeTimer;
    private void Awake()
    {
        instance = this;
        mCam = GetComponent<CinemachineVirtualCamera>();
    }

    public void ShakeCamera(float intesity, float time)
    {
        CinemachineBasicMultiChannelPerlin cbmcp = mCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cbmcp.m_AmplitudeGain = intesity;

        ShakeTimer = time;
    }

    private void Update()
    {
        if(ShakeTimer > 0f)
        {
            ShakeTimer -= Time.deltaTime;
            if(ShakeTimer <= 0f) 
            {
                CinemachineBasicMultiChannelPerlin cbmcp = mCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cbmcp.m_AmplitudeGain = 0f;
            }
        }
    }

}
