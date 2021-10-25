using System;

namespace Yukarinette.Distribution.Plugin.Core
{
    /// <summary>
    /// 設定データ。
    /// </summary>
    public class ConfigData
    {
        /// <summary>
        /// VOICEROID 実行パス。
        /// </summary>
        public String VoiceroidPath { get; set; }
        public String ObsOutTextPath { get; set; }

        /// <summary>
        /// VOICEROID自動終了フラグ。
        /// </summary>
        public Boolean AutoExit { get; set; }
        public Boolean ObsOutTxt { get; set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ConfigData()
        {
            this.VoiceroidPath = "";
            this.ObsOutTextPath = "";
            this.AutoExit = false;
            this.ObsOutTxt = false;
        }
    }
}
