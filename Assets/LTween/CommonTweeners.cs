using UnityEngine;

namespace Luxko.Tween
{
    [System.Serializable]
    public class RtActivationTweener : Tweener
    {
        public RectTransform rect;
        public float activateScale = 1;
        public float deactivateScale;
        public Vector2 activateAnchor;
        public Vector2 deactivateAnchor;
        public Easing activateEase;
        public float activateDuration;
        public Easing deacitvateEase;
        public float deactivateDuration;

        bool _toActive;
        protected override void Update()
        {
            if (rect == null)
            {
                t = 1;
                return;
            }
            float duration;
            Easing method;
            Vector2 fromAnchor;
            Vector2 toAnchor;
            float fromScale;
            float toScale;
            if (_toActive)
            {
                duration = activateDuration;
                method = activateEase;
                fromAnchor = deactivateAnchor;
                toAnchor = activateAnchor;
                fromScale = deactivateScale;
                toScale = activateScale;
            }
            else
            {
                duration = deactivateDuration;
                method = deacitvateEase;
                fromAnchor = activateAnchor;
                toAnchor = deactivateAnchor;
                fromScale = activateScale;
                toScale = deactivateScale;
            }
            t += Time.unscaledDeltaTime / duration;
            var et = t >= 1 ? 1 : method.Ease(t);
            if (_toActive) rect.gameObject.SetActive(true);
            else if (t >= 1) rect.gameObject.SetActive(false);
            rect.anchoredPosition = Vector2.LerpUnclamped(fromAnchor, toAnchor, et);
            var scale = Mathf.LerpUnclamped(fromScale, toScale, et);
            rect.localScale = new Vector3(scale, scale, 1);
        }

        public void Reset(bool toActive)
        {
            this._toActive = toActive;
            Reset();
        }
    }

    [System.Serializable]
    public class DummyTweener : Tweener
    {
        public float duration;
        [System.NonSerialized] public System.Action<float> _act; // t
        protected override void Update()
        {
            if (_act == null)
            {
                t = 1;
                return;
            }

            t += Time.unscaledDeltaTime / duration;
            _act(t);
        }
    }

    [System.Serializable]
    public class GeneralTweener : Tweener
    {
        public EaseProfile ease;
        public float duration;
        public float offset;
        float _offsetCounter;

        [System.NonSerialized] public System.Action<float, float> _act; // t, et

        protected override void Update()
        {
            if (_act == null)
            {
                t = 1;
                return;
            }
            var dt = Time.unscaledDeltaTime;
            if (_offsetCounter < offset)
            {
                _offsetCounter += dt;
                return;
            }
            t += dt / duration;
            var et = t >= 1 ? 1 : ease.Ease(t);
            _act(t, et);
        }

        public GeneralTweener(EaseProfile profile, float duration) : base()
        {
            this.ease = profile;
            this.duration = duration;
        }

        public override void Reset()
        {
            _offsetCounter = 0;
            base.Reset();
        }
    }

    [System.Serializable]
    public class GeneralInOutTweener : Tweener
    {
        public EaseProfile inEase;
        public float inDuration;
        public float inOffset;
        public EaseProfile outEase;
        public float outDuration;
        public float outOffset;
        bool _isIn;
        float _offsetCounter;
        [System.NonSerialized] public System.Action<float, float, bool> _act; // t, et, isIn

        protected override void Update()
        {
            if (_act == null)
            {
                t = 1;
                return;
            }
            float duration;
            EaseProfile method;

            var dt = Time.unscaledDeltaTime;

            if (_isIn)
            {
                if (_offsetCounter < inOffset)
                {
                    _offsetCounter += dt;
                    return;
                }
                duration = inDuration;
                method = inEase;
            }
            else
            {
                if (_offsetCounter < outOffset)
                {
                    _offsetCounter += dt;
                    return;
                }
                duration = outDuration;
                method = outEase;
            }

            t += dt / duration;
            // when out, et is eased from (1-t)
            var et = t >= 1 ? (_isIn ? 1 : 0) : method.Ease(_isIn ? t : (1 - t));

            _act(t, et, _isIn);
        }

        public void Reset(bool isIn)
        {
            // _act = act;
            _isIn = isIn;
            Reset();
        }

        public override void Reset()
        {
            _offsetCounter = 0;
            base.Reset();
        }
    }
}