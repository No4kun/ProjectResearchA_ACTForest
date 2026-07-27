using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpTrigger : MonoBehaviour
{
    [Tooltip("ワープで引き戻す距離")]
    public float loopDistance = 90f;
    
    [Tooltip("OVRPlayerControllerをここにアタッチしてください")]
    public Transform player;
    
    [Header("連携する演出マネージャー")]
    public MindTransitionManager transitionManager;
    
    void Start()
    {
        // 起動時に自動で MindTransitionManager を探し出して連携する
        if (transitionManager == null)
        {
            transitionManager = FindObjectOfType<MindTransitionManager>();
        }
    }
    
    void Update()
    {
        // プレイヤーが設定されていない場合は何もしない
        if (player == null) return;

        // プレイヤーのZ座標が loopDistance を超えたらワープ実行
        if (player.position.z >= loopDistance)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // ループ距離分だけ座標を戻す
            Vector3 newPos = player.position;
            newPos.z -= loopDistance;
            player.position = newPos;

            if (cc != null) cc.enabled = true;
            
            // ワープ処理が行われた直後に、これを呼び出す
            if (transitionManager != null)
            {
                transitionManager.AddLoopCount();
            }
        }
    }
}