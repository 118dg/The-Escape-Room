using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Custom interactable than will rotation around a given axis. It can be limited in range and also be made either
/// continuous or snapping to integer steps.
/// Rotation can be driven either by controller rotation (e.g. rotating a volume dial) or controller movement (e.g.
/// pulling down a lever)
/// </summary>
public class DialInteractable : XRBaseInteractable
{
    public enum InteractionType
    {
        ControllerRotation,
        ControllerPull
    }
    
    [System.Serializable]
    public class DialTurnedAngleEvent : UnityEvent<float> { } //System이네 
    [System.Serializable]
    public class DialTurnedStepEvent : UnityEvent<int> { }

    [System.Serializable]
    public class DialChangedEvent : UnityEvent<DialInteractable> { }

    public InteractionType DialType = InteractionType.ControllerRotation;
    
    public Rigidbody RotatingRigidbody; //temperature lever
    public Vector3 LocalRotationAxis; //돌리는 기준 축
    public Vector3 LocalAxisStart; //처음에 가리키고 있는 방향
    public float RotationAngleMaximum; //최대 온도까지 돌리기 //315

    [Tooltip("If 0, this is a float dial going from 0 to 1, if not 0, that dial is int with that many steps")]
    public int Steps = 0; //온도 단계. 7. //나는 Steps=10
    public bool SnapOnRelease = true; //?

    public AudioClip SnapAudioClip; //돌리는 소리
    
    public DialTurnedAngleEvent OnDialAngleChanged;
    public DialTurnedStepEvent OnDialStepChanged;
    public DialChangedEvent OnDialChanged;

    public float CurrentAngle => m_CurrentAngle; //이 화살표는 무엇인가
    public int CurrentStep => m_CurrentStep;
    
    XRBaseInteractor m_GrabbingInteractor;
    Quaternion m_GrabbedRotation;
    
    Vector3 m_StartingWorldAxis;
    float m_CurrentAngle = 0;
    int m_CurrentStep = 0;
    
    float m_StepSize;
    Transform m_SyncTransform;
    Transform m_OriginalTransform;

