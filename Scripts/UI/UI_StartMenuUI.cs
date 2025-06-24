using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_StartMenuUI : MonoBehaviour, UI_Base
{
    public Slider m_sliderProgress;
    public Text m_textProgress;
    public Text m_textVersion;
    private string m_strVersion = "v0.34";
    // Start is called before the first frame update
    void Start()
    {
        m_textVersion.text = m_strVersion;
        GlobalAssist.m_curUI = gameObject;
        if (GlobalAssist.m_globalUpdate == null)
        {
            Application.runInBackground = true;//先设为true，以便后台加载资源
            StartCoroutine(LoadAllResource());
        }
        else
        {
            
        }
    }

    IEnumerator LoadAllResource()
    {
        SetProgress(10);
        yield return null;
        yield return null;
        GameAssets.LoadAssetBundle();//最开始加载AssetBundle
        SetProgress(50);
        yield return null;
        GlobalAssist.InitTipsCanvas();
        //全局更新
        GameObject globalUpdate = GameAssets.LoadAsset<GameObject>("Assets/Resource/GlobalUpdate.prefab");
        globalUpdate = Instantiate(globalUpdate);
        DontDestroyOnLoad(globalUpdate);
        //背景音乐
        GameObject bgm = GameAssets.LoadAsset<GameObject>("Assets/Resource/BGMManager.prefab");
        bgm = Instantiate(bgm);
        BGMManager.m_instance = bgm.GetComponent<BGMManager>();
        DontDestroyOnLoad(bgm);
        //音效
        GameObject sounds = GameAssets.LoadAsset<GameObject>("Assets/Resource/SoundManager.prefab");
        sounds = Instantiate(sounds);
        DontDestroyOnLoad(sounds);
        //游戏相机
        GameObject camera = GameAssets.LoadAsset<GameObject>("Assets/Resource/BattleCamera.prefab");
        camera = Instantiate(camera);
        DontDestroyOnLoad(camera);
        CameraCtrl.m_cameraCtrl = camera.GetComponent<CameraCtrl>();
        CameraCtrl.m_cameraCtrl.InitComponent();
        //游戏UI
        GameObject gameUI = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/GameUI.prefab");
        gameUI = Instantiate(gameUI);
        DontDestroyOnLoad(gameUI);
        UI_GameUI.m_UI = gameUI.GetComponent<UI_GameUI>();
        UI_GameUI.Hide();

        //角色预览
        HeroModelPreviewCtrl.Init();
        Application.runInBackground = false;
        Hero.InitAll();
        AnimationManager.InitAllAnimationClip();//初始化动画片段
        InputData.InitKeyToInput();
    }
    void SetProgress(int progress)
    {
        //m_textProgress.text = progress + "%";
        //m_sliderProgress.value = progress;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    public void ButtonStartPress()
    {
        UI_SelectHeroUI.OpenUI();
    }
    public void ButtonEditHero()
    {
        UI_EditHeroUI.OpenUI();
    }
    public void ButtonQuitPress()
    {
        Application.Quit();
    }
    //UI_Base接口的实现
    public bool IsRootUI()
    {
        return true;
    }
    public void SetParent(GameObject parentUI)
    {

    }
    public GameObject GetParent()
    {
        return null;
    }
    public int GetSortOrder()//返回Canvas的sort order
    {
        return 10;
    }
    public void SetSortOrder(int order)//设置order
    {

    }
}
