using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityARMiniGames
{
    [System.Serializable]
    public class Airplane
    {
        [System.Serializable]
        public class Parameter
        {
            public Color color;
            public float initSpeed = 3.7f;
            public float initAngle = 0;
            public float initHeight = 2;

            public float m = 0.003f;//weight kg
            public float wingsSpan = 0.12f;
            public float S = 0.017f;//Wing Area m2
            //[SerializeField] protected float planeLength = 0.28f; //m

            public float g = 9.807f;//gravity m/s2
            public float p = 1.225f;//air density kg/m3
        }

        [System.Serializable]
        public class Runtime
        {
            public float ar = 0.86f; //wingsSpan*wingsSpan/wingArea

            //Runtime variables
            public float attackAngle = 9.3f * Mathf.Deg2Rad;//alpha
            public float CLx = 0;
            public float CL = 0;
            public float CD = 0;

            public float v = 0;//air speed
            public float y = 0;//flight path angle
            public float h = 0;//height
            public float r = 0;//range

            public float t = 0;//time

            public float3 Pos => new float3(this.r, this.h, 0);
            public Quaternion Rot => Quaternion.Euler(0, 0, this.y * Mathf.Rad2Deg);

            public Quaternion RotModel => Quaternion.Euler(0, 0, (this.y + this.attackAngle) * Mathf.Rad2Deg);
            public void Init(Parameter p)
            {
                this.ar = p.wingsSpan * p.wingsSpan / p.S;
                var ar2 = (ar / 2) * (ar / 2);
                this.CLx = (Mathf.PI * ar) / (1 + Mathf.Sqrt(1 + ar2));
                this.CL = this.CLx * this.attackAngle;

                var e = 1 / (Mathf.PI * 0.9f * ar);
                this.CD = 0.02f + e * CL * CL;

                this.v = p.initSpeed;
                this.y = p.initAngle * Mathf.Deg2Rad;

                this.h = p.initHeight;
                this.r = 0;
                this.t = 0;
            }
        }

        protected delegate T DyDx<T>(T y, float t, Runtime runtime, Parameter parameter);

        public static void Process(float dt, Runtime runtime, Parameter parameter)
        {
            var nv = Step(DvDt, runtime.v, runtime.t, dt, runtime, parameter);
            var ny = Step(DyDt, runtime.y, runtime.t, dt, runtime, parameter);
            var nh = Step(DhDt, runtime.h, runtime.t, dt, runtime, parameter);
            var nr = Step(DrDt, runtime.r, runtime.t, dt, runtime, parameter);
            runtime.t += dt;

            runtime.v = nv;
            runtime.y = ny;
            runtime.h = nh;
            runtime.r = nr;
        }
        protected static float DrDt(float h, float t, Runtime r, Parameter p)
        {
            return r.v * Mathf.Cos(r.y);
        }
        protected static float DhDt(float h, float t, Runtime r, Parameter p)
        {
            return r.v * Mathf.Sin(r.y);
        }
        protected static float DvDt(float v, float t, Runtime r, Parameter p)
        {
            return -r.CD * (0.5f * p.p * v * v) * p.S / p.m - p.g * Mathf.Sin(r.y);
        }
        protected static float DyDt(float y, float t, Runtime r, Parameter p)
        {
            return (r.CL * (0.5f * p.p * r.v * r.v) * p.S / p.m - p.g * Mathf.Cos(y)) / (r.v != 0 ? r.v : 1);
        }

        protected static float Step(DyDx<float> func, float yn, float xn, float h, Runtime runtime, Parameter parameter)
        {
            var k1 = h * func(yn, xn, runtime, parameter);
            var k2 = h * func(yn + 0.5f * k1, xn + 0.5f * h, runtime, parameter);
            yn = yn + k2;

            return yn;
        }

        public static void OnDrawGizmos(Quaternion q, float3 t, Runtime runtime, Parameter parameter)
        {
            Gizmos.color = parameter.color;
            runtime.Init(parameter);

            var pos = new float3(runtime.Pos.x, runtime.Pos.y, 0);
            pos = q * pos;
            pos += t;
            Gizmos.DrawRay(pos, q * runtime.Rot * Vector3.right);

            for (var i = 0; i < 5000; ++i)
            {
                var prev = new float3(runtime.Pos.x, runtime.Pos.y, 0);
                Process(0.001f, runtime, parameter);
                pos = new float3(runtime.Pos.x, runtime.Pos.y, 0);

                Gizmos.DrawLine(new float3(q * prev) + t, new float3(q * pos) + t);
            }
        }
    }

}