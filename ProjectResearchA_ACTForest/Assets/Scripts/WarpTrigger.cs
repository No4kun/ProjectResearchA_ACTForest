using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpTrigger : MonoBehaviour
{
    [Tooltip("ワープで引き戻す距離")]
    public float loopDistance = 90f;
    
    [Tooltip("OVRPlayerControllerをここにアタッチしてください")]
    public Transform player;

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
            
            // （※後ほど、ここに「蔓のオブジェクトたちも一緒に戻す」処理を追加します）
        }
    }
}