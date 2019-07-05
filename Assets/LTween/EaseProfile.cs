using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Luxko.Tween
{
    [System.Serializable]
    public struct EaseProfile
    {
        public Easing ease;
        [Range(-10, 10)]
        public float ex;
        [Range(-10, 10)]
        public float ey;
        [Range(-10, 10)]
        public float ez;

        public float Ease(float t) { return ease.Ease(t, new Vector3(ex, ey, ez)); }

        public EaseProfile(Easing ease, float ex, float ey, float ez)
        {
            this.ease = ease;
            this.ex = ex;
            this.ey = ey;
            this.ez = ez;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EaseProfile))]
    public class EaseProfileDrawer : PropertyDrawer
    {
        static EditorTools.CurveDrawer _curveDrawer = new EditorTools.CurveDrawer(new Rect(0, -0.5f, 1, 2));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var _ease = property.FindPropertyRelative("ease");
            {
                var easeRect = position;
                easeRect.height = EditorGUIUtility.singleLineHeight;

                var foldoutRect = easeRect;
                foldoutRect.width *= 0.25f;
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

                easeRect.x += position.width * 0.3f;
                easeRect.width -= position.width * 0.3f;

                EditorGUI.PropertyField(easeRect, _ease, GUIContent.none);
            }

            if (property.isExpanded)
            {
                var dx = 15f;
                position.x += dx;
                position.width -= dx;

                var dy = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                position.y += dy;
                position.height -= dy;

                var graphWidth = dy * 12 * _curveDrawer.ValueRange.width / _curveDrawer.ValueRange.height;
                var sliderWidth = position.width - graphWidth - dx;

                var sliderRect = position;
                sliderRect.width = sliderWidth;
                sliderRect.height = EditorGUIUtility.singleLineHeight;
                var _ex = property.FindPropertyRelative("ex");
                EditorGUI.PropertyField(sliderRect, _ex);
                sliderRect.y += dy;
                var _ey = property.FindPropertyRelative("ey");
                EditorGUI.PropertyField(sliderRect, _ey);
                sliderRect.y += dy;
                var _ez = property.FindPropertyRelative("ez");
                EditorGUI.PropertyField(sliderRect, _ez);
                sliderRect.y += dy;

                var vp = position;
                vp.x += sliderWidth + dx;
                vp.width = graphWidth;
                _curveDrawer.ViewPort = vp;

                _curveDrawer.DrawRect(_curveDrawer.ValueRange, 0.1f, 0.4f);
                _curveDrawer.DrawLine(new Vector2(0, 0), new Vector2(1, 0), Color.blue * 0.5f);
                _curveDrawer.DrawLine(new Vector2(0, 1), new Vector2(1, 1), Color.red * 0.5f);
                var ep = new EaseProfile((Easing)_ease.intValue, _ex.floatValue, _ey.floatValue, _ez.floatValue);
                _curveDrawer.DrawCurve(x => ep.Ease(x), Color.white);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                height += 3 * height;
            }
            return height;
        }
    }
#endif
}