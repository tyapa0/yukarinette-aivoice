using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace Yukarinette.Distribution.Plugin.Core
{
    /// <summary>
    /// VOICEROID管理クラス。
    /// </summary>
    public abstract class VoiceroidManager
    {
        /// <summary>
        /// VOICEROIDプロセス。
        /// </summary>
        protected Process mVoiceroidProcess;

        private Boolean mMonitoring;
        private Task mMonitoringTask;
        private String mVoiceroidName;
        public String ObsOutTextPath { get; set; }
        public Boolean ObsOutTxt { get; set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public VoiceroidManager(String voiceroidName)
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mVoiceroidProcess = null;

            this.mMonitoring = false;
            this.mMonitoringTask = null;

            this.mVoiceroidName = voiceroidName;

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// VOICEROIDプロセスの操作を得る。
        /// </summary>
        public void Create(String voiceroidPath)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath + ", voiceroidName=" + this.mVoiceroidName);

            if (String.IsNullOrEmpty(voiceroidPath))
            {
                YukarinetteLogger.Instance.Error(this.mVoiceroidName + " exe path is not setting.");
                throw new YukarinetteException(this.mVoiceroidName + " の設定が行われていません。プラグイン設定を確認してください。");
            }
            if (!File.Exists(voiceroidPath))
            {
                YukarinetteLogger.Instance.Error(this.mVoiceroidName + " exe path is noting.");
                throw new YukarinetteException(this.mVoiceroidName + " の実行ファイルが見つかりません。プラグイン設定を確認してください。");
            }

            try
            {
                AutomationElement vform = this.getVoiceroidApp(voiceroidPath);
                if (null == vform)
                {
                    YukarinetteLogger.Instance.Info("voiceroid process not found.");
                    vform = this.startVoiceroidApp(voiceroidPath);
                }
                this.GetComponent(vform);

                YukarinetteLogger.Instance.Info(this.mVoiceroidName + " manager create.");

                this.MonitoringVoiceroid(voiceroidPath);
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex);
                throw new YukarinetteException(this.mVoiceroidName + " の認識に失敗しました。" + ex.Message);
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// VOICEROIDアプリが動作していたら取得する。
        /// </summary>
        /// <returns></returns>
        protected virtual AutomationElement getVoiceroidApp(String voiceroidPath, Boolean iswait = true)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath);

            foreach (Process ps in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(voiceroidPath)))
            {
                if (voiceroidPath == ps.MainModule.FileName)
                {
                    YukarinetteLogger.Instance.Info("voiceroid process discovery.");

                    if (iswait)
                    {
                        Stopwatch swatch = new Stopwatch();
                        swatch.Start();
                        while (IntPtr.Zero == ps.MainWindowHandle)
                        {
                            Thread.Sleep(250);

                            if ((15000) <= swatch.ElapsedMilliseconds)
                            {
                                YukarinetteLogger.Instance.Error("voiceroid process wait error.");
                                swatch.Stop();
                                throw new TimeoutException("Voiceroid process has been waiting 15 seconds, start-up was not completed.");
                            }
                        }
                        swatch.Stop();
                    }
                    else
                    {
                        if (IntPtr.Zero == ps.MainWindowHandle)
                        {
                            YukarinetteLogger.Instance.Debug("end. return=null");
                            return null;
                        }
                    }

                    YukarinetteLogger.Instance.Info("voiceroid window recognition.");
                    AutomationElement vform = AutomationElement.FromHandle(ps.MainWindowHandle);

                    WindowPattern vformPattern = (WindowPattern)vform.GetCurrentPattern(WindowPattern.Pattern);
                    vformPattern.SetWindowVisualState(WindowVisualState.Normal);

                    YukarinetteLogger.Instance.Debug("end. return=" + ps.ProcessName);
                    return vform;
                }
            }

            YukarinetteLogger.Instance.Debug("end. return=null");
            return null;
        }

        /// <summary>
        /// VOICEROIDアプリを起動する。
        /// </summary>
        /// <returns></returns>
        protected virtual AutomationElement startVoiceroidApp(String voiceroidPath)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath);

            ProcessStartInfo pInfo = new ProcessStartInfo()
            {
                FileName = voiceroidPath,
                WorkingDirectory = Path.GetDirectoryName(voiceroidPath),
                CreateNoWindow = true,
                UseShellExecute = false,
            };
            this.mVoiceroidProcess = Process.Start(pInfo);

            YukarinetteLogger.Instance.Info("voiceroid execute.");

            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            while (IntPtr.Zero == this.mVoiceroidProcess.MainWindowHandle)
            {
                Thread.Sleep(250);

                if ((15000) <= swatch.ElapsedMilliseconds)
                {
                    YukarinetteLogger.Instance.Error("voiceroid process wait error.");
                    swatch.Stop();
                    throw new TimeoutException("Voiceroid process has been waiting 15 seconds, start-up was not completed.");
                }
            }
            swatch.Stop();

            YukarinetteLogger.Instance.Info("voiceroid window recognition.");
            AutomationElement vform = AutomationElement.FromHandle(this.mVoiceroidProcess.MainWindowHandle);

            YukarinetteLogger.Instance.Debug("end. return=" + this.mVoiceroidProcess.MainWindowHandle);
            return vform;
        }

        /// <summary>
        /// VOICEROIDをモニタリングする。
        /// </summary>
        private void MonitoringVoiceroid(String voiceroidPath)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath);

            this.mMonitoring = true;
            this.mMonitoringTask = Task.Run(() => 
            {
                YukarinetteLogger.Instance.Info("voiceroid process monitoring start.");
                while (this.mMonitoring)
                {
                    try
                    {
                        Thread.Sleep(250);

                        if (this.IsCanSpeech()) continue;

                        AutomationElement vform = this.getVoiceroidApp(voiceroidPath, false);
                        if (null == vform) continue;
                        this.GetComponent(vform);

                        YukarinetteLogger.Instance.Info("voiceroid window recognition.");
                    }
                    catch (Exception ex)
                    {
                        YukarinetteLogger.Instance.Error(ex);

                        if (null != this.mVoiceroidProcess)
                        {
                            this.mVoiceroidProcess.Dispose();
                            this.mVoiceroidProcess = null;
                        }
                    }
                }
                YukarinetteLogger.Instance.Info("voiceroid process monitoring stop.");
            });

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 破棄する
        /// </summary>
        public virtual void Dispose(Boolean autoexit)
        {
            YukarinetteLogger.Instance.Debug("start.");

            if (null != this.mMonitoringTask)
            {
                this.mMonitoring = false;
                this.mMonitoringTask.Wait();
                this.mMonitoringTask = null;
            }

            if (null != this.mVoiceroidProcess)
            {
                try
                {
                    if (autoexit)
                    {
                        this.mVoiceroidProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    YukarinetteLogger.Instance.Error(ex);
                }
                this.mVoiceroidProcess.Dispose();
                this.mVoiceroidProcess = null;
            }

            YukarinetteLogger.Instance.Info("dispose ok.");

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 読み上げを実施する。
        /// </summary>
        /// <param name="text"></param>
        public void Speech(String text)
        {
            YukarinetteLogger.Instance.Debug("start. text=" + text);

            try
            {
                if (!this.IsCanSpeech())
                {
                    YukarinetteLogger.Instance.Info("speech skip.");
                    return;
                }

                YukarinetteLogger.Instance.Info("speech start. text=" + text);
                this.SpeechControl(text);
                YukarinetteLogger.Instance.Info("speech end.");
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex);
                YukarinetteLogger.Instance.Info("voiceroid process is missing.");

                if (null != this.mVoiceroidProcess)
                {
                    this.mVoiceroidProcess.Dispose();
                    this.mVoiceroidProcess = null;
                }
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 制御に必要なコンポーネントを取得する。
        /// </summary>
        /// <param name="form"></param>
        protected abstract void GetComponent(AutomationElement form);

        /// <summary>
        /// 読み上げ制御を行う
        /// </summary>
        /// <param name="text"></param>
        protected abstract void SpeechControl(String text);

        /// <summary>
        /// 読み上げ可能か否かを返す
        /// </summary>
        /// <returns></returns>
        protected abstract Boolean IsCanSpeech();
    }
}
