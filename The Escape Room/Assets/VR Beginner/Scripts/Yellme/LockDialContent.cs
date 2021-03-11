using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDialContent : MonoBehaviour
{

    int m_DialNumber = 0;

    public void ChangeDialNumber(int step)
    {
        m_DialNumber = step;
        Debug.Log("m_DialNumber: " + m_DialNumber);
    }
}
