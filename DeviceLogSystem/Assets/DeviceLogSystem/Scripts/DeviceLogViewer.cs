using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeviceLogSystem
{
    /// <summary>
    /// DeviceLogReceiverWPFで保存した位置情報を再現するサンプル
    /// Pキーで再生，停止
    /// 左右矢印キーで前後フレームへ移動
    /// </summary>
    public class DeviceLogViewer : MonoBehaviour
    {
        /// <summary>
        /// 位置情報再生対象オブジェクト
        /// </summary>
        [SerializeField] private Transform target;

        private List<JsonMessage> list = new List<JsonMessage>();
        private int count = 0;
        private bool playerFlag = false;
        private float start,end,current ;

        // Use this for initialization
        void Start()
        {
            target.parent = transform;
            // ファイルを読み込んで位置情報をリストに格納
            var path = @"..\DeviceLogReceiverWPF\DeviceLogReceiverWPF\bin\Debug\Transform.txt";
            if (File.Exists(path) == true)
            {
                var json = new JsonMessage();
                foreach (var line in File.ReadAllLines(path))
                {
                    json = JsonUtility.FromJson<JsonMessage>(line);
                    if (json != null) list.Add(json);
                }
                Debug.Log("Loaded File");
                start = list[0].GetTime();
                end = list[list.Count - 1].GetTime();
                current = start;
                count = 0;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // キーボードの左右矢印キーで時間を移動
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (list.Count > count) count++;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (count > 0) count--;
            }

            // 自動再生
            if (Input.GetKeyUp(KeyCode.P)) playerFlag = !playerFlag;
            if (Input.GetKeyUp(KeyCode.R)) current = start;
            if (playerFlag)
            {
                current += Time.deltaTime;
                // 初めに戻す
                if (current > end) current = start;
                for (var i = 0; i < list.Count; i++)
                {
                    if (current <= list[i].GetTime())
                    {
                        count = i;
                        break;
                    }
                }
            }

            // timecountに対応した位置情報を設定
            if (list.Count > count)
            {
                target.localPosition = list[count].GetPos();
                target.localRotation = list[count].GetRot();
            }
        }
    }
}
