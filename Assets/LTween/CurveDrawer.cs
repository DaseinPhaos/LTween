#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Luxko.EditorTools
{
    public class CurveDrawer
    {
        const int k_MaxRes = 192;
        static Vector3[] _curveVerts = new Vector3[k_MaxRes];
        public Rect ViewPort;
        public Rect ValueRange = new Rect(0, 0, 1, 1);

        Vector2 PointInViewport(float x, float y)
        {
            x = Mathf.LerpUnclamped(ViewPort.x, ViewPort.xMax, (x - ValueRange.x) / ValueRange.width);
            y = Mathf.LerpUnclamped(ViewPort.yMax, ViewPort.y, (y - ValueRange.y) / ValueRange.height);
            return new Vector2(x, y);
        }

        public void DrawRect(Rect rect, float fillAlpha, float borderAlpha)
        {
            _curveVerts[0] = PointInViewport(rect.xMin, rect.yMin);
            _curveVerts[1] = PointInViewport(rect.xMax, rect.yMin);
            _curveVerts[2] = PointInViewport(rect.xMax, rect.yMax);
            _curveVerts[3] = PointInViewport(rect.xMin, rect.yMax);

            Handles.DrawSolidRectangleWithOutline(
                _curveVerts,
                fillAlpha < 0 ? Color.clear : Color.white * fillAlpha,
                borderAlpha < 0 ? Color.clear : Color.white * borderAlpha
            );
        }

        public void DrawCurve(System.Func<float, float> curve, Color color, int sampleCount = 128)
        {
            if (sampleCount < 2) sampleCount = 2;
            if (sampleCount > k_MaxRes) sampleCount = k_MaxRes;
            for (int i = 0; i < sampleCount - 1; ++i)
            {
                var x = ((float)i) / sampleCount * ValueRange.width + ValueRange.x;
                var y = curve(x);
                _curveVerts[i] = PointInViewport(x, y);
            }
            {
                var x = ValueRange.width + ValueRange.x;
                var y = curve(x);
                _curveVerts[sampleCount - 1] = PointInViewport(x, y);
            }
            var oc = Handles.color;
            Handles.color = color;
            Handles.DrawAAPolyLine(2.0f, sampleCount, _curveVerts);
            Handles.color = oc;
        }

        public void DrawLine(Vector2 from, Vector2 to, Color color)
        {
            _curveVerts[0] = PointInViewport(from.x, from.y);
            _curveVerts[1] = PointInViewport(to.x, to.y);
            var oc = Handles.color;
            Handles.color = color;
            Handles.DrawAAPolyLine(2.0f, 2, _curveVerts);
            Handles.color = oc;
        }

        public CurveDrawer(Rect valueRange)
        {
            this.ValueRange = valueRange;
        }
    }
}

#endif