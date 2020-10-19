using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityTools.Debuging;

namespace UnityARMiniGames
{

    public class UILog : MonoBehaviour, LogTool.ILogUser
    {
        protected Text text;
        protected int lineCount = 0;

        public void Log(string message)
        {
            if (this.text == null) this.text = this.GetComponent<Text>();
            if (this.lineCount++ > 50)
            {
                this.text.text = "";
                this.lineCount = 0;
            }
            this.text.text += message + "\n";
        }
    }
}