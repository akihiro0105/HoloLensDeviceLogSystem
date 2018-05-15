using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeviceLogSystem
{
    public class DeviceLogViewer : MonoBehaviour
    {
        public GameObject CenterObject;
        public GameObject LogObjectPrefab;

        private List<JsonDataList> logObjectList = new List<JsonDataList>();
        private long timecount = -1;
        // Use this for initialization
        void Start()
        {
            LoadDataListFile(@"..\DeviceLogReceiverWPF\DeviceLogReceiverWPF\bin\Debug\Transform_HOLOLENS-2LBOV.txt");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                timecount++;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                timecount--;
                if (timecount < 0) timecount = 0;
            }

            foreach (var item in logObjectList)
            {
                item.SetObjectTransform(timecount);
            }
        }

        private void LoadDataListFile(string path)
        {
            JsonDataList data = new JsonDataList(Instantiate(LogObjectPrefab, CenterObject.transform), Path.GetFileNameWithoutExtension(path));
            if (File.Exists(path) == true)
            {
                var readlines = File.ReadAllLines(path);
                for (int i = 0; i < readlines.Length; i++)
                {
                    if (readlines[i].Length > 1)
                    {
                        JsonMessage json = new JsonMessage();
                        json = JsonUtility.FromJson<JsonMessage>(readlines[i]);
                        if (json != null) data.device.Add(json);
                    }
                }
                long buf = data.device[0].h * 3600 + data.device[0].m * 60 + data.device[0].s;
                if (timecount > buf || timecount<0) timecount = buf;
                logObjectList.Add(data);
            }
        }
    }

    [Serializable]
    public class JsonDataList
    {
        public GameObject go;
        public List<JsonMessage> device;
        public JsonDataList(GameObject go, string name)
        {
            this.go = go;
            this.go.name = name;
            device = new List<JsonMessage>();
        }
        public void SetObjectTransform(long time)
        {
            foreach (var item in device)
            {
                long buf = item.h * 3600 + item.m * 60 + item.s;
                if (time <= buf)
                {
                    go.transform.localPosition = item.pos;
                    go.transform.localRotation = item.rot;
                    break;
                }
            }
        }
    }
}
