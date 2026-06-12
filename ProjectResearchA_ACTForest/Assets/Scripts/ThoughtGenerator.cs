using UnityEngine;
using TMPro;

public class ThoughtGenerator : MonoBehaviour
{
    [Header("設定")]
    public GameObject thoughtPrefab;
    
    [Tooltip("OVRPlayerController ではなく、その中にある CenterEyeAnchor をセットしてください")]
    public Transform vrCameraTransform; 
    
    public float spawnDistance = 15f;

    [Header("テスト用")]
    public string testThoughtText = "私はダメだ";

    [ContextMenu("思考オブジェクトを生成 (Test Spawn)")]
    public void TestSpawnThought()
    {
        GenerateThought(testThoughtText);
    }

    public void GenerateThought(string textToDisplay)
    {
        if (thoughtPrefab == null || vrCameraTransform == null) return;

        // 1. カメラの正面のベクトルを取得
        Vector3 forwardDir = vrCameraTransform.forward;
        
        // 2. Y軸（上下の傾き）を 0 にして水平なベクトルにする
        forwardDir.y = 0;
        
        // 3. ベクトルの長さを 1 に戻す（正規化）
        forwardDir.Normalize();

        // 4. 水平方向に spawnDistance 分だけ進んだ位置を計算
        Vector3 spawnPos = vrCameraTransform.position + (forwardDir * spawnDistance);
        
        // 5. 高さを強制的に固定する（例：地面から 1.5m の目の高さ）
        spawnPos.y = 1.5f;

        // 文字がプレイヤーの方を向くように回転
        Quaternion spawnRot = Quaternion.LookRotation(spawnPos - vrCameraTransform.position);
        // ※文字が斜め上や下を向かないよう、回転のY軸の傾きも補正したい場合は以下を有効にします
        // spawnRot.x = 0; spawnRot.z = 0;

        GameObject newThought = Instantiate(thoughtPrefab, spawnPos, spawnRot);

        TextMeshPro tmp = newThought.GetComponentInChildren<TextMeshPro>();
        if (tmp != null) tmp.text = textToDisplay;

        Debug.Log($"生成完了: 水平方向 {spawnDistance}m 先、高さ {spawnPos.y}m に出現");
    }
}