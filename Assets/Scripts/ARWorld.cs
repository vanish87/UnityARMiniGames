using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityTools;
using UnityTools.Debuging;

namespace UnityARMiniGames
{
    public class ARWorld : MonoBehaviour
    {
        public interface ARWorldUser
        {
            ARWorld World { get; set; }
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

            foreach(var user in this.GetComponentsInChildren<ARWorldUser>()) user.World = this;
        }

        protected void OnDisable()
        {
            this.planeManager.planesChanged -= this.OnPlaneChanged;
        }

        protected void OnPlaneChanged(ARPlanesChangedEventArgs obj)
        {
            var maxSize = 0f;
            foreach(var p in obj.added)
            {
                var size = p.size.sqrMagnitude;
                if(this.IsPlayable(p) && size > maxSize)
                {
                    this.playableFloor = p;
                    maxSize = size;
                }
            }
            foreach(var p in obj.updated)
            {
                var size = p.size.sqrMagnitude;
                if(this.IsPlayable(p) && size > maxSize)
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
    }
}