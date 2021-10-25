using System;
using System.Collections.Generic;
using System.IO;
using Yukarinette.Distribution.Plugin.Core;

namespace Yukarinette.Distribution.Plugin
{
    /// <summary>
    /// A.I.VOICEプラグインやっつけ
    /// </summary>
    public class YukariPlugin : IYukarinetteInterface
    {
        /// <summary>
        /// プラグイン設定管理クラス。
        /// </summary>
        private ConfigManager mConfigManager;

        /// <summary>
        /// VOICEROID管理クラス。
        /// </summary>
        private VoiceroidManager mVoiceroidManager;

        /// <summary>
        /// プラグイン名。
        /// </summary>
        public override string Name
        {
            get
            {
                return "A.I.VOICE";
            }
        }

        /// <summary>
        /// プラグインGUID
        /// </summary>
        public override string GUID
        {
            get { return "82A62F8C-9CBD-4916-9110-419E47DBE650"; }
        }

        /// <summary>
        /// VOICEROIDパス。
        /// </summary>
        private String[] VoiceroidPath
        {
            get
            {
                List<String> ret = new List<String>();
                ret.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"AHS\VOICEROID+\YukariEX\VOICEROID.exe"));
                ret.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"AHS\VOICEROID+\yukari\VOICEROID.exe"));
                return ret.ToArray();
            }
        }

        /// <summary>
        /// アプリケーション起動時に呼ばれる。
        /// </summary>
        public override void Loaded()
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mConfigManager = new ConfigManager("DistYukariPlugin.dll.config");
            this.mConfigManager.Load(this.Name, this.VoiceroidPath);

            this.mVoiceroidManager = new YukariManager(this.Name);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// アプリケーション終了時に呼ばれる。
        /// </summary>
        public override void Closed()
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mConfigManager.Save(this.Name);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 設定で呼ばれる。
        /// </summary>
        public override void Setting()
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mConfigManager.SearchVoiceroid(this.VoiceroidPath);
            OptionWindow.Show(this.mConfigManager, this.Name, this.Name);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 音声認識開始時に呼ばれる。
        /// </summary>
        public override void SpeechRecognitionStart()
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mConfigManager.SearchVoiceroid(this.VoiceroidPath);
            this.mVoiceroidManager.ObsOutTextPath = this.mConfigManager.Data.ObsOutTextPath;
            this.mVoiceroidManager.ObsOutTxt = this.mConfigManager.Data.ObsOutTxt;
            this.mVoiceroidManager.Create(this.mConfigManager.Data.VoiceroidPath);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 音声認識終了時に呼ばれる。
        /// </summary>
        public override void SpeechRecognitionStop()
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mVoiceroidManager.Dispose(this.mConfigManager.Data.AutoExit);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 音声認識結果を受け取る際に呼ばれる。
        /// </summary>
        /// <param name="text"></param>
        public override void Speech(string text)
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.mVoiceroidManager.Speech(text);

            YukarinetteLogger.Instance.Debug("end.");
        }
    }
}
