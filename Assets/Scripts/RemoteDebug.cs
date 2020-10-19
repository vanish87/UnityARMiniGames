using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        protected List<SocketData> devPCs = new List<SocketData>();
        protected MessageDataSocket debugSocket;
        public void OnInit(NetworkController.NetworkData networkData)
        {
            if(this.debugSocket != null) this.debugSocket.Dispose();
            this.debugSocket = new MessageDataSocket();
            var remoteDevs = networkData.devPCs.Where(p => p.ports.Find(port=>port.name == "RemoteDebug") != null);
            this.isDevice = networkData.devPCs.Select(p => p.ipAddress).Contains(networkData.current.ipAddress) == false;
            if(this.isDevice)
            {
                //this is ar device,so send data to dev pc
                foreach(var dev in remoteDevs)
                {
                    var socket = SocketData.Make(dev.ipAddress, dev.ports.Find(p=>p.name == "RemoteDebug").port);
                    if(Tool.IsReachable(socket.endPoint))
                    {
                        this.devPCs.Add(socket);
                        LogTool.Log("Start send remote debug info to " + socket.endPoint.ToString(), LogLevel.Dev, LogChannel.Network);
                    }
                }
            }
            else
            {
                var port = remoteDevs.First().ports.First().port;
                this.debugSocket.StartReceive(port);
                LogTool.Log("Start receive remote debug info on prot " + port, LogLevel.Dev, LogChannel.Network);
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
                foreach(var remote in this.devPCs)
                {
                    this.debugSocket.SendData(remote);
                }
            }
        }
    }
}