    void Start()
    {
        //축 normalize
        LocalAxisStart.Normalize();
        LocalRotationAxis.Normalize();

        //temperature lever의 rigidbody 가져오기
        if (RotatingRigidbody == null) 
        {
            RotatingRigidbody = GetComponentInChildren<Rigidbody>();
        }
        
        m_CurrentAngle = 0;
        
        GameObject obj = new GameObject("Dial_Start_Copy"); //이 오브젝트가 왜 필요하지? //temperature lever의 위치값 정하는 오브젝트네 //근데 play될때만 나타나.. active되는 것도 아니고 그냥 뿅 나타나는데 이게 어떻게 가능?
        m_OriginalTransform = obj.transform;
        m_OriginalTransform.SetParent(transform.parent);
        m_OriginalTransform.localRotation = transform.localRotation;
        m_OriginalTransform.localPosition = transform.localPosition;
        
        if (Steps > 0) m_StepSize = RotationAngleMaximum / Steps; //m_StepSize = 315 / 7 = 45... 엥 15아닌가
        else m_StepSize = 0.0f;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) //파라미터 ?
    {
        if (isSelected) //isSelected == true고
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed) //Fixed면
            {
                m_StartingWorldAxis = m_OriginalTransform.TransformDirection(LocalAxisStart); 
                
                Vector3 worldAxisStart = m_SyncTransform.TransformDirection(LocalAxisStart); //world 좌표계로 바꾸나봄. StartVector를.
                Vector3 worldRotationAxis = m_SyncTransform.TransformDirection(LocalRotationAxis);

                float angle = 0.0f; //VR controller로 돌린 각도
                Vector3 newRight = worldAxisStart;
                
                if (DialType == InteractionType.ControllerRotation)
                {
                    Quaternion difference = m_GrabbingInteractor.transform.rotation * Quaternion.Inverse(m_GrabbedRotation);

                    newRight = difference * worldAxisStart;

                    //get the new angle between the original right and this new right along the up axis
                    angle = Vector3.SignedAngle(m_StartingWorldAxis, newRight, worldRotationAxis);

                    if (angle < 0) angle = 360 + angle;
                }
                else //﻿DialType이 Pull이면
                {
                    Vector3 centerToController = m_GrabbingInteractor.transform.position - transform.position;
                    centerToController.Normalize();

                    newRight = centerToController;

                    angle = Vector3.SignedAngle(m_StartingWorldAxis, newRight, worldRotationAxis);
                    
                    if (angle < 0) angle = 360 + angle;
                }

                /* angle 범위 조정 */
                //if the angle is < 0 or > to the max rotation, we clamp but TO THE CLOSEST (a simple clamp would clamp
                // of an angle of 350 for a 0-180 angle range would clamp to 180, when we want to clamp to 0)
                if (angle > RotationAngleMaximum)
                {
                    float upDiff = 360 - angle;
                    float lowerDiff = angle - RotationAngleMaximum;

                    if (upDiff < lowerDiff) angle = 0;
                    else angle = RotationAngleMaximum;
                }
                
                float finalAngle = angle;
                if (!SnapOnRelease && Steps > 0)
                {
                    int step = Mathf.RoundToInt(angle / m_StepSize);
                    finalAngle = step * m_StepSize;

                    //Debug.Log("Initial finalAngle: " + finalAngle + " Initial m_CurrentAngle: " + m_CurrentAngle);

                    if (!Mathf.Approximately(finalAngle , m_CurrentAngle)) //어쨋든 지금 finalAngle이랑 m_CurrentAngle이랑 다를 때라는 거 아닌가?
                    {
                        //Debug.Log("finalAngle: " +  finalAngle + " m_CurrentAngle: " + m_CurrentAngle);

                        SFXPlayer.Instance.PlaySFX(SnapAudioClip, transform.position, new SFXPlayer.PlayParameters() //SFX 소리 켜는거네
                        {
                            Pitch = UnityEngine.Random.Range(0.9f, 1.1f), //SFX Player 스크립트에 있는 public 변수
                            SourceID = -1,
                            Volume = 1.0f
                        }, 0.0f);
                        
                        OnDialStepChanged.Invoke(step); //여기서 스텝 달라질때마다 숫자 값 달라지는 거 저장하는거 해야함!
                        OnDialChanged.Invoke(this);
                        m_CurrentStep = step;
                    }
                }

                //first, we use the raw angle to move the sync transform, that allow to keep the proper current rotation
                //even if we snap during rotation
                newRight = Quaternion.AngleAxis(angle, worldRotationAxis) * m_StartingWorldAxis;
                angle = Vector3.SignedAngle(worldAxisStart, newRight, worldRotationAxis);
                Quaternion newRot = Quaternion.AngleAxis(angle, worldRotationAxis) * m_SyncTransform.rotation;

                //then we redo it but this time using finalAngle, that will snap if needed. //snap?>????
                newRight = Quaternion.AngleAxis(finalAngle, worldRotationAxis) * m_StartingWorldAxis;
                m_CurrentAngle = finalAngle;
                OnDialAngleChanged.Invoke(finalAngle);
                OnDialChanged.Invoke(this);
                finalAngle = Vector3.SignedAngle(worldAxisStart, newRight, worldRotationAxis);
                Quaternion newRBRotation = Quaternion.AngleAxis(finalAngle, worldRotationAxis) * m_SyncTransform.rotation; //이 과정 아까부터 계속 나오던데 정확히 뭔지 파악 좀 해야겠음 무슨 계산인지

                if (RotatingRigidbody != null)
                    RotatingRigidbody.MoveRotation(newRBRotation);
                else
                    transform.rotation = newRBRotation;
                
                m_SyncTransform.transform.rotation = newRot;

                m_GrabbedRotation = m_GrabbingInteractor.transform.rotation;
            }
        }
    }

    protected override void OnSelectEnter(XRBaseInteractor interactor)
    {
        m_GrabbedRotation = interactor.transform.rotation;
        m_GrabbingInteractor = interactor;

        //create an object that will track the rotation
        var syncObj = new GameObject("TEMP_DialSyncTransform");
        m_SyncTransform = syncObj.transform;
        
        if(RotatingRigidbody != null)
        {
            m_SyncTransform.rotation = RotatingRigidbody.transform.rotation;
            m_SyncTransform.position = RotatingRigidbody.position;
        }
        else
        {
            m_SyncTransform.rotation = transform.rotation;
            m_SyncTransform.position = transform.position;
        }
        
        base.OnSelectEnter(interactor);
    }

    protected override void OnSelectExit(XRBaseInteractor interactor)
    {
        base.OnSelectExit(interactor);
        
        if (SnapOnRelease && Steps > 0)
        {
            Vector3 right = transform.TransformDirection(LocalAxisStart);
            Vector3 up = transform.TransformDirection(LocalRotationAxis);
            
            float angle = Vector3.SignedAngle(m_StartingWorldAxis, right, up);
            if (angle < 0) angle = 360 + angle;

            int step = Mathf.RoundToInt(angle / m_StepSize);
            angle = step * m_StepSize;
            
            if (angle != m_CurrentAngle)
            {
                SFXPlayer.Instance.PlaySFX(SnapAudioClip, transform.position, new SFXPlayer.PlayParameters()
                {
                    Pitch = UnityEngine.Random.Range(0.9f, 1.1f),
                    SourceID = -1,
                    Volume = 1.0f
                }, 0.0f);
                
                OnDialStepChanged.Invoke(step);
                OnDialChanged.Invoke(this);
                m_CurrentStep = step;
            }
            
            Vector3 newRight = Quaternion.AngleAxis(angle, up) * m_StartingWorldAxis;
            angle = Vector3.SignedAngle(right, newRight, up);

            m_CurrentAngle = angle;

            if (RotatingRigidbody != null)
            {
                Quaternion newRot = Quaternion.AngleAxis(angle, up) * RotatingRigidbody.rotation;
                RotatingRigidbody.MoveRotation(newRot);
            }
            else
            {
                Quaternion newRot = Quaternion.AngleAxis(angle, up) * transform.rotation;
                transform.rotation = newRot;
            }
        }
        
        Destroy(m_SyncTransform.gameObject);
    }
    
    public override bool IsSelectableBy(XRBaseInteractor interactor)
    {
        int interactorLayerMask = 1 << interactor.gameObject.layer;
        return base.IsSelectableBy(interactor) && (interactionLayerMask.value & interactorLayerMask) != 0 ;
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Handles.DrawSolidArc(transform.position, transform.TransformDirection(LocalRotationAxis), transform.TransformDirection(LocalAxisStart), RotationAngleMaximum, 0.2f );
    }
    #endif
    
    
}
