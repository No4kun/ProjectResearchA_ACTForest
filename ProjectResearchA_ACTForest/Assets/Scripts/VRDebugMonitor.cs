using UnityEngine;
using TMPro; // TextMeshPro用

public class VRDebugMonitor : MonoBehaviour
{
    public TMP_Text debugText; // 表示用のテキスト
    public Transform player; // OVRPlayerController
    public MindTransitionManager transitionManager; // マネージャー

    void Update()
    {
        if (debugText != null && player != null && transitionManager != null)
        {
            // プレイヤーのZ座標と、現在のループ回数をリアルタイム表示
            debugText.text = $"Z Pos: {player.position.z:F1} m\nLoop: {transitionManager.currentLoop}";
        }
    }
}