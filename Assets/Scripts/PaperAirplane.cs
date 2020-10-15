using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityARMiniGames
{
    public class PaperAirplane : MonoBehaviour
    {
        [SerializeField] protected Airplane.Parameter parameter = new Airplane.Parameter();
        [SerializeField] protected Airplane.Runtime runtime = new Airplane.Runtime();
        protected Airplane.Runtime curve = new Airplane.Runtime();
        protected Quaternion localRotation;

        public void Init(float3 startPos, float3 dir, float speed)
        {
            this.parameter.initHeight = startPos.y;
            // this.parameter.initSpeed = speed;
            this.parameter.initAngle = math.asin(dir.y) * Mathf.Rad2Deg;

            dir.y = 0;
            var d1 = math.normalize(new float3(1,0,0));
            var d2 = math.normalize(dir);
            var axis = math.cross(d1,d2);
            var angle = math.acos(math.dot(d1,d2)) * Mathf.Rad2Deg;
            this.localRotation = Quaternion.AngleAxis(angle, axis);

        }
        protected void Start()
        {
            this.runtime.Init(this.parameter);
            this.curve.Init(this.parameter);
        }

        protected void Update()
        {
            Airplane.Process(Time.deltaTime, this.runtime, this.parameter);

            this.transform.position = this.localRotation * this.runtime.Pos;//Matrix4x4.Rotate(this.localRotation).MultiplyPoint(this.runtime.Pos);
            this.transform.rotation = this.localRotation * this.runtime.RotModel;
            // this.transform.rotation = this.transform.localRotation * this.localRotation;
        }

        protected void OnDrawGizmos()
        {
            Airplane.OnDrawGizmos(this.localRotation, this.curve, this.parameter);
        }
    }
}