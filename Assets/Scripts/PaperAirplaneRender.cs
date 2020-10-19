using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityARMiniGames
{
    public class PaperAirplaneRender : MonoBehaviour
    {
        protected Material material;
        protected void Start()
        {
            this.material = this.GetComponentInChildren<Material>();
            LogTool.AssertNotNull(this.material);
        }
    }
}