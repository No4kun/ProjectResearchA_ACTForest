using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class MindTransitionManager : MonoBehaviour
{
    [Header("ループ設定")]
    public int maxLoops = 12;
    public int currentLoop = 0;
    public AnimationCurve transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("VR ウィンドウ（スタート・終了画面）")]
    public CanvasGroup startHUD; 
    public CanvasGroup endHUD;   
    public float uiFadeTime = 2.0f; 
    
    // ★修正：距離をインスペクターから自由に調整できるように変更
    [Header("終了画面の表示距離")]
    [Tooltip("顔から何メートル離すか（デフォルトは2.5m）")]
    public float hudDistance = 2.5f; 

    [Header("プレイヤー設定")]
    public OVRPlayerController playerController;
    public Transform centerEyeCamera; 

    [Header("フェードさせるプレハブ（蔓＋文字）")]
    public GameObject vineWithTextPrefab;

    [Header("空間のVolume設定")]
    public PostProcessVolume initialVolume;
    public PostProcessVolume finalVolume;

    [Header("Fog（霧）と環境光の設定")]
    public float darkFogEnd = 80f;
    public float brightFogEnd = 300f;
    public Color darkFogColor = new Color(0.05f, 0.05f, 0.1f);
    public Color brightFogColor = new Color(0.8f, 0.85f, 0.95f);
    public Color darkAmbientColor = new Color(0.1f, 0.1f, 0.15f);
    public Color brightAmbientColor = new Color(0.9f, 0.85f, 0.8f);

    [Header("Skybox（空）の設定")]
    public Color darkSkyTint = new Color(0.1f, 0.1f, 0.2f);
    public Color brightSkyTint = new Color(0.5f, 0.5f, 0.5f);
    public float darkSkyExposure = 0.1f;
    public float brightSkyExposure = 1.0f;

    // 内部変数
    private Material vineMaterial, tmpMaterial;
    private Color initialVineColor, initialTmpFaceColor, initialTmpEmission, initialTmpGlow, initialTmpOutline;
    private Material originalSkybox, skyboxInstance;

    private bool isGameStarted = false;
    private bool isGameEnded = false;
    private bool isEndFadeComplete = false; 

    private void Start()
    {
        currentLoop = 0;

        if (RenderSettings.skybox != null)
        {
            originalSkybox = RenderSettings.skybox;
            skyboxInstance = new Material(originalSkybox);
            RenderSettings.skybox = skyboxInstance;
        }

        if (vineWithTextPrefab != null)
        {
            TMP_Text textMesh = vineWithTextPrefab.GetComponentInChildren<TMP_Text>(true);
            if (textMesh != null)
            {
                tmpMaterial = textMesh.fontSharedMaterial;
                if (tmpMaterial.HasProperty("_FaceColor")) initialTmpFaceColor = tmpMaterial.GetColor("_FaceColor");
                if (tmpMaterial.HasProperty("_EmissionColor")) initialTmpEmission = tmpMaterial.GetColor("_EmissionColor");
                if (tmpMaterial.HasProperty("_GlowColor")) initialTmpGlow = tmpMaterial.GetColor("_GlowColor");
                if (tmpMaterial.HasProperty("_OutlineColor")) initialTmpOutline = tmpMaterial.GetColor("_OutlineColor");
            }

            Renderer[] allRenderers = vineWithTextPrefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in allRenderers)
            {
                if (r.GetComponent<TMP_Text>() == null)
                {
                    vineMaterial = r.sharedMaterial;
                    initialVineColor = vineMaterial.color;
                    break;
                }
            }
        }

        ApplyTransition(0f);

        if (startHUD != null) startHUD.alpha = 1f;
        if (endHUD != null) { endHUD.alpha = 0f; endHUD.gameObject.SetActive(false); }
        
        if (playerController != null) playerController.EnableLinearMovement = false;
    }

    private void Update()
    {
        bool isInputDetected = OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) || 
                               Input.GetKeyDown(KeyCode.Space) || 
                               Input.GetMouseButtonDown(0);

        if (!isGameStarted && isInputDetected)
        {
            StartCoroutine(StartGameProcess());
        }

        if (isEndFadeComplete && isInputDetected)
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    [ContextMenu("ループを1つ進める")]
    public void AddLoopCount()
    {
        if (currentLoop < maxLoops)
        {
            currentLoop++;
            float rawProgress = (float)currentLoop / maxLoops;
            ApplyTransition(rawProgress);

            if (currentLoop >= maxLoops)
            {
                StartCoroutine(EndGameProcess());
            }
        }
    }

    private void ApplyTransition(float rawProgress)
    {
        float progress = transitionCurve.Evaluate(rawProgress);

        if (initialVolume != null && finalVolume != null)
        {
            initialVolume.weight = Mathf.Lerp(1.0f, 0.0f, progress);
            finalVolume.weight = Mathf.Lerp(0.0f, 1.0f, progress);
        }

        if (vineMaterial != null)
        {
            Color vColor = initialVineColor; vColor.a = Mathf.Lerp(1.0f, 0.0f, progress);
            vineMaterial.color = vColor;
        }

        if (tmpMaterial != null)
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, progress);
            if (tmpMaterial.HasProperty("_FaceColor")) { Color c = initialTmpFaceColor; c.a = alpha; tmpMaterial.SetColor("_FaceColor", c); }
            if (tmpMaterial.HasProperty("_EmissionColor")) tmpMaterial.SetColor("_EmissionColor", initialTmpEmission * alpha);
            if (tmpMaterial.HasProperty("_GlowColor")) { Color c = initialTmpGlow; c.a = alpha; tmpMaterial.SetColor("_GlowColor", c); }
            if (tmpMaterial.HasProperty("_OutlineColor")) { Color c = initialTmpOutline; c.a = alpha; tmpMaterial.SetColor("_OutlineColor", c); }
        }

        RenderSettings.fogEndDistance = Mathf.Lerp(darkFogEnd, brightFogEnd, progress);
        RenderSettings.fogColor = Color.Lerp(darkFogColor, brightFogColor, progress);
        RenderSettings.ambientLight = Color.Lerp(darkAmbientColor, brightAmbientColor, progress);

        if (skyboxInstance != null)
        {
            if (skyboxInstance.HasProperty("_SkyTint")) skyboxInstance.SetColor("_SkyTint", Color.Lerp(darkSkyTint, brightSkyTint, progress));
            if (skyboxInstance.HasProperty("_Exposure")) skyboxInstance.SetFloat("_Exposure", Mathf.Lerp(darkSkyExposure, brightSkyExposure, progress));
        }
    }

    private IEnumerator StartGameProcess()
    {
        if (startHUD != null)
        {
            float timer = 0f;
            while (timer < uiFadeTime)
            {
                timer += Time.deltaTime;
                startHUD.alpha = Mathf.Lerp(1f, 0f, timer / uiFadeTime);
                yield return null;
            }
            startHUD.alpha = 0f;
            startHUD.gameObject.SetActive(false);
        }

        if (playerController != null) playerController.EnableLinearMovement = true;
        isGameStarted = true;
    }

    private IEnumerator EndGameProcess()
    {
        yield return new WaitForSeconds(2.0f);
        
        if (endHUD != null && centerEyeCamera != null)
        {
            Canvas parentCanvas = endHUD.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                // ★大修正：フワッとした追従を廃止し、カメラの完全な子オブジェクトとしてガッチリ固定する
                parentCanvas.transform.SetParent(centerEyeCamera);
                
                // カメラの真正面に hudDistance メートル離して配置
                parentCanvas.transform.localPosition = new Vector3(0f, 0f, hudDistance);
                parentCanvas.transform.localRotation = Quaternion.identity;
            }
            
            endHUD.transform.localPosition = Vector3.zero;
            endHUD.transform.localRotation = Quaternion.identity;

            endHUD.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < uiFadeTime)
            {
                timer += Time.deltaTime;
                endHUD.alpha = Mathf.Lerp(0f, 1f, timer / uiFadeTime);
                yield return null;
            }
            endHUD.alpha = 1f;

            isEndFadeComplete = true; 
            isGameEnded = true; 
        }
    }

    private void OnApplicationQuit()
    {
        if (vineMaterial != null) vineMaterial.color = initialVineColor;
        if (tmpMaterial != null)
        {
            if (tmpMaterial.HasProperty("_FaceColor")) tmpMaterial.SetColor("_FaceColor", initialTmpFaceColor);
            if (tmpMaterial.HasProperty("_EmissionColor")) tmpMaterial.SetColor("_EmissionColor", initialTmpEmission);
            if (tmpMaterial.HasProperty("_GlowColor")) tmpMaterial.SetColor("_GlowColor", initialTmpGlow);
            if (tmpMaterial.HasProperty("_OutlineColor")) tmpMaterial.SetColor("_OutlineColor", initialTmpOutline);
        }
        if (originalSkybox != null) RenderSettings.skybox = originalSkybox;
    }
}