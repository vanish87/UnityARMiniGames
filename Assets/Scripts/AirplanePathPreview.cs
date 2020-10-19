using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools;
using UnityTools.Debuging;

namespace UnityARMiniGames
{
    public class AirplanePathPreview : MonoBehaviour, ARWorld.IARWorldUser, AirplaneManager.IAirplaneManagerUser
    {
        public ARWorld World { get; set; }
        public AirplaneManager Manager { get; set; }

        [SerializeField] protected float previewTimeLength = 5;
        [SerializeField] protected float dt = 0.001f;
        [SerializeField] protected float3 posOffset = new float3();
        [SerializeField] protected float rotate = 0f;
        protected LineRenderer lineRenderer;
        protected Airplane.Runtime runtime = new Airplane.Runtime();

        protected void Start()
        {
            this.lineRenderer = ObjectTool.FindOrAddTypeInComponentsAndChildren<LineRenderer>(this.gameObject);
            LogTool.AssertNotNull(this.lineRenderer);
            LogTool.AssertIsTrue(this.dt > 0);
        }

        protected void Update()
        {
            this.UpdateLinePostions();
        }

        protected void UpdateLinePostions()
        {
            var count = Mathf.CeilToInt(this.previewTimeLength / dt * ((Time.time % 5) / 5f));
            var vcount = 100;
            var pcount = Mathf.CeilToInt(count * 1f / vcount);
            if (this.lineRenderer.positionCount != pcount) this.lineRenderer.positionCount = pcount;

            this.runtime.Init(Manager.CurrentParameter);

            foreach (var p in Enumerable.Range(0, count))
            {
                if (p % vcount == 0)
                {
                    float3 cameraOffset = this.World.ARCamera.transform.position;
                    var pos = this.runtime.Pos + this.posOffset;
                    pos = Manager.CurrentLocalRotation * Quaternion.Euler(0,this.rotate,0) * pos;
                    pos += cameraOffset;
                    this.lineRenderer.SetPosition(p / vcount, pos);
                    // Debug.Log(p/vcount + " " + this.lineRenderer.positionCount);
                }
                Airplane.Process(this.dt, this.runtime, Manager.CurrentParameter);
            }
            this.lineRenderer.startWidth = 0.1f;
            this.lineRenderer.endWidth = 0.1f;
        }
    }

}