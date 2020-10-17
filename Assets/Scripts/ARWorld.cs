using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityTools;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Networking;

namespace UnityARMiniGames
{
    public class ARWorld : MonoBehaviour, RemoteDebug.IRemoteDebugUser, MessageDataSocket.IMessageData
    {
        public interface IARWorldUser
        {
            ARWorld World { get; set; }
        }

        [System.Serializable]
        public class SyncedData
        {
            public float3 position;
            public Quaternion rotation;
            public float3 scale;

        }


        public string HashString => this.ToString();

        public byte[] OnSerialize()
        {
            var cam = this.ARCamera.transform;
            var data = new SyncedData();
            data.position = cam.position;
            data.rotation = cam.rotation;
            data.scale = cam.localScale;
            var str = JsonUtility.ToJson(data);
            return CompressTool.Compress(Serialization.ObjectToByteArray(str));
        }

        public void OnDeserialize(byte[] data)
        {
            var raw = CompressTool.Decompress(data);
            var str = Serialization.ByteArrayToObject<string>(raw);
            var remote = JsonUtility.FromJson<SyncedData>(str);
            if(this.syncedData == null) this.syncedData = new SyncedData();
            lock (this.syncedData)
            {
                this.syncedData.position = remote.position;
                this.syncedData.rotation = remote.rotation;
                this.syncedData.scale = remote.scale;
            }
        }
        public Camera ARCamera => this.currentOrigin.camera;
        protected SyncedData syncedData;


        [SerializeField] protected ARSessionOrigin currentOrigin;
        [SerializeField] protected ARPlaneManager planeManager;
        [SerializeField] protected ARPlane playableFloor;

        protected void OnEnable()
        {
            this.currentOrigin = ObjectTool.FindAllObject<ARSessionOrigin>().FirstOrDefault();
            LogTool.AssertNotNull(this.currentOrigin);

            this.planeManager = this.currentOrigin.GetComponent<ARPlaneManager>();
            LogTool.AssertNotNull(this.planeManager);

            this.planeManager.planesChanged += this.OnPlaneChanged;

            foreach (var user in ObjectTool.FindAllObject<IARWorldUser>()) user.World = this;
        }

        protected void OnDisable()
        {
            this.planeManager.planesChanged -= this.OnPlaneChanged;
        }
        protected void Update()
        {
            if (this.syncedData != null)
            {
                this.ARCamera.transform.position = this.syncedData.position;
                this.ARCamera.transform.rotation = this.syncedData.rotation;
                this.ARCamera.transform.localScale = this.syncedData.scale;
            }
        }

        protected void OnPlaneChanged(ARPlanesChangedEventArgs obj)
        {
            var maxSize = 0f;
            foreach (var p in obj.added)
            {
                var size = p.size.sqrMagnitude;
                if (this.IsPlayable(p) && size > maxSize)
                {
                    this.playableFloor = p;
                    maxSize = size;
                }
            }
            foreach (var p in obj.updated)
            {
                var size = p.size.sqrMagnitude;
                if (this.IsPlayable(p) && size > maxSize)
                {
                    this.playableFloor = p;
                    maxSize = size;
                }
            }
        }

        protected bool IsPlayable(ARPlane p)
        {
            var type = p.classification;
            var bound = p.size;
            return (type == PlaneClassification.Floor && bound.sqrMagnitude > 1);
        }

        public void OnBind(RemoteDebug debug)
        {
            debug.Bind(this);
        }

    }
}