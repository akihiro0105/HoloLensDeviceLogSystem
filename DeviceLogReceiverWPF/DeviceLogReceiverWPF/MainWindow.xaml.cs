using HoloLensModule.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceLogReceiverWPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<string> deviceIPList = new List<string>();
            // UDP Receiver
            UDPListenerManager listener = new UDPListenerManager(8080);
            listener.ListenerMessageEvent += (ms, ip) =>
            {
                JsonMessage jm = new JsonMessage();
                jm = JsonConvert.DeserializeObject<JsonMessage>(ms);
                if (deviceIPList.BinarySearch(ip) < 0)
                {
                    deviceIPList.Add(ip);
                    ListBox1.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        StackPanel stp = new StackPanel();
                        stp.Orientation = Orientation.Horizontal;
                        stp.Children.Add(new TextBlock() { Text = " " + jm.device });
                        stp.Children.Add(new TextBlock() { Text = " (" + ip + ")" });
                        stp.Children.Add(new TextBlock() { Text = "   " + "0.0%" });
                        ListBox1.Items.Add(stp);
                    }));
                }
                System.IO.StreamWriter sw = new System.IO.StreamWriter(@".\Transform_" + jm.device + ".txt", true);
                sw.WriteLine(JsonConvert.SerializeObject(jm));
                sw.Close();
            };

            // restAPI
            string user = "hololab";
            string pass = "hololab";
            Task.Run(async () =>
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslpolicyerrors) => true;
                HttpClient httpClient = new HttpClient();
                var param = Convert.ToBase64String(Encoding.UTF8.GetBytes(user + ":" + pass));
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", param);
                while (true)
                {
                    int count = 0;
                    foreach (var item in deviceIPList)
                    {
                        var result = httpClient.GetAsync("https://" + item + "/api/power/battery").Result;
                        var stringdata = await result.Content.ReadAsStringAsync();
                        JsonDevicePortal jdp = new JsonDevicePortal();
                        try
                        {
                            jdp = JsonConvert.DeserializeObject<JsonDevicePortal>(stringdata);
                            float battery = (float)jdp.RemainingCapacity / jdp.MaximumCapacity * 100;
                            await ListBox1.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                TextBlock tb = (TextBlock)((StackPanel)ListBox1.Items[count]).Children[2];
                                tb.Text = "   " + battery.ToString("##.#") + "%";
                            }));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        count++;
                    }
                    await Task.Delay(10000);
                }
            });
        }
    }

    [Serializable]
    public class JsonMessage
    {
        public string device = "";
        public int h = 0, m = 0, s = 0;
        public float px = 0.0f, py = 0.0f, pz = 0.0f;
        public float rx = 0.0f, ry = 0.0f, rz = 0.0f, rw = 0.0f;
    }

    [Serializable]
    public class JsonDevicePortal
    {
        public uint AcOnline;
        public uint BatteryPresent;
        public uint Charging;
        public uint DefaultAlert1;
        public uint DefaultAlert2;
        public uint EstimatedTime;
        public uint MaximumCapacity;
        public uint RemainingCapacity;
    }
}
