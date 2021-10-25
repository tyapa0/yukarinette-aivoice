using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using System.Windows.Automation;
using RemoteTranceObject;
using Yukarinette.Distribution.Plugin.Core;

namespace Yukarinette.Distribution.Plugin
{
    /// <summary>
    /// A.I.VOICEプラグインやっつけ
    /// </summary>
    public class YukariManager : VoiceroidManager
    {
        private ClassFileInfo m_msg = null; // IPCオブジェクト
        protected Process AivoiceControlProcess;

        private IntPtr mEdithWnd;
        private IntPtr mSpeakhWnd;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public YukariManager(String voiceroidName)
            : base(voiceroidName)
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.ClearMember();

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 破棄するときに呼ばれる
        /// </summary>
        public override void Dispose(Boolean autoexit)
        {
            ////////////////////////////////////////
            // IPCサーバーを終了させる
            if (null != this.AivoiceControlProcess)
            {
                try
                {
                    this.AivoiceControlProcess.Kill();
                }
                catch (Exception ex)
                {
                    YukarinetteLogger.Instance.Error(ex);
                }
                this.AivoiceControlProcess.Dispose();
                this.AivoiceControlProcess = null;
            }
            ////////////////////////////////////////

            base.Dispose(autoexit);

            YukarinetteLogger.Instance.Debug("start.");

            this.ClearMember();

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// メンバ変数を初期化する。
        /// </summary>
        private void ClearMember()
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mEdithWnd = IntPtr.Zero;
            this.mSpeakhWnd = IntPtr.Zero;

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 認識するための部品を指定する。
        /// </summary>
        /// <param name="form"></param>
        protected override void GetComponent(AutomationElement form)
        {
            YukarinetteLogger.Instance.Debug("start.");

            try
            {
                ////////////////////////////////////////
                //IPCサーバーのexeを起動する
                System.Reflection.Assembly executionAsm = System.Reflection.Assembly.GetExecutingAssembly();
                string CtrlPath = System.IO.Path.GetDirectoryName(executionAsm.Location);
                CtrlPath += "\\AivoiceControl.exe";
                ProcessStartInfo pInfo = new ProcessStartInfo()
                {
                    FileName = CtrlPath,
                    WorkingDirectory = Path.GetDirectoryName(CtrlPath),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };
                AivoiceControlProcess = Process.Start(pInfo);
                Thread.Sleep(500);

                if (m_msg == null)
                {
                    //IPCクライアントを起動する
                    IpcClientChannel clientChannel = new IpcClientChannel();
                    // リモートオブジェクトを登録
                    ChannelServices.RegisterChannel(clientChannel, true);
                    // オブジェクトを作成
                    m_msg = (ClassFileInfo)Activator.GetObject(typeof(ClassFileInfo), "ipc://aivoiceyatuuke/speak");
                }

                // OBS用のテキスト出力先を通知する
                if (ObsOutTxt == true)
                {
                    m_msg.DataTrance(2, ObsOutTextPath); //IPC通信で転送する
                }
                else
                {
                    m_msg.DataTrance(2, ""); //IPC通信で転送する\
                }

                ////////////////////////////////////////
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex);
                this.ClearMember();
                Dispose(false);

                throw ex;
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 実際に喋らす際の制御を行う。
        /// </summary>
        /// <param name="text"></param>
        protected override void SpeechControl(String text)
        {
            YukarinetteLogger.Instance.Debug("start. text=" + text);

            try
            {
                ////////////////////////////////////////
                m_msg.DataTrance(1, text); //IPC通信で転送する
                ////////////////////////////////////////
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex);
                YukarinetteLogger.Instance.Info("voiceroid process is missing.");

                this.ClearMember();

                throw ex;
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 読み上げ可能か否かを返す。
        /// </summary>
        /// <returns></returns>
        protected override bool IsCanSpeech()
        {
            YukarinetteLogger.Instance.Debug("start.");
            YukarinetteLogger.Instance.Debug("end. return=true");
            return true;
        }


    }
}
