using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Networking;

namespace UnityARMiniGames
{
    public class ARSharing : MonoBehaviour, ARLauncher.ILauncherUser, ARNetworkController.INetworkUser, ARWorld.IARWorldUser
    {
        public Environment Runtime { get; set; }

        public int Order => (int)ARLauncher.LauncherOrder.Default;

        public interface ISharedMarker
        {
            Transform transform { get; set; }
        }

        [System.Serializable]
        public class Data
        {
            public PCInfo server;

            public Transform serverCamera;
        }


        public Data CurrentData { get; set; }
        public bool IsServer { get; set; }
        public ARWorld World { get; set; }

        public class SharingSocket : UDPSocket<Data>
        {
            protected ARSharing sharing;
            public SharingSocket(ARSharing sharing) : base()
            {
                this.sharing = sharing;
            }

            public override void OnMessage(SocketData socket, Data data)
            {
                this.sharing.CurrentData = data;
            }

            public override byte[] OnSerialize(Data data)
            {
                var str = JsonUtility.ToJson(data);
                var bytes = Serialization.ObjectToByteArray(str);
                return CompressTool.Compress(bytes);
            }

            public override Data OnDeserialize(byte[] data, int length)
            {
                var str = Serialization.ByteArrayToObject<string>(CompressTool.Decompress(data));
                return JsonUtility.FromJson<Data>(str);
            }
        }

        protected UDPSocket<Data> sender;
        protected short port = 0;
        protected UDPSocket<Data> discover;

        protected PCInfo current;
        public void OnLaunchEvent(ARLauncher.Data data, Launcher<ARLauncher.Data>.LaunchEvent levent)
        {
            switch (levent)
            {
                case ARLauncher.LaunchEvent.Init:
                    {
                        this.StartCoroutine(this.Discover(data.sharingConfigure.Data));
                    }
                    break;
                default:
                    break;
            }
        }

        protected IEnumerator Discover(ARSharingConfigure.ARData data)
        {
            this.CurrentData = null;
            if (this.discover != null) this.discover.Dispose();
            this.discover = new UDPSocket<Data>();
            this.discover.StartReceive(data.scanPort.port);
            yield return new WaitForSeconds(3);

            this.StartNetworkSharing(data);
        }

        protected void StartNetworkSharing(ARSharingConfigure.ARData data)
        {
            if (this.discover != null) this.discover.Dispose();
            if (this.CurrentData == null)
            {
                //if server not find, then start as new server
                this.CurrentData = new Data();
                this.CurrentData.server = this.current;
                this.sender = new SharingSocket(this);
                this.port = data.scanPort.port;
                this.IsServer = true;

                LogTool.Log("Start as server", LogLevel.Dev, LogChannel.Network);

            }
            else
            {
                //otherwise start receiving data from server
                if (this.sender != null) this.sender.Dispose();
                this.sender = new SharingSocket(this);
                this.sender.StartReceive(data.scanPort.port);
                this.IsServer = false;
                LogTool.Log("Start as client", LogLevel.Dev, LogChannel.Network);
            }

        }

        public void OnInit(NetworkController.NetworkData networkData)
        {
            this.current = networkData.current;
        }


        protected void Update()
        {
            if (this.IsServer)
            {
                this.CurrentData.serverCamera = this.World.ARCamera.transform;
                this.sender.Broadcast(this.CurrentData, this.port);
            }
        }
    }
}