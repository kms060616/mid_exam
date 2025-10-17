using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmSwing : MonoBehaviour
{
    [Header("�� �ǹ�(��� ��ġ�� Empty)")]
    public Transform armPivot;

    [Header("���� ����")]
    public float windupAngle = -20f;   
    public float hitAngle = 55f;    
    public float windupTime = 0.06f;  
    public float hitTime = 0.08f;  
    public float recoverTime = 0.10f;
    public Vector3 swingAxis = Vector3.up;

    [Header("��Ÿ��/�ߺ� ����")]
    public float lockTime = 0.05f;

    Quaternion _restRot;   
    bool _busy;

    void Awake()
    {
        if (armPivot == null) Debug.LogWarning("[ArmMeleeSwing] armPivot ������");
        _restRot = armPivot ? armPivot.localRotation : Quaternion.identity;
    }

    public void TriggerSwing()
    {
        if (!_busy && armPivot != null) StartCoroutine(SwingRoutine());
    }

    IEnumerator SwingRoutine()
    {
        _busy = true;

        
        yield return RotateToAngle(_restRot, windupAngle, windupTime);

        
        yield return RotateToAngle(_restRot, hitAngle, hitTime);

        float t = 0f;
        Quaternion from = armPivot.localRotation;
        while (t < recoverTime)
        {
            t += Time.deltaTime;
            float k = t / recoverTime;
            armPivot.localRotation = Quaternion.Slerp(from, _restRot, k);
            yield return null;
        }
        armPivot.localRotation = _restRot;

        
        yield return new WaitForSeconds(lockTime);
        _busy = false;
    }

    IEnumerator RotateToAngle(Quaternion rest, float angle, float dur)
    {
        float t = 0f;
        
        Quaternion target = Quaternion.AngleAxis(angle, swingAxis) * rest;
        Quaternion start = armPivot.localRotation;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            // �ణ�� ����/����
            float ease = k * k * (3f - 2f * k);
            armPivot.localRotation = Quaternion.Slerp(start, target, ease);
            yield return null;
        }
        armPivot.localRotation = target;
    }

    
    public void RebindRestRotation()
    {
        if (armPivot) _restRot = armPivot.localRotation;
    }
}
