using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace Yukarinette.Distribution.Plugin.Core
{
    /// <summary>
    /// OptionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OptionWindow : Window
    {
        /// <summary>
        /// オプションウィンドウを表示する。
        /// </summary>
        public static void Show(ConfigManager manager, String pluginName, String voiceroidName)
        {
            YukarinetteLogger.Instance.Debug("start. pluginName=" + pluginName + ", voiceroidName=" + voiceroidName);

            OptionWindow optWin = new OptionWindow(manager);
            optWin.Title = "設定 - " + pluginName;
            optWin.VoiceroidGroupBox.Header = voiceroidName;
            optWin.VoiceroidTextBlock.Text = voiceroidName + " " + optWin.VoiceroidTextBlock.Text;

            YukarinetteLogger.Instance.Info("dialog open.");
            if (optWin.ShowDialog().Value)
            {
                YukarinetteLogger.Instance.Info("dialog ok.");
                optWin.Save(manager, pluginName);
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        private OptionWindow(ConfigManager manager)
        {
            YukarinetteLogger.Instance.Debug("start.");

            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            this.VoiceroidPathTextBox.Text = manager.Data.VoiceroidPath;
            this.VoiceroidAutoExit.IsChecked = manager.Data.AutoExit;
            this.ObsOutoputTextPath.Text = manager.Data.ObsOutTextPath;
            this.ObsOutoputText.IsChecked = manager.Data.ObsOutTxt;
            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// ダイアログに設定された値を保存する。
        /// </summary>
        private void Save(ConfigManager manager, String pluginName)
        {
            YukarinetteLogger.Instance.Debug("start. pluginName=" + pluginName);

            manager.Data.VoiceroidPath = this.VoiceroidPathTextBox.Text;
            manager.Data.AutoExit = this.VoiceroidAutoExit.IsChecked.Value;
            manager.Data.ObsOutTextPath = this.ObsOutoputTextPath.Text;
            manager.Data.ObsOutTxt = this.ObsOutoputText.IsChecked.Value;
            manager.Save(pluginName);

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// 参照ボタン。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VoiceroidPathButton_Click(object sender, RoutedEventArgs e)
        {
            YukarinetteLogger.Instance.Debug("start.");

            // 既に入力がある場合の初期表示ディレクトリの調整
            String initDir = System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (0 < this.VoiceroidPathTextBox.Text.Length)
            {
                if (File.Exists(this.VoiceroidPathTextBox.Text))
                {
                    initDir = Path.GetDirectoryName(this.VoiceroidPathTextBox.Text);
                }
            }

            OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = "AIVoiceEditor.exe",
                InitialDirectory = initDir,
                Filter = "AIVoiceEditor (*.exe)|*.exe",
                Title = "AIVoiceEditor の実行ファイルを指定してください。",
            };

            YukarinetteLogger.Instance.Info("dialog open.");
            if (ofd.ShowDialog().Value)
            {
                YukarinetteLogger.Instance.Info("dialog ok.");
                this.VoiceroidPathTextBox.Text = ofd.FileName;
            }

            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// OBS保存先ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObsOutoputTextPathButton_Click(object sender, RoutedEventArgs e)
        {
            YukarinetteLogger.Instance.Debug("start.");

            // 既に入力がある場合の初期表示ディレクトリの調整
            String initDir = System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (0 < this.ObsOutoputTextPath.Text.Length)
            {
                initDir = Path.GetDirectoryName(this.ObsOutoputTextPath.Text);
            }

            OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = "OutAIVOICE.txt",
                InitialDirectory = initDir,
                Filter = "Folder|.",
                CheckFileExists = false,
                Title = "OBS用の出力テキストフォルダを選択してください。",
            };

            YukarinetteLogger.Instance.Info("dialog open.");
            if (ofd.ShowDialog().Value)
            {
                YukarinetteLogger.Instance.Info("dialog ok.");
                this.ObsOutoputTextPath.Text = ofd.FileName;
                try
                {
                    File.WriteAllText(ofd.FileName, ""); // 空のファイルをクリエイトする
                }
                catch (Exception) { }
            }
            YukarinetteLogger.Instance.Debug("end.");
        }

        /// <summary>
        /// OKボタン。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }


    }
}
