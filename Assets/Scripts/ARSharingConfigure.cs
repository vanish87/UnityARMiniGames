using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools;
using static UnityTools.Common.PCInfo;

namespace UnityARMiniGames
{
    public class ARSharingConfigure : Config<ARSharingConfigure.ARData>
    {

        [System.Serializable]
        public class ARData
        {
            public Port scanPort;

        }
        [SerializeField] protected string fileName = "ARSharingConfigure.xml";
        [SerializeField] protected ARData data;
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, this.fileName); } }

        public override ARData Data { get => this.data; set => this.data = value; }
    }
}