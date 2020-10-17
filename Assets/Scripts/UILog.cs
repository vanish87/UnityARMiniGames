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

        public void Log(string message)
        {
            if (this.text == null) this.text = this.GetComponent<Text>();
            this.text.text += message + "\n";
        }
    }
}