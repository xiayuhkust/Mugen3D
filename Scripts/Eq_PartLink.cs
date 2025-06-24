using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicaCloth;

public enum PhysicType
{
    NoPhysics,
    Upper,
    Lower,
    Cape,
    Total,
    Robe,//罩袍
}

public enum PartType//配件类型，用于识别特殊配件，如头部
{
    None=0,
    HeadBeardEyebrow,//身体、胡子、眉毛
    Eye,//眼球
    Eyelash,//睫毛
    Hair,//头发
    HairBall,//头发丸子
}

public class Eq_PartLink : MonoBehaviour
{
    public SkinnedMeshRenderer smr;
    public int sort;
    public MagicaMeshCloth meshCloth;
    public List<GameObject> prefabColList = new List<GameObject>();
    public PhysicType physicType;
    public PartType partType;
    [HideInInspector] public MagicaAvatarParts part;

    private void OnEnable()
    {
        part = GetComponent<MagicaAvatarParts>();
    }

}
