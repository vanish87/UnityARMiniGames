using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools;
using UnityTools.Debuging;

namespace UnityARMiniGames
{
    public class AirplaneManager : MonoBehaviour, ARWorld.IARWorldUser
    {
        public interface IAirplaneManagerUser
        {
            AirplaneManager Manager { get; set; }
        }
        public ARWorld World { get; set; }

        public Airplane.Parameter CurrentParameter => this.parameter;
        public Quaternion CurrentLocalRotation => this.localRotation;
        [SerializeField] protected GameObject airplanePrefab = null;
        [SerializeField] protected Airplane.Parameter parameter = new Airplane.Parameter();
        // [SerializeField] protected List<PaperAirplane> planes = new List<PaperAirplane>();

        protected Quaternion localRotation;

        protected void Start()
        {
            LogTool.AssertNotNull(this.airplanePrefab);
            foreach (var user in this.GetComponentsInChildren<IAirplaneManagerUser>().ToList()) user.Manager = this;
        }

        protected void Update()
        {
            if (Input.touchCount > 1)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended)
                {
                    // var ray = this.World.ARCamera.ScreenPointToRay(pos);

                    this.CreateAirplane();
                }

            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                var ray = new Ray(this.World.ARCamera.transform.position, this.World.ARCamera.transform.forward);
                this.CreateAirplane();
            }

            this.UpdateCurrentParameter();
            this.UpdateLocalRotation();

            var speed = this.CheckSwipe();
            if (speed > 0)
            {
                this.CreateAirplane(speed);
            }


            // this.CheckDestory();
        }

        protected float swipeTimer = 0;
        protected float2 startPos;
        protected float CheckSwipe()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    swipeTimer = 0;
                    startPos = touch.position;
                }
                else
                if (touch.phase == TouchPhase.Moved)
                {
                    swipeTimer += touch.deltaTime;
                }
                else
                if (touch.phase == TouchPhase.Ended)
                {
                    if(this.swipeTimer > 0.1f)
                    {
                        var deltaDistance = (new float2(touch.position) - this.startPos) / new float2(Screen.width, Screen.height);
                        var speed = math.length(deltaDistance) / this.swipeTimer;
                        speed *= 2;
                        LogTool.Log("speed " + speed);
                        LogTool.Log("timer " + this.swipeTimer);
                        return speed;
                    }
                }
            }
            return -1;
        }

        protected void CreateAirplane(float speed = 0)
        {
            var go = GameObject.Instantiate(this.airplanePrefab, this.transform);
            var plane = go.GetComponent<PaperAirplane>();
            LogTool.AssertNotNull(plane);

            var para = this.parameter.DeepCopyJson();
            para.initSpeed = speed>0?speed:para.initSpeed;
            var offset = this.World.ARCamera.transform.position;
            plane.Init(offset, this.localRotation, para);

            // this.planes.Add(plane);
        }

        // protected void CheckDestory()
        // {
        //     for(var i = 0; i < this.planes.Count; ++i)
        //     {
        //         var p = this.planes[i];
        //         if(p.transform.position.y < -1)
        //         {
        //             this.planes.RemoveAt(i);
        //             p.gameObject.DestoryObj();
        //             i--;
        //         }
        //     }
        // }
        protected void UpdateCurrentParameter()
        {
            var dir = this.World.ARCamera.transform.forward;
            this.parameter.initHeight = 0;
            this.parameter.initAngle = math.asin(dir.y) * Mathf.Rad2Deg;
        }
        protected void UpdateLocalRotation()
        {
            var dir = this.World.ARCamera.transform.forward;
            dir.y = 0;
            var d1 = math.normalize(new float3(1, 0, 0));
            var d2 = math.normalize(dir);
            var axis = math.cross(d1, d2);
            var angle = math.acos(math.dot(d1, d2)) * Mathf.Rad2Deg;
            this.localRotation = Quaternion.AngleAxis(angle, axis);
        }
    }
}
