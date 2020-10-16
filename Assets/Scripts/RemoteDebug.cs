using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Networking;

namespace UnityARMiniGames
{
    public class RemoteDebug : MonoBehaviour, ARNetworkController.INetworkUser
    {
        public interface IRemoteDebugUser
        {
            void OnBind(RemoteDebug debug);
        }
        protected bool isDevice = false;
        protected SocketData devPC; 
        protected MessageDataSocket debugSocket;
        public void OnInit(NetworkController.NetworkData networkData)
        {
            if(this.debugSocket != null) this.debugSocket.Dispose();
            this.debugSocket = new MessageDataSocket();
            var port = networkData.devPC.ports.Find(p => p.name == "RemoteDebug");
            this.isDevice = networkData.current.ipAddress != networkData.devPC.ipAddress;
            if(this.isDevice)
            {
                //this is ar device,so send data to dev pc
                this.devPC = SocketData.Make(networkData.devPC.ipAddress, port.port);
                LogTool.Log("Start send remote debug info to " + networkData.devPC.ipAddress + " " + port.port, LogLevel.Dev, LogChannel.Network);
            }
            else
            {
                this.debugSocket.StartReceive(port.port);
                LogTool.Log("Start receive remote debug info on prot " + port.port, LogLevel.Dev, LogChannel.Network);
            }

            foreach(var user in ObjectTool.FindAllObject<IRemoteDebugUser>()) user.OnBind(this);
        }
        public void Bind(MessageDataSocket.IMessageData data)
        {
            this.debugSocket.Bind(data);
        }
        protected void Update()
        {
            if(this.isDevice)
            {
                this.debugSocket.SendData(this.devPC);
            }
        }
    }
}