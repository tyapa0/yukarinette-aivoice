using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Yukarinette.Distribution.Plugin.Core
{
    /// <summary>
    /// 設定値管理クラス。
    /// </summary>
    public class ConfigManager
    {
        /// <summary>
        /// 設定データ。
        /// </summary>
        public ConfigData Data { get; protected set; }

        /// <summary>
        /// 設定値ファイルパス。
        /// </summary>
        private String SettingPath { get; set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ConfigManager(String filename)
        {
            YukarinetteLogger.Instance.Debug("start.");

            this.Data = null;

            String settingDir = Path.Combine(YukarinetteCommon.AppSettingFolder, "plugins");
            this.SettingPath = Path.Combine(settingDir, filename);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 設定値を読み込む。
        /// </summary>
        public void Load(String pluginName, String[] voiceroidPath)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath + ", pluginName=" + pluginName);

            // ファイルがない場合
            if (!File.Exists(this.SettingPath))
            {
                YukarinetteLogger.Instance.Info("setting file not found. SettingPath=" + this.SettingPath);
                this.CreateNewSetting(voiceroidPath);
                YukarinetteLogger.Instance.Debug("end.");
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(this.SettingPath, FileMode.Open))
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(ConfigData));
                    this.Data = (ConfigData)xml.Deserialize(sr);
                }

                YukarinetteLogger.Instance.Info("setting load ok. SettingPath=" + this.SettingPath);
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex);
                this.CreateNewSetting(voiceroidPath);
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " の設定ファイルが読み取れませんでした。初期値で動作します。");
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 設定値を保存する。
        /// </summary>
        public void Save(String pluginName)
        {
            YukarinetteLogger.Instance.Debug("start. pluginName=" + pluginName);

            if (null == this.Data)
            {
                YukarinetteLogger.Instance.Info("save data is null.");
                YukarinetteLogger.Instance.Debug("end.");
                return;
            }

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(this.SettingPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this.SettingPath));
                }

                using (FileStream fs = new FileStream(this.SettingPath, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(ConfigData));
                    xml.Serialize(sw, this.Data);
                }

                YukarinetteLogger.Instance.Info("setting save ok. SettingPath=" + this.SettingPath);
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex);
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " の設定ファイルの保存に失敗しました。");
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 初期値を生成する。
        /// </summary>
        public void CreateNewSetting(String[] voiceroidPath)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath);

            this.Data = new ConfigData();
            this.SearchVoiceroid(voiceroidPath);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// VOICEROID パスが設定されていない場合、自動的にパスを検索し設定する。
        /// </summary>
        public void SearchVoiceroid(String[] voiceroidPath)
        {
            YukarinetteLogger.Instance.Debug("start. voiceroidPath=" + voiceroidPath);

            if (String.IsNullOrEmpty(this.Data.VoiceroidPath))
            {
                foreach (String path in voiceroidPath)
                {
                    if (File.Exists(path))
                    {
                        YukarinetteLogger.Instance.Info("voiceroid discovery. path=" + path);
                        this.Data.VoiceroidPath = path;
                        break;
                    }
                }
            }
            else
            {
                YukarinetteLogger.Instance.Info("voiceroid path setting ok.");
            }

            YukarinetteLogger.Instance.Debug("end.");
        }
    }
}