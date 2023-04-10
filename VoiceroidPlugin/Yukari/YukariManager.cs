using System;
using System.Diagnostics;
using AI.Talk.Editor.Api;
using Yukarinette.Distribution.Plugin.Core;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;

namespace Yukarinette.Distribution.Plugin
{
    /// <summary>
    /// A.I.VOICEプラグインやっつけ
    /// </summary>
    public class YukariManager : VoiceroidManager
    {
        protected Process AivoiceControlProcess;

        private IntPtr mEdithWnd;
        private IntPtr mSpeakhWnd;

        private TtsControl _ttsControl;     // TTS APIの呼び出し用オブジェクト
        string  CurrentHost;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public YukariManager(String voiceroidName)
            : base(voiceroidName)
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.ClearMember();

            _ttsControl = new TtsControl();

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 破棄するときに呼ばれる
        /// </summary>
        public override void Dispose(Boolean autoexit)
        {
            // ホストプログラムとの接続を解除する
            this.Disconnect();

            if (autoexit)
            {
                // ホストプログラムを終了する
                Task.Factory.StartNew(() =>
                {
                    _ttsControl.TerminateHost();
                });
            }

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
        protected override void GetComponent()
        {
            YukarinetteLogger.Instance.Debug("start.");

            try
            {
                // 接続可能なホストの一覧を取得する
                string[] AvailableHosts = _ttsControl.GetAvailableHostNames();

                if (AvailableHosts.Length > 0)
                {
                    // 最初に見つかるAIvoiceホストを登録する
                    CurrentHost = AvailableHosts[0];
                }

                // APIを初期化する
                _ttsControl.Initialize(CurrentHost);

                // ホストと接続する
                this.Startup();
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

            if (_ttsControl.Status == HostStatus.NotConnected)
            {
                // ホストプログラムに接続する
                _ttsControl.Connect();
            }

            // Busy中は最大5秒まで待つ
            int tickStart = System.Environment.TickCount & int.MaxValue;
            while (_ttsControl.Status == HostStatus.Busy)
            {
                if (_ttsControl.Status == HostStatus.Idle)
                {
                    break;
                }
                int tickCount = System.Environment.TickCount & int.MaxValue;
                if (tickCount > (tickStart + 5000)) {
                    break;
                }
                Thread.Sleep(1);
            }

            try
            {
                // OBS用のテキスト出力先を通知する
                if (ObsOutTxt == true)
                {
                    ObsOutText(text);
                }
                //テキスト設定
                _ttsControl.Text = text;

                //スピーチ
                _ttsControl.Play();
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

        //OBSテキストへ出力する
        private void ObsOutText(string text)
        {
            try
            {
                if (ObsOutTextPath != "")
                {
                    File.WriteAllText(ObsOutTextPath, text, System.Text.Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region ホストへの接続・切断

        /// <summary>
        /// ホストへの接続を開始します。
        /// </summary>
        private void Startup()
        {
            try
            {
                if (_ttsControl.Status == HostStatus.NotRunning)
                {
                    // ホストプログラムを起動する
                    _ttsControl.StartHost();
                }

                // ホストプログラムに接続する
                _ttsControl.Connect();

                string Version = "ホストバージョン: " + _ttsControl.Version;

                YukarinetteLogger.Instance.Debug("ホストへの接続を開始しました。");
            }
            catch (Exception ex)
            {
                throw new YukarinetteException(this.CurrentHost + " の認識に失敗しました。" + Environment.NewLine + ex.Message);
            }
        }

        /// <summary>
        /// ホストへの接続を終了します。
        /// </summary>
        private void Disconnect()
        {
            try
            {
                // ホストプログラムとの接続を解除する
                _ttsControl.Disconnect();

                YukarinetteLogger.Instance.Debug("ホストへの接続を終了しました。");
            }
            catch (Exception ex)
            {
                throw new YukarinetteException(this.CurrentHost + " の終了に失敗しました。" + Environment.NewLine + ex.Message);
            }
        }

        #endregion // ホストへの接続・切断
    }
}
