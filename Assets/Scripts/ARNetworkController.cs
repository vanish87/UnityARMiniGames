using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Networking;

namespace UnityARMiniGames
{
    public class ARNetworkController : NetworkController, ARLauncher.ILauncherUser
    {
        public Environment Runtime { get ; set ; }

        public int Order => (int)ARLauncher.LauncherOrder.Network;

        public void OnLaunchEvent(ARLauncher.Data data, Launcher<ARLauncher.Data>.LaunchEvent levent)
        {
            switch(levent)
            {
                case ARLauncher.LaunchEvent.Init:
                    {
                        var networkData = this.GetNetworkData(data.pcConfigure.Data.pcList);
                        this.NotifyUser(networkData);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
