using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Luxko.Tween
{
    public enum Easing
    {
        None = 0,
        Linear = 1,

        OutSine = 2,
        InSine = 3,
        InOutSine = 4,

        InExpo = 5,
        OutExpo = 6,
        InOutExpo = 7,

        InBack = 8,
        OutBack = 9,
        InOutBack = 10,

        InBounce = 11,
        OutBounce = 12,
        InOutBounce = 13,

        InElastic = 14,
        OutElastic = 15,
        InOutElastic = 16,

        CubicInterpolation = 17,
        PowerCurve = 18,

        HoldTillStart = 19,
        HoldUntilEnd = 20,
    }

    public static class EasingExtensions
    {
        const float Pi = Mathf.PI;
        const float PiDiv2 = Pi * 0.5f;
        // From https://github.com/nicolausYes/easing-functions/blob/master/src/easing.cpp
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInSine(float t) { return Mathf.Sin(PiDiv2 * t); }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseOutSine(float t) { return 1 + Mathf.Sin(PiDiv2 * (t - 1)); }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInOutSine(float t) { return 0.5f * (1 + Mathf.Sin(Pi * (t - 0.5f))); }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInExpo(float t, float e = 8) { return (Mathf.Pow(2, e * t) - 1) / (Mathf.Pow(2, e) - 1); }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseOutExpo(float t, float e = 8) { return EaseInExpo(t, -e); }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInOutExpo(float t, float e = 8)
        {
            if (t < 0.5f) return EaseInExpo(t * 2, e) * 0.5f;
            else return EaseOutExpo(t * 2 - 1, e) * 0.5f + 0.5f;
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInBack(float t, float e = 1.6f) { return t * t * ((e + 1) * t - e); }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseOutBack(float t, float e = 1.6f)
        {
            var t1 = t - 1;
            return 1 + t1 * t1 * ((e + 1) * t1 + e);
        }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInOutBack(float t, float e = 1.6f)
        {
            if (t < 0.5f) return EaseInBack(t * 2, e) * 0.5f;
            else return EaseOutBack(t * 2 - 1, e) * 0.5f + 0.5f;
        }

        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInBounce(float t, float ey = 5, float ex = 2, float bounce = 1)
        {
            return Mathf.Pow(2, ey * (t - 1)) * Mathf.Abs(Mathf.Sin(EaseOutExpo(t, ex) * Pi * (bounce * 2 + 0.5f)));
        }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseOutBounce(float t, float ey = 5, float ex = 2, float bounce = 1)
        {
            return 1 - Mathf.Pow(2, -ey * t) * Mathf.Abs(Mathf.Cos(EaseInExpo(t, ex) * Pi * (bounce * 2 + 0.5f)));
        }
        [System.Runtime.CompilerServices.MethodImpl(256)]
        static float EaseInOutBounce(float t, float ey = 5, float ex = 2, float bounce = 1)
        {
            if (t < 0.5f) return EaseInBounce(t * 2, ey, ex, bounce) * 0.5f;
            else return EaseOutBounce(t * 2 - 1, ey, ex, bounce) * 0.5f + 0.5f;
        }

        static float EaseInElastic(float t, float ey = 5, float ex = 1.66f, float bounce = 1)
        {
            return Mathf.Pow(2, ey * (t - 1)) * Mathf.Sin(EaseOutExpo(t, ex) * Pi * (bounce * 2 + 0.5f));
        }

        static float EaseOutElastic(float t, float ey = 5, float ex = 1.66f, float bounce = 1)
        {
            return 1 - Mathf.Pow(2, -ey * t) * Mathf.Cos(EaseInExpo(t, ex) * Pi * (bounce * 2 + 0.5f));
        }

        static float EaseInOutElastic(float t, float ey = 5, float ex = 1.66f, float bounce = 1)
        {
            if (t < 0.5f) return EaseInElastic(t * 2, ey, ex, bounce) * 0.5f;
            else return EaseOutElastic(t * 2 - 1, ey, ex, bounce) * 0.5f + 0.5f;
        }

        static float EaseCubic(float t, float ea = 0, float eb = 0f)
        {
            var t1 = 1 - t;
            return ((ea * t1 + (1 - eb) * t) * 3 * t1 + t * t) * t;
        }

        static float PowerCurve(float t, float ea = 0, float eb = 0)
        {
            float k = Mathf.Pow(ea + eb, ea + eb) / (Mathf.Pow(ea, ea) * Mathf.Pow(eb, eb));
            return k * Mathf.Pow(t, ea) * Mathf.Pow(1 - t, eb);
        }

        public static float Ease(this Easing method, float t)
        {
            switch (method)
            {
                case Easing.None: return 1;
                case Easing.Linear: return t;
                case Easing.OutSine: return EaseInSine(t);
                case Easing.InSine: return EaseOutSine(t);
                case Easing.InOutSine: return EaseInOutSine(t);

                case Easing.InExpo: return EaseInExpo(t);
                case Easing.OutExpo: return EaseOutExpo(t);
                case Easing.InOutExpo: return EaseInOutExpo(t);

                case Easing.InBack: return EaseInBack(t);
                case Easing.OutBack: return EaseOutBack(t);
                case Easing.InOutBack: return EaseInOutBack(t);

                case Easing.InBounce: return EaseInBounce(t);
                case Easing.OutBounce: return EaseOutBounce(t);
                case Easing.InOutBounce: return EaseInOutBounce(t);

                case Easing.InElastic: return EaseInElastic(t);
                case Easing.OutElastic: return EaseOutElastic(t);
                case Easing.InOutElastic: return EaseInOutElastic(t);

                case Easing.CubicInterpolation: return EaseCubic(t);

                case Easing.PowerCurve: return PowerCurve(t);

                case Easing.HoldTillStart: return t <= 0 ? 0 : 1;
                case Easing.HoldUntilEnd: return t < 1 ? 0 : 1;
                default: return t;
            }
        }

        public static float Ease(this Easing method, float t, Vector3 config)
        {
            switch (method)
            {
                case Easing.None: return 1;
                case Easing.Linear: return t;
                case Easing.OutSine: return EaseInSine(t);
                case Easing.InSine: return EaseOutSine(t);
                case Easing.InOutSine: return EaseInOutSine(t);

                case Easing.InExpo: return EaseInExpo(t, config.x);
                case Easing.OutExpo: return EaseOutExpo(t, config.x);
                case Easing.InOutExpo: return EaseInOutExpo(t, config.x);

                case Easing.InBack: return EaseInBack(t, config.x);
                case Easing.OutBack: return EaseOutBack(t, config.x);
                case Easing.InOutBack: return EaseInOutBack(t, config.x);

                case Easing.InBounce: return EaseInBounce(t, config.x, config.y, config.z);
                case Easing.OutBounce: return EaseOutBounce(t, config.x, config.y, config.z);
                case Easing.InOutBounce: return EaseInOutBounce(t, config.x, config.y, config.z);

                case Easing.InElastic: return EaseInElastic(t, config.x, config.y, config.z);
                case Easing.OutElastic: return EaseOutElastic(t, config.x, config.y, config.z);
                case Easing.InOutElastic: return EaseInOutElastic(t, config.x, config.y, config.z);

                case Easing.CubicInterpolation: return EaseCubic(t, config.x, config.y);

                case Easing.PowerCurve: return PowerCurve(t, config.x, config.y);

                case Easing.HoldTillStart: return t <= 0 ? 0 : 1;
                case Easing.HoldUntilEnd: return t < 1 ? 0 : 1;
                default: return t;
            }
        }
    }

    [Serializable]
    public abstract class Tweener : IEnumerator
    {
        protected float t = 0;

        protected abstract void Update();

        public object Current { get { return null; } }

        public bool MoveNext()
        {
            if (t < 0 || t > 1)
            {
                _invoking = false;
                return false;
            }
            Update();
            return true;
        }

        public void SetTimeThenEvaluate(float t)
        {
            this.t = Mathf.Clamp01(t);
            Update();
        }

        public virtual void Reset() { t = 0; }
        bool _invoking;
        public bool Invoking { get { return _invoking; } }
        public void InvokeBy(MonoBehaviour invoker)
        {
            Reset();
            if (!_invoking)
            {
                _invoking = true;
                invoker.StartCoroutine(this);
            }
        }
    }
}
