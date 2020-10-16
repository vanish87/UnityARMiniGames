using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


        public string HashString => this.ToString();

        public byte[] OnSerialize()
        {
            var str = JsonUtility.ToJson(this.ARCamera.transform);
            var data = Serialization.ObjectToByteArray(str);
            return CompressTool.Compress(data);
        }

        public void OnDeserialize(byte[] data)
        {
            var str = Serialization.ByteArrayToObject<string>(CompressTool.Decompress(data));
            var remote = JsonUtility.FromJson<Transform>(str);
            this.ARCamera.transform.position = remote.position;
            this.ARCamera.transform.rotation = remote.rotation;
            this.ARCamera.transform.localScale = remote.localScale;
        }
        public Camera ARCamera => this.currentOrigin.camera;


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