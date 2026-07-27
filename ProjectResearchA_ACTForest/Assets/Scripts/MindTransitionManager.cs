using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro; // TextMeshProを使用

public class MindTransitionManager : MonoBehaviour
{
    [Header("ループ設定")]
    public int maxLoops = 12;
    public int currentLoop = 0;

    [Header("フェードさせるプレハブ（蔓＋文字）")]
    public GameObject vineWithTextPrefab;

    [Header("空間のVolume設定（ドラッグ＆ドロップ）")]
    public PostProcessVolume initialVolume;
    public PostProcessVolume finalVolume;

    // 内部で自動取得するマテリアル
    private Material vineMaterial;
    private Material tmpMaterial;

    private Color initialVineColor;
    private Color initialTmpFaceColor;
    private Color initialTmpEmission;
    private Color initialTmpGlow;
    private Color initialTmpOutline;

    private void Start()
    {
        if (vineWithTextPrefab != null)
        {
            // 変更点1：TMP_Textにすることで、3D用もUI用も両方見つけ出します
            // 変更点2：(true)を入れることで、非表示のオブジェクトからも探し出します
            TMP_Text textMesh = vineWithTextPrefab.GetComponentInChildren<TMP_Text>(true);
            
            if (textMesh != null)
            {
                Debug.Log($"<color=green>【成功】テキストを発見しました: {textMesh.gameObject.name}</color>");
                tmpMaterial = textMesh.fontSharedMaterial;
                
                // 色々なTMPプロパティの初期色を記憶
                if (tmpMaterial.HasProperty("_FaceColor")) initialTmpFaceColor = tmpMaterial.GetColor("_FaceColor");
                if (tmpMaterial.HasProperty("_EmissionColor")) initialTmpEmission = tmpMaterial.GetColor("_EmissionColor");
                if (tmpMaterial.HasProperty("_GlowColor")) initialTmpGlow = tmpMaterial.GetColor("_GlowColor");
                if (tmpMaterial.HasProperty("_OutlineColor")) initialTmpOutline = tmpMaterial.GetColor("_OutlineColor");
            }
            else
            {
                Debug.LogWarning("<color=red>【警告】プレハブの中からTextMeshProが見つかりませんでした！</color>");
            }

            // 蔓本体の取得
            Renderer[] allRenderers = vineWithTextPrefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in allRenderers)
            {
                if (r.GetComponent<TMP_Text>() == null)
                {
                    vineMaterial = r.sharedMaterial;
                    initialVineColor = vineMaterial.color;
                    Debug.Log($"<color=green>【成功】蔓のマテリアルを発見しました: {vineMaterial.name}</color>");
                    break;
                }
            }
        }

        ApplyTransition(0f);
    }

    [ContextMenu("ループを1進める (テスト用)")]
    public void AddLoopCount()
    {
        if (currentLoop < maxLoops)
        {
            currentLoop++;
            float progress = (float)currentLoop / maxLoops;
            ApplyTransition(progress);
        }
    }

    private void ApplyTransition(float progress)
    {
        if (initialVolume != null && finalVolume != null)
        {
            initialVolume.weight = Mathf.Lerp(1.0f, 0.0f, progress);
            finalVolume.weight = Mathf.Lerp(0.0f, 1.0f, progress);
        }

        if (vineMaterial != null)
        {
            Color vColor = initialVineColor;
            vColor.a = Mathf.Lerp(1.0f, 0.0f, progress);
            vineMaterial.color = vColor;
        }

        // 変更点3：文字に関する「すべての色（輪郭やGlow含む）」を強制的に透明にします
        if (tmpMaterial != null)
        {
            float alpha = Mathf.Lerp(1.0f, 0.0f, progress);

            if (tmpMaterial.HasProperty("_FaceColor"))
            {
                Color c = initialTmpFaceColor; c.a = alpha; tmpMaterial.SetColor("_FaceColor", c);
            }
            if (tmpMaterial.HasProperty("_EmissionColor"))
            {
                tmpMaterial.SetColor("_EmissionColor", initialTmpEmission * alpha);
            }
            if (tmpMaterial.HasProperty("_GlowColor"))
            {
                Color c = initialTmpGlow; c.a = alpha; tmpMaterial.SetColor("_GlowColor", c);
            }
            if (tmpMaterial.HasProperty("_OutlineColor"))
            {
                Color c = initialTmpOutline; c.a = alpha; tmpMaterial.SetColor("_OutlineColor", c);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // 終了時に元に戻す処理
        if (vineMaterial != null) vineMaterial.color = initialVineColor;
        if (tmpMaterial != null)
        {
            if (tmpMaterial.HasProperty("_FaceColor")) tmpMaterial.SetColor("_FaceColor", initialTmpFaceColor);
            if (tmpMaterial.HasProperty("_EmissionColor")) tmpMaterial.SetColor("_EmissionColor", initialTmpEmission);
            if (tmpMaterial.HasProperty("_GlowColor")) tmpMaterial.SetColor("_GlowColor", initialTmpGlow);
            if (tmpMaterial.HasProperty("_OutlineColor")) tmpMaterial.SetColor("_OutlineColor", initialTmpOutline);
        }
    }
}