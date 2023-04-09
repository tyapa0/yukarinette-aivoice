using System;

namespace RemoteTranceObject  //共有オブジェクト(サーバー/クライアントで全く同一内容で定義してください)
{
    public class ClassFileInfo
    {
        public class ClassFileInfoEventArg : EventArgs            //情報を引き渡すイベント引数クラス
        {
            private int m_command = 0;             //コマンド
            private string m_text = "";            //文字列
            public int Command { get { return m_command; } set { m_command = value; } }
            public string Text { get { return m_text; } set { m_text = value; } }
            public ClassFileInfoEventArg(int inCommand, string inText)
            {
                m_command = inCommand;
                m_text = inText;
            }
        }

        public delegate void CallEventHandler(ClassFileInfoEventArg e);
        public event CallEventHandler OnTrance;
        public void DataTrance(int inCommand, string inText)
        {
            if (OnTrance != null)
            {
                OnTrance(new ClassFileInfoEventArg(inCommand, inText));
            }
        }

        /// 自動的に切断されるのを回避する
        /// </summary>

    }
}