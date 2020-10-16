using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Common.Example;

namespace UnityARMiniGames
{
    public class ARLauncher : Launcher<ARLauncher.Data>
    {
        [System.Serializable]
        public class Data
        {
            public PCConfigure pcConfigure;

            public ARSharingConfigure sharingConfigure;

        }

        protected override void ConfigureEnvironment()
        {
            base.ConfigureEnvironment();

            this.data.pcConfigure.Initialize();
            this.data.sharingConfigure.Initialize();
        }

    }
}
