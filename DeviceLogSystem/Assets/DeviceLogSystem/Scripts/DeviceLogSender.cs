using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DeviceLogSystem
{
    public class DeviceLogSender : MonoBehaviour
    {

        public GameObject CenterObject;
        public GameObject TargetObject;

        private UDPSenderManager sender;
        // Use this for initialization
        void Start()
        {
            sender = new UDPSenderManager(SystemInfomation.DirectedBroadcastAddress, 8080);
            StartCoroutine(PositionUpdate(SystemInfomation.DeviceName, 1.0f));
        }

        void OnDestroy()
        {
            sender.DisConnectSender();
        }

        private IEnumerator PositionUpdate(string name, float t)
        {
            JsonMessage ms = new JsonMessage();
            ms.device = name;
            while (true)
            {
                yield return new WaitForSeconds(t);
                ms.SetTime();
                ms.pos = CenterObject.transform.InverseTransformPoint(TargetObject.transform.position);
                ms.rot = Quaternion.Inverse(CenterObject.transform.rotation) * TargetObject.transform.rotation;
                sender.SendMessage(JsonUtility.ToJson(ms));
            }
        }
    }

    [Serializable]
    public class JsonMessage
    {
        public string device = "";
        public int h = 0, m = 0, s = 0;
        public float px = 0.0f, py = 0.0f, pz = 0.0f;
        public float rx = 0.0f, ry = 0.0f, rz = 0.0f, rw = 0.0f;
        public JsonMessage()
        {

        }
        public void SetTime()
        {
            DateTime now = DateTime.Now;
            h = now.Hour;
            m = now.Minute;
            s = now.Second;
        }
        public Vector3 pos
        {
            get { return new Vector3(px, py, pz); }
            set
            {
                px = value.x;
                py = value.y;
                pz = value.z;
            }
        }

        public Quaternion rot
        {
            get { return new Quaternion(rx, ry, rz, rw); }
            set
            {
                rx = value.x;
                ry = value.y;
                rz = value.z;
                rw = value.w;
            }
        }
    }
}
