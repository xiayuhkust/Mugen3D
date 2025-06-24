using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicaCloth;

public class Eq_Avatar : MonoBehaviour
{
    public Transform m_hip;
    public Transform m_neck;
    public Transform m_head;
    public Transform m_shoulder;
    public Transform[] m_thigh = new Transform[2];
    public Transform[] m_shank = new Transform[2];
    public Transform[] m_eyes = new Transform[2];
    public MagicaCapsuleCollider[] m_thighCollider = new MagicaCapsuleCollider[2];
    public MagicaCapsuleCollider[] m_shankCollider = new MagicaCapsuleCollider[2];
    public List<ColliderComponent> upperColliderList = new List<ColliderComponent>();
    public List<ColliderComponent> lowerColliderList = new List<ColliderComponent>();
    public List<ColliderComponent> totalColliderList = new List<ColliderComponent>();
    public List<ColliderComponent> m_totalColliderList2 = new List<ColliderComponent>();
    public List<ColliderComponent> m_robeColliderList = new List<ColliderComponent>();//罩袍
    [HideInInspector] public float m_thighStartRadius = 1;
    [HideInInspector] public float m_thighEndRadius = 1;
    [HideInInspector] public float m_shankStartRadius = 1;
    [HideInInspector] public float m_shankEndRadius = 1;
    [HideInInspector] public Vector3[] m_baseEyePos = new Vector3[2];//眼睛的初始局部z坐标

    private void OnEnable()
    {
        if (m_eyes[0] != null) m_baseEyePos[0] = m_eyes[0].localPosition;
        if (m_eyes[1] != null) m_baseEyePos[1] = m_eyes[1].localPosition;
        if (m_thighCollider[0] == null) return;
        m_thighStartRadius = m_thighCollider[0].StartRadius;//记录初始的腿部碰撞体半径
        m_thighEndRadius = m_thighCollider[0].EndRadius;
        m_shankStartRadius = m_shankCollider[0].StartRadius;
        m_shankEndRadius = m_shankCollider[0].EndRadius;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
