using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloLensModule.Environment;
using HoloLensModule.Network;

namespace DeviceLogSystem
{
    /// <summary>
    /// Targetオブジェクトの位置情報をUDPのブロードキャストアドレスで送信するサンプル
    /// </summary>
    public class DeviceLogSender : MonoBehaviour
    {
        /// <summary>
        /// 位置情報送信対象オブジェクト
        /// </summary>
        [SerializeField]private Transform target;

        private UDPSenderManager sender;
        // Use this for initialization
        void Start()
        {
            // UDPのブロードキャストで送信
            sender = new UDPSenderManager(SystemInfomation.DirectedBroadcastAddress, 8080);
            StartCoroutine(PositionUpdate(SystemInfomation.DeviceName, 0.1f));
        }

        void OnDestroy()
        {
            sender.DisConnectSender();
        }

        /// <summary>
        /// オブジェクト名と送信間隔を指定して送信
        /// </summary>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private IEnumerator PositionUpdate(string name, float interval)
        {
            var ms = new JsonMessage();
            ms.device = name;
            while (true)
            {
                yield return new WaitForSeconds(interval);
                var pos = transform.InverseTransformPoint(target.position);
                var rot = Quaternion.Inverse(transform.rotation) * target.rotation;
                ms.SetTransform(pos,rot);
                sender.SendMessage(JsonUtility.ToJson(ms));
            }
        }
    }

    /// <summary>
    /// 送信データ内容
    /// デバイス名，時間，位置，向きを送信
    /// </summary>
    [Serializable]
    public class JsonMessage
    {
        public string device = "";
        public int h = 0, m = 0, s = 0, mm = 0;
        public float px = 0.0f, py = 0.0f, pz = 0.0f;
        public float rx = 0.0f, ry = 0.0f, rz = 0.0f, rw = 0.0f;

        public void SetTransform(Vector3 pos,Quaternion rot)
        {
            var now = DateTime.Now;
            h = now.Hour;
            m = now.Minute;
            s = now.Second;
            mm = now.Millisecond;
            px = pos.x;
            py = pos.y;
            pz = pos.z;
            rx = rot.x;
            ry = rot.y;
            rz = rot.z;
            rw = rot.w;
        }

        public float GetTime()
        {
            return h * 3600 + m * 60 + s + (float) mm / 1000;
        }

        public Vector3 GetPos() { return new Vector3(px,py,pz);}
        public Quaternion GetRot() { return new Quaternion(rx,ry,rz,rw);}
    }
}
