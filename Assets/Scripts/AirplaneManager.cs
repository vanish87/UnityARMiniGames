using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityARMiniGames
{
    public class AirplaneManager : MonoBehaviour, ARWorld.ARWorldUser
    {
        public ARWorld World { get; set; }
        [SerializeField] protected GameObject airplanePrefab = null;
        [SerializeField] protected List<PaperAirplane> planes = new List<PaperAirplane>();

        protected void Start()
        {
            LogTool.AssertNotNull(this.airplanePrefab);
        }

        protected void Update()
        {
            if (Input.touchCount > 0 )
            {
                var pos = Input.GetTouch(0).position;
                var ray = this.World.ARCamera.ScreenPointToRay(pos);

                this.CreatePlane(this.World.ARCamera.transform.position, ray.direction, 1);
            }
            if(Input.GetKeyDown(KeyCode.I))
            {
                var ray = new Ray(this.World.ARCamera.transform.position, this.World.ARCamera.transform.forward);
                this.CreatePlane(this.World.ARCamera.transform.position, ray.direction, 1);
            }
        }


        protected void CreatePlane(float3 pos, float3 dir, float speed)
        {
            var go = GameObject.Instantiate(this.airplanePrefab, this.transform);
            var plane = go.GetComponent<PaperAirplane>();
            LogTool.AssertNotNull(plane);
            plane.Init(pos, dir, speed);
            
            this.planes.Add(plane);
        }

    }
}
