using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [System.Serializable]
    public struct EnvironmentSettings
    {
        public Color fogColor;
        public float fogStartDistance;
        public float fogEndDistance;
        public Color ambientLightColor;
        public Color directionalLightColor;
        public float directionalLightIntensity;
    }

    [Header("環境プロファイル")]
    public EnvironmentSettings darkState;   // 初期状態（暗い森）
    public EnvironmentSettings lightState;  // 変化後（明るい森）

    [Header("対象の参照")]
    public Light directionalLight;

    private void Start()
    {
        // 起動時は「暗い森」の設定を適用
        ApplyEnvironment(darkState);
    }

    public void ApplyEnvironment(EnvironmentSettings settings)
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogStartDistance = settings.fogStartDistance;
        RenderSettings.fogEndDistance = settings.fogEndDistance;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = settings.ambientLightColor;

        if (directionalLight != null)
        {
            directionalLight.color = settings.directionalLightColor;
            directionalLight.intensity = settings.directionalLightIntensity;
        }
    }

    // 将来的にこれをコルーチンなどで徐々に呼び出すことで、滑らかに霧が晴れる演出ができる
    public void TransitionToLight(float duration)
    {
        // ここにLerp（線形補間）を使った変化処理を記述する予定
    }
}