# ゆかりねっと A.I.VOICE 連携プラグイン
A.I.VOICE を ゆかりねっと で制御するためのプラグインです。

# Features
* Voiceroid2よりもちょっとだけ応答が早いです。
* OBS用のテキストファイル出力に対応しています。  
![yukarisettei.png.](/image/yukarisettei.png "settei")  
* 画面に小さいウインドウが出るのは仕様です。（閉じないでください）  
![ctrlwin.png.](/image/ctrlwin.png "ctrlwin")  

# Installation
1. [Releaseページ](https://github.com/tyapa0/yukarinette-aivoice/releases/) から最新バージョンの `AivoiceControl_vX.X.zip` をダウンロードします
2. ダウンロードした zip ファイルのプロパティを開いて、ブロックされていたら解除します。
3. ダウンロードした zip ファイルを展開します。
4. 以下のファイルを ゆかりねっと の`Plugins`フォルダーにコピーします。
   - `AivoiceControl.exe`
   - `AivoiceControlPlugin.dll`  
   ※解凍ツールによってはセキュリティ許可がされていない場合があります。  
   ファイルを右クリック→プロパティで表示し、セキュリティを許可してください。  
   ![kyoka.png.](/image/kyoka.png "kyoka")

5. ゆかりねっと を起動したら 音声認識 の欄に「A.I.VOICE」が追加されているのでチェックを入れます。

以上

# Author
* [おかゆう](http://www.okayulu.moe/)氏作のVOICEROID EX/EX+制御プラグインを改造しています。
* [おかゆう](http://www.okayulu.moe/)氏作のVOICEROID EX/EX+制御プラグインを改造しています。
* NuGetパッケージの Friendly.2.6.1を使用しています。
* NuGetパッケージの ILMerge.3.0.41を使用しています。
* NuGetパッケージの MSBuild.ILMerge.Task.1.1.3を使用しています。
* 
# License
"yukarinette-aivoice" is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).
