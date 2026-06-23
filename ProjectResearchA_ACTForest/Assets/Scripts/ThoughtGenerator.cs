using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.SceneManagement; // エディタのシーン管理機能を使うために必要
#endif

public class ThoughtGenerator : MonoBehaviour
{
    [Header("設定")]
    public GameObject thoughtPrefab;
    [Tooltip("OVRPlayerController をセットしてください")]
    public Transform playerTransform; 
    [Tooltip("生成した蔦をまとめる親オブジェクト（空のGameObject）")]
    public Transform thoughtParent;
    
    [Tooltip("WarpTriggerと同じワープ距離（ループ距離）を設定してください")]
    public float loopDistance = 100f;

    [Header("配置エリア")]
    public float pathWidth = 2.5f;
    public float maxAreaWidth = 10f;

    [Header("テスト用一括生成設定")]
    public string testThoughtText = "私はダメだ";
    public int testSpawnCount = 100;
    public float spawnZRange = 100f;

    [ContextMenu("思考の蔦を一括生成 (Generate Multiple)")]
    public void GenerateMultipleThoughts()
    {
        if (thoughtPrefab == null || playerTransform == null || thoughtParent == null) return;

        // リセット処理
        for (int i = thoughtParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(thoughtParent.GetChild(i).gameObject);
        }

        // 一括生成
        for (int i = 0; i < testSpawnCount; i++)
        {
            float randomZ = playerTransform.position.z + Random.Range(5f, spawnZRange);
            GenerateThoughtAtDistance(testThoughtText, randomZ, i);
        }

        Debug.Log($"『{testThoughtText}』の蔦を {testSpawnCount} 本（×3エリア分）一括生成しました！");
    }

    [ContextMenu("思考の蔦を1つ生成 (Test Single Spawn)")]
    public void TestSpawnSingleThought()
    {
        float targetZ = playerTransform.position.z + 15f;
        // 1つ生成する際も、インデックスにランダムな値を入れて名前が被らないようにする
        GenerateThoughtAtDistance(testThoughtText, targetZ, Random.Range(1000, 9999));
    }

    // --- ループ対応の内部生成処理 ---
    private void GenerateThoughtAtDistance(string textToDisplay, float targetZ, int index)
    {
        // 1. 基本となるX座標と回転を決定（3つのコピーで全て同じ形にするため）
        // 1. 0.0 ～ 1.0 の間でベースとなるランダム値を出す
        float randomT = Random.value;

        // 2. 値を2乗して「0に近い値（道の近く）」が出現する確率を圧倒的に高くする
        // （もっと密集させたい場合は randomT * randomT * randomT と3乗にしてください）
        float bias = randomT * randomT;

        // 3. Mathf.Lerpを使って、pathWidth(道沿い) と maxAreaWidth(森の奥) の間で座標を決定する
        float randomX = Mathf.Lerp(pathWidth, maxAreaWidth, bias);
    
        // 4. 左右の振り分け
        if (Random.value > 0.5f) randomX = -randomX; 

        Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // --- ① メインエリア ---
        Vector3 mainPos = new Vector3(randomX, 0f, targetZ);
        GameObject mainThought = Instantiate(thoughtPrefab, mainPos, randomRot, thoughtParent);
        mainThought.name = $"ThoughtVine_{index}_Main";
        SetThoughtText(mainThought, textToDisplay);

        // --- ② 前方エリア（ループ用コピー：+loopDistance） ---
        Vector3 forwardPos = mainPos;
        forwardPos.z += loopDistance;
        GameObject forwardThought = Instantiate(thoughtPrefab, forwardPos, randomRot, thoughtParent);
        forwardThought.name = $"ThoughtVine_{index}_Forward";
        SetThoughtText(forwardThought, textToDisplay);

        // --- ③ 後方エリア（振り返り用コピー：-loopDistance） ---
        Vector3 backwardPos = mainPos;
        backwardPos.z -= loopDistance;
        GameObject backwardThought = Instantiate(thoughtPrefab, backwardPos, randomRot, thoughtParent);
        backwardThought.name = $"ThoughtVine_{index}_Backward";
        SetThoughtText(backwardThought, textToDisplay);
        
        #if UNITY_EDITOR
                if (!Application.isPlaying) // 実行中（Playモード）以外の時だけ作動させる
                {
                    EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
        #endif
    }

    // テキスト書き換え用のヘルパーメソッド
    private void SetThoughtText(GameObject thoughtObj, string text)
    {
        TextMeshPro tmp = thoughtObj.GetComponentInChildren<TextMeshPro>();
        if (tmp != null) tmp.text = text;
    }
}