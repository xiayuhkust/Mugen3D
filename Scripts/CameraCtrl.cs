using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [HideInInspector]public Camera m_camera;
    [HideInInspector] public Transform m_transform;
    public static CameraCtrl m_cameraCtrl;

    private static float m_cameraShakeTime = 0;//屏幕震动时长
    private static int m_cameraShakeType = 0;//屏幕震动幅度，0-小，1-中，2-大
    private static Rect m_cameraRect;//屏幕震动参数
    private static bool m_resetCamera = false;//屏幕震动参数
    private static float m_fps = 20.0f;//屏幕震动参数
    private static float m_frameTime = 0.0f;//屏幕震动参数
    private static float m_shakeRange = 0.05f;//手动指定震动幅度

    public void InitComponent()
    {
        m_camera = GetComponent<Camera>();
        m_transform = transform;
        m_cameraRect = m_camera.rect;
    }
    // Update is called once per frame
    public void UpdateCamera(Vector3 pos1, Vector3 pos2)
    {
        float dis = Vector3.Distance(pos1, pos2);
        float cameraDis = dis * 0.8f;
        if (ScreenShake(cameraDis*10)) return;
        if (cameraDis < 2.5f) cameraDis = 2.5f;
        Vector3 centerPos = (pos1 + pos2) * 0.5f;
        Vector3 vec = pos2 - pos1;
        Vector3 vecVertical = new Vector3(vec.z, vec.y, -vec.x);
        vecVertical.Normalize();
        Vector3 cameraPos = centerPos + vecVertical * cameraDis;
        centerPos.y = cameraPos.y = 1.2f;
        Vector3 cameraForward = centerPos - cameraPos;
        m_transform.position = cameraPos;
        m_transform.forward = cameraForward;
    }
    public static void StartShake(float duration, int shakeType, float shakeRange)
    {
        m_cameraShakeTime = duration;
        m_cameraShakeType = shakeType;
        m_shakeRange = shakeRange;
        if (m_shakeRange > 10f) m_shakeRange = 10f;
        else if (m_shakeRange < 0.1f) m_shakeRange = 0.1f;
    }
    public bool ScreenShake(float cameraDis)
    {
        if (m_cameraShakeTime > 0)
        {
            float shakeDelta = m_shakeRange * 0.01f;
            if (m_cameraShakeType == 0) shakeDelta = 0.01f;
            else if (m_cameraShakeType == 1) shakeDelta = 0.03f;
            else if (m_cameraShakeType == 2) shakeDelta = 0.06f;
            shakeDelta = shakeDelta * 10 / cameraDis;
            if (shakeDelta < 0.01f) shakeDelta = 0.01f;
            else if (shakeDelta > 0.1f) shakeDelta = 0.1f;
            m_resetCamera = true;
            m_cameraShakeTime -= Time.deltaTime;
            m_frameTime += Time.deltaTime;
            if (m_frameTime > 1.0 / m_fps)
            {
                m_frameTime = 0;
                m_camera.rect = new Rect(shakeDelta * (-1.0f + 2.0f * UnityEngine.Random.value), shakeDelta * (-1.0f + 2.0f * UnityEngine.Random.value), 1.0f, 1.0f);
            }
            return true;
        }
        if (m_resetCamera)
        {
            m_resetCamera = false;
            m_camera.rect = m_cameraRect;
            m_frameTime = 0.03f;
        }
        return false;
    }
}
