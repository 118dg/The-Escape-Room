using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDialContent : MonoBehaviour
{

    int m_DialNumber = 0;

    public void ChangeTemperature(int step)
    {
        m_DialNumber = step;
        m_CauldronEffect.SetBubbleIntensity(step);
    }
}
