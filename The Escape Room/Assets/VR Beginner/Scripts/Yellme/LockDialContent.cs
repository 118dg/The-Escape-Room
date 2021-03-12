using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDialContent : MonoBehaviour
{

    int m_DialNumber = 0;

    [Header("Effects")]
    public Animator CauldronAnimator;

    bool m_CanBrew = false;

    [Header("Audio")]
    public AudioSource AmbientSoundSource;

    float m_StartingVolume;

    private void Start()
    {
        m_StartingVolume = AmbientSoundSource.volume;
        AmbientSoundSource.volume = m_StartingVolume * 0.2f;
    }

    public void Open()
    {
        CauldronAnimator.SetTrigger("Open");
        m_CanBrew = true;
        AmbientSoundSource.volume = m_StartingVolume;
    }

    public void ChangeDialNumber(int step)
    {
        m_DialNumber = step;
        Debug.Log("m_DialNumber: " + m_DialNumber);
    }
}
