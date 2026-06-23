using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class ForestGenerator : MonoBehaviour
{
    [Header("配置するオブジェクトと親")]
    [Tooltip("木の代わりになるプレハブ（Cylinderなど）")]
    public GameObject treePrefab;
    [Tooltip("生成した木をまとめて格納する空のオブジェクト")]
    public Transform forestParent;

    [Header("生成パラメータ")]
    [Tooltip("手前エリア(1区画)に配置する木の数")]
    public int treeCount = 100;
    [Tooltip("ワープの距離 (WarpTriggerと同じ値にしてください)")]
    public float loopDistance = 100f;
    
    [Header("道の幅設定")]
    [Tooltip("この幅(X座標の絶対値)の中には木を置かない（道として空ける）")]
    public float pathWidth = 3f;
    [Tooltip("木を配置する最大範囲(X座標の絶対値)")]
    public float maxAreaWidth = 25f;

    // インスペクターの右上メニュー(ケバブメニュー)から実行できる機能
    [ContextMenu("森を生成する (Generate Forest)")]
    public void GenerateForest()
    {
        if (treePrefab == null || forestParent == null)
        {
            Debug.LogWarning("Tree Prefab または Forest Parent が設定されていません！");
            return;
        }

        // 1. 既存の木をすべて削除してリセット（何度も試行錯誤できるように）
        for (int i = forestParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(forestParent.GetChild(i).gameObject);
        }

        // 2. 指定された数だけ木を生成
        for (int i = 0; i < treeCount; i++)
        {
            // --- ① メインエリア（0 ～ loopDistance）の配置 ---
            float randomZ = Random.Range(0f, loopDistance);
                    
            float randomX = Random.Range(pathWidth, maxAreaWidth);
            if (Random.value > 0.5f) randomX = -randomX;
        
            Vector3 spawnPos = new Vector3(randomX, 0f, randomZ);
        
            GameObject tree1 = Instantiate(treePrefab, spawnPos, Quaternion.identity, forestParent);
            tree1.name = $"Tree_Main_{i}";
        
            tree1.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            float randomScale = Random.Range(0.8f, 1.5f);
            tree1.transform.localScale = treePrefab.transform.localScale * randomScale;
        
            // --- ② 前方エリア（ループ用コピー：+loopDistance） ---
            Vector3 forwardPos = spawnPos;
            forwardPos.z += loopDistance;
                    
            GameObject tree2 = Instantiate(treePrefab, forwardPos, tree1.transform.rotation, forestParent);
            tree2.transform.localScale = tree1.transform.localScale;
            tree2.name = $"Tree_Forward_{i}";
        
             // --- ③ 後方エリア（振り返り用コピー：-loopDistance） ---
             // ※ここが新しく追加した処理です
             Vector3 backwardPos = spawnPos;
             backwardPos.z -= loopDistance;
                    
             GameObject tree3 = Instantiate(treePrefab, backwardPos, tree1.transform.rotation, forestParent);
             tree3.transform.localScale = tree1.transform.localScale;
             tree3.name = $"Tree_Backward_{i}";
             
            #if UNITY_EDITOR
                    if (!Application.isPlaying) // 実行中（Playモード）以外の時だけ作動させる
                    {
                        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                    }
            #endif
        }

        Debug.Log($"合計 {treeCount * 3} 本の木を生成・配置しました！");
    }
}
