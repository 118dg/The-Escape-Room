using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TeleporterAnim : MonoBehaviour
{

   float m_MaxAlphaIntensity = 2f;
   float m_MinAlphaIntensity = 0f;
   float m_CurrentTime = 0f;

    [FormerlySerializedAs("fadeSpeed")]
    [SerializeField]
    float m_FadeSpeed = 2.2f;

    bool m_Highlighted = false;

    [FormerlySerializedAs("meshRenderer")]
    [SerializeField]
    MeshRenderer m_MeshRenderer;
    MaterialPropertyBlock m_Block;

    int m_AlphaIntensityID;
    
    void Start()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();

        m_AlphaIntensityID = Shader.PropertyToID("AlphaIntensity"); //혹시 Alpha값인가? 투명도?

        m_Block = new MaterialPropertyBlock(); //??? 이것이 관건
        m_Block.SetFloat(m_AlphaIntensityID, m_CurrentTime); //근데 currentTime의 최대값은 2f인데 알파값은 0~1 사이의 값 아니었나?

        m_CurrentTime = 0;

        m_MeshRenderer.SetPropertyBlock(m_Block); //alphaIntensity 값 다시 설정해주는 것
    }
    
    void Update()
    {
        if (m_Highlighted)
        {
            m_CurrentTime += Time.deltaTime * m_FadeSpeed; //highlighted인데 왜 시간을 증가시키지? //암튼 fadespeed의 속도로 텔레포트 자리 빛 켜는 것
        }
        else if (!m_Highlighted)
        {
            m_CurrentTime -= Time.deltaTime * m_FadeSpeed;
        }
        
        //current time : 0f ~ 2f 이내 (clipping)
        if (m_CurrentTime > m_MaxAlphaIntensity)
            m_CurrentTime = m_MaxAlphaIntensity;
        else if (m_CurrentTime < m_MinAlphaIntensity)
            m_CurrentTime = m_MinAlphaIntensity;

        m_MeshRenderer.GetPropertyBlock(m_Block);
        m_Block.SetFloat(m_AlphaIntensityID, m_CurrentTime);
        m_MeshRenderer.SetPropertyBlock(m_Block);
    }

    public void StartHighlight()
    {
        m_Highlighted = true;
    }

    public void StopHighlight()
    {
        m_Highlighted = false;
    }
}
