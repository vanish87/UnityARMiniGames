using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
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

        
        public void Init(float3 translate, Quaternion rotation, Airplane.Parameter parameter)
        {
            this.translate = translate;
            this.parameter = parameter;
            this.localRotation = rotation;
        }
        protected void Start()
        {
            LogTool.AssertNotNull(this.parameter);
            this.runtime.Init(this.parameter);
            this.curve.Init(this.parameter);
        }

        protected void Update()
        {
            Airplane.Process(Time.deltaTime, this.runtime, this.parameter);

            this.transform.position = new float3(this.localRotation * this.runtime.Pos) + this.translate;
            this.transform.rotation = this.localRotation * this.runtime.RotModel;
            // this.transform.rotation = this.transform.localRotation * this.localRotation;
        }

        protected void OnDrawGizmos()
        {
            Airplane.OnDrawGizmos(this.localRotation, this.translate, this.curve, this.parameter);
        }
    }
}