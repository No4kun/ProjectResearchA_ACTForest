using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class WarpTrigger : MonoBehaviour
{
    [Tooltip("ワープで引き戻す距離（道の長さと同じにする）")]
    public float loopDistance = 80f;

    void OnTriggerEnter(Collider other)
    {
        // ぶつかったのがプレイヤーかどうか確認
        if (other.CompareTag("Player"))
        {
            // 【重要】CharacterControllerがアタッチされている場合、
            // 座標の直接変更が無視されることがあるため、一時的にオフにする
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
            }

            // スムーズなワープ処理（現在のZ座標からループ距離分を引く）
            Vector3 newPos = other.transform.position;
            newPos.z -= loopDistance;
            other.transform.position = newPos;

            // CharacterControllerをオンに戻す
            if (cc != null)
            {
                cc.enabled = true;
            }

            Debug.Log("ワープ機構作動：プレイヤーを " + newPos.z + " に移動しました。");
        }
    }
}