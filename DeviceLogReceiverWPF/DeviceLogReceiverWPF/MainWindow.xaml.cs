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
        /// <summary>
        /// UDP受信IPアドレスリスト
        /// </summary>
        private List<string> ipList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            // UDP受信処理
            var listener = new UDPListenerManager(8080);
            listener.ListenerMessageEvent += (ms, ip) =>
            {
                setListBox(ip, ms);
                // ファイルに位置情報を保存
                var sw = new System.IO.StreamWriter(@".\Transform.txt", true);
                sw.WriteLine(ms);
                sw.Close();
            };

            // HoloLensのバッテリー残量取得
            sendRestAPI();
        }

        /// <summary>
        /// リストボックスを更新
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ms"></param>
        private void setListBox(string ip, string ms)
        {
            if (ipList.BinarySearch(ip) < 0)
            {
                var jm = new JsonMessage();
                jm = JsonConvert.DeserializeObject<JsonMessage>(ms);
                ipList.Add(ip);
                ListBox1.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var stp = new StackPanel();
                    stp.Tag = ip;
                    stp.Orientation = Orientation.Horizontal;
                    stp.Children.Add(new TextBlock() { Text = " " + jm.device });
                    stp.Children.Add(new TextBlock() { Text = " (" + ip + ")" });
                    stp.Children.Add(new TextBlock() { Text = "   " + "0.0%" });
                    ListBox1.Items.Add(stp);
                }));
            }
        }

        /// <summary>
        /// RestAPIによるHoloLensデバイスのバッテリー残量情報取得
        /// </summary>
        private void sendRestAPI()
        {
            // 初期化
            var user = "hololab";
            var pass = "hololab";
            username.Text = user;
            password.Text = pass;
            Task.Run(async () =>
            {
                // サーバー認証が必要な場合通過させる
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslpolicyerrors) => true;
                while (true)
                {
                    // ユーザー名，パスワード設定
                    await username.Dispatcher.BeginInvoke(new Action(() => { user = username.Text; }));
                    await password.Dispatcher.BeginInvoke(new Action(() => { pass = password.Text; }));

                    // ユーザー名とパスワードの暗号化
                    var param = Convert.ToBase64String(Encoding.UTF8.GetBytes(user + ":" + pass));

                    foreach (var item in ipList)
                    {
                        using (var client = new HttpClient())
                        {
                            // ユーザー名とパスワードをパラメータとして設定
                            client.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", param);
                            // RestAPIでGet要求
                            var result = client.GetAsync("https://" + item + "/api/power/battery").Result;
                            // レスポンスデータの取得
                            var data = await result.Content.ReadAsStringAsync();
                            var jdp = new JsonDevicePortal();
                            // バッテリー情報の取得
                            jdp = JsonConvert.DeserializeObject<JsonDevicePortal>(data);
                            if (jdp != null)
                            {
                                // バッテリー残量算出
                                var battery = (float) jdp.RemainingCapacity / jdp.MaximumCapacity * 100;
                                // 対象IPアドレスのリストにバッテリー残量更新
                                await ListBox1.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    foreach (StackPanel box in ListBox1.Items)
                                    {
                                        if (box.Tag.Equals(item))
                                        {
                                            var tb = (TextBlock) box.Children[2];
                                            tb.Text = "   " + battery.ToString("##.#") + "%";
                                            break;
                                        }
                                    }
                                }));
                            }
                        }
                    }

                    // 10秒間隔で取得
                    await Task.Delay(10000);
                }
            });
        }
    }

    /// <summary>
    /// UDP受信データ
    /// </summary>
    [Serializable]
    public class JsonMessage
    {
        public string device = "";
        public int h = 0, m = 0, s = 0, mm = 0;
        public float px = 0.0f, py = 0.0f, pz = 0.0f;
        public float rx = 0.0f, ry = 0.0f, rz = 0.0f, rw = 0.0f;
    }

    /// <summary>
    /// バッテリー状態データ
    /// </summary>
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
