using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

            this.mMonitoringTask = null;

            this.mVoiceroidName = voiceroidName;

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// VOICEROIDプロセスの操作を得る。
        /// </summary>
        public void Create(String voiceroidPath)
        {
            this.GetComponent();

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
                this.mMonitoringTask.Wait();
                this.mMonitoringTask = null;
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
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 制御に必要なコンポーネントを取得する。
        /// </summary>
        /// <param name="form"></param>
        protected abstract void GetComponent();

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
