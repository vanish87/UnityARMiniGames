using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

            public float3 position;
            public Quaternion rotation;
            public float3 scale;
        }


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
                this.sharing.currentData = data;
                this.sharing.isServer = false;
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
        [SerializeField] protected Data currentData;

        protected UDPSocket<Data> sender;
        protected bool isServer = true;
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
            if (this.sender != null) this.sender.Dispose();
            this.sender = new SharingSocket(this);
            this.sender.StartReceive(data.scanPort.port);
            yield return new WaitForSeconds(3);

            if (this.isServer)
            {
                this.currentData.server = this.current;
                this.sender.Dispose();
                this.sender = new SharingSocket(this);
                LogTool.Log("Start as server", LogLevel.Dev, LogChannel.Network);

                while (true)
                {
                    this.currentData.position = this.World.ARCamera.transform.position;
                    this.currentData.rotation = this.World.ARCamera.transform.rotation;
                    this.currentData.scale = this.World.ARCamera.transform.localScale;
                    this.sender.Broadcast(this.currentData, data.scanPort.port);
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                //otherwise start receiving data from server
                LogTool.Log("Start as client", LogLevel.Dev, LogChannel.Network);
            }
        }

        public void OnInit(NetworkController.NetworkData networkData)
        {
            this.current = networkData.current;
        }

    }
}