using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//残影控制类
public class GhostCtrl : MonoBehaviour
{
    private static Shader m_ghostShader = null;
    private MeshRenderer m_meshRenderer;
    private float m_duration = 0;//持续时间
    private float m_leftTime = 0;

    public static void GenGhost(List<SkinnedMeshRenderer> listRenderer, Color color, float duration)
    {
        if (listRenderer == null || listRenderer.Count == 0) return;
        if (m_ghostShader == null) m_ghostShader = Shader.Find("lijia/Xray");
        for (int i = 0; i < listRenderer.Count; i++)
        {
            Mesh mesh = new Mesh();
            listRenderer[i].BakeMesh(mesh);

            GameObject go = new GameObject();
            go.hideFlags = HideFlags.HideAndDontSave;

            GhostCtrl ghostCtrl = go.AddComponent<GhostCtrl>();//控制残影消失
            ghostCtrl.m_duration = duration;
            ghostCtrl.m_leftTime = duration;

            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            MeshRenderer meshRen = go.AddComponent<MeshRenderer>();

            meshRen.material = listRenderer[i].material;
            meshRen.material.shader = m_ghostShader;//设置xray效果
            meshRen.material.SetFloat("_Intension", 1);//颜色强度传入shader中
            meshRen.material.SetColor("_RimColor", color);

            go.transform.localScale = listRenderer[i].transform.localScale;
            go.transform.position = listRenderer[i].transform.position;
            go.transform.rotation = listRenderer[i].transform.rotation;

            ghostCtrl.m_meshRenderer = meshRen;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_leftTime -= Time.deltaTime;
        if (m_leftTime <= 0) Destroy(gameObject);
        else if (m_meshRenderer.material)
        {
            float rate = m_leftTime / m_duration;//计算生命周期的比例
            Color cal = m_meshRenderer.material.GetColor("_RimColor");
            cal.a = rate;//设置透明通道
            m_meshRenderer.material.SetColor("_RimColor", cal);
        }
    }
}
