using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BodyPartType
{
    Head=0,//头
    Neck,//脖子
    Chest,//胸部
    AbdomenUp,//上腹部
    AbdomenDown,//下腹部
    RightArm,//右上臂
    LeftArm,//左上臂
    RightForearm,//右前臂
    LeftForearm,//左前臂
    RightHand,//右手
    LeftHand,//左手
    RightThigh,//右大腿
    LeftThigh,//左大腿
    RightShin,//右小腿
    LeftShin,//左小腿
    RightFoot,//右脚
    LeftFoot,//左脚
}
public class BodyColliderCtrl : MonoBehaviour
{
    public BodyPartType m_type = BodyPartType.Head;//类型，当为attack时忽略
    public HeroCtrl m_heroCtrl;//关联的人物模型
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("BodyCollider") || m_heroCtrl.m_ignoreCollider) return;
        BodyColliderCtrl bcCtrl = other.GetComponent<BodyColliderCtrl>();
        if (bcCtrl.m_heroCtrl == m_heroCtrl) return;//自己的碰撞体忽略
        m_heroCtrl.m_listMyCollider.Add((int)m_type);
        m_heroCtrl.m_listOtherCollider.Add((int)bcCtrl.m_type);
    }
}
