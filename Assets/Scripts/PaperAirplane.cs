using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools;
using UnityTools.Debuging;

namespace UnityARMiniGames
{
    public class PaperAirplane : MonoBehaviour
    {
        [SerializeField] protected Airplane.Parameter parameter = null;
        [SerializeField] protected Airplane.Runtime runtime = new Airplane.Runtime();
        protected Airplane.Runtime curve = new Airplane.Runtime();
        protected Quaternion localRotation;
        protected float3 translate;
        protected Mode mode = Mode.AirPhysics;

        protected enum Mode
        {
            AirPhysics,
            UnityRigidBody
        }


        public void Init(float3 translate, Quaternion rotation, Airplane.Parameter parameter)
        {
            this.translate = translate;
            this.parameter = parameter;
            this.localRotation = rotation;
        }
        protected void Start()
        {
            LogTool.AssertNotNull(this.parameter);

            this.parameter.g = this.parameter.g * math.exp(UnityEngine.Random.value);
            this.runtime.Init(this.parameter);
            this.curve.Init(this.parameter);
        }

        protected void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<FloorMarker>() == null) return;
            var rig = this.GetComponent<Rigidbody>();
            rig.velocity = this.transform.right * this.runtime.v;
            this.mode = Mode.UnityRigidBody;

            this.StartCoroutine(this.DestoryTimer());
        }

        protected void Update()
        {
            if (this.mode == Mode.AirPhysics)
            {
                var step = 10;
                var dt = Time.deltaTime / step;
                foreach (var s in Enumerable.Range(0, step))
                {
                    Airplane.Process(dt, this.runtime, this.parameter);
                }

                this.transform.position = new float3(this.localRotation * this.runtime.Pos) + this.translate;
                this.transform.rotation = this.localRotation * this.runtime.RotModel;
            }

        }

        protected void OnDrawGizmos()
        {
            Airplane.OnDrawGizmos(this.localRotation, this.translate, this.curve, this.parameter);
        }

        protected IEnumerator DestoryTimer()
        {
            yield return new WaitForSeconds(10);
            this.gameObject.DestoryObj();
        }
    }
}