using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using RemoteTranceObject;
using RM.Friendly.WPFStandardControls; //ソリューション→参照からnugetパッケージのインストールでライブラリを追加

//"A.I.VOICE Editor";
namespace AIVoiceControl
{
    public partial class Form1 : Form
    {
        private ClassFileInfo m_msg;
        WindowControl mainWindow;

        private List<string> lText;
        private bool threadloop;
        private bool ObsTextOut;
        private string ObsTextOutPath;
        private Thread myTread;

        public Form1()
        {
            InitializeComponent();
            mainWindow = null;
            ObsTextOut = false;
            ObsTextOutPath = "";

            lText = new List<string>();
            threadloop = true;
            Thread myTread = new Thread(new ThreadStart(ThreadProc));
            myTread.Start();
        }
        ~Form1()
        {
            threadloop = false;
            if (myTread != null)
            {
                myTread.Abort();
                myTread = null;
            }
        }


        protected void GetComponent()
        {
            try
            {
                // A.I.VOICEの制御を取得する
                System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("AIVoiceEditor");
                var app = new WindowsAppFriend(ps[0]);
                mainWindow = WindowControl.FromZTop(app);
                Thread.Sleep(200); // 最初の発生時音がぶつるのでウエイトを入れる
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("AIVoiceEditorが起動していません\n");
                throw ex;
            }
        }
        protected void SpeechControl(String text)
        {
            try
            {
                // A.I.VOICEを発音する
                WPFTextBox txtMessage = new WPFTextBox(mainWindow.IdentifyFromLogicalTreeIndex(0, 4, 3, 6, 3, 0, 0, 0, 1, 2));
                txtMessage.EmulateChangeText(text);

                WPFButtonBase btnPlay = new WPFButtonBase(mainWindow.IdentifyFromLogicalTreeIndex(0, 4, 3, 6, 3, 0, 0, 0, 1, 3, 0));
                btnPlay.EmulateClick();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Friendlyの初期化に失敗しました\n");
                throw ex;
            }
        }
        protected bool IsSpeech()
        {
            WPFButtonBase btnPlay = new WPFButtonBase(mainWindow.IdentifyFromLogicalTreeIndex(0, 4, 3, 6, 3, 0, 0, 0, 1, 3, 0));
            var d = btnPlay.LogicalTree();
            System.Windows.Visibility v = (System.Windows.Visibility)(d[2])["Visibility"]().Core; // [再生]の画像の表示状態
            if (v == System.Windows.Visibility.Visible)
            {
                return true;
            }
            return false;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // IPC サーバーを作成
                IpcServerChannel servChannel = new IpcServerChannel("aivoiceyatuuke");

                // リモートオブジェクトを登録
                ChannelServices.RegisterChannel(servChannel, true);

                // イベントを登録
                m_msg = new ClassFileInfo();

                m_msg.OnTrance += new ClassFileInfo.CallEventHandler(m_msg_OnTrance);
                RemotingServices.Marshal(m_msg, "speak", typeof(ClassFileInfo));
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("IPC サーバーの作成に失敗しました\n");
                // ここで例外を投げるとプラグインチェック連打時にウインドウが出るので投げないようにする
            }
        }

        //クライアントから転送されてきた情報を処理する
        void m_msg_OnTrance(ClassFileInfo.ClassFileInfoEventArg e)
        {

            switch (e.Command)
            {
                case 1: //発話
                    if (mainWindow == null)
                    {
                        GetComponent();
                    }
                    // とりあえずバッファにためる
                    lText.Add(e.Text);
                    break;

                case 2: //OBS保存先
                    if (e.Text != "")
                    {
                        ObsTextOut = true;
                        ObsTextOutPath = e.Text;
                    } else {
                        ObsTextOut = false;
                        ObsTextOutPath = "";
                    }
                    break;

                default:
                    break;
            }
        }

        //OBS用のテキストファイル出力
        private void ObsOutText(string text)
        {
            try
            {
                File.WriteAllText(ObsTextOutPath, text, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // 発話用スレッド
        private void ThreadProc()
        {
            while (threadloop)
            {
                if (lText.Count != 0)
                {
                    if (IsSpeech())
                    {
                        if (ObsTextOut)
                        {
                            ObsOutText(lText[0]);
                        }
                        SpeechControl(lText[0]);
                        lText.RemoveAt(0);
                    }
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        //終了時処理
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            threadloop = false;
            if (myTread != null)
            {
                myTread.Abort();
                myTread = null;
            }
        }
    }
}
