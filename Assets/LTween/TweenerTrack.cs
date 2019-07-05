using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Luxko.Tween
{
    [System.Serializable]
    public class TweenerTrack : Tweener
    {
        [System.Serializable]
        public struct Clip
        {
            [Range(0.0f, 1.0f)] public float startTime;
            [Range(0.0f, 1.0f)] public float endTime;

            public EaseProfile profile;

            [HideInInspector] public Vector4 startValue;
            [HideInInspector] public Vector4 endValue;
            [HideInInspector] public string propertyPath;
            [HideInInspector] public UnityEngine.Object context;
            internal Action<Vector4> _setter;
            internal Func<Vector4> _getter;

            public void Evaluate(float t)
            {
                var clipT = (t - this.startTime) / (this.endTime - this.startTime);
                if (float.IsNaN(clipT)) clipT = 0;
                else if (clipT < 0) clipT = 0;
                else if (clipT > 1) clipT = 1;
                var et = this.profile.Ease(clipT);
                var ev = Vector4.LerpUnclamped(this.startValue, this.endValue, et);

                this._setter(ev);
            }

            public static void Init(ref Clip clip)
            {
                if (clip._setter != null) return;
                if (clip.context == null)
                {
                    clip._setter = unused => { };
                    clip._getter = () => new Vector4();
                }
                var contextType = clip.context.GetType();

                string propName;
                string postfix;
                var postfixIdx = clip.propertyPath.LastIndexOf('/');

                if (postfixIdx >= 0)
                {
                    propName = clip.propertyPath.Substring(0, postfixIdx);
                    postfix = clip.propertyPath.Substring(postfixIdx + 1);
                }
                else
                {
                    propName = clip.propertyPath;
                    postfix = null;
                }
                var propInfo = contextType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
                var setterInfo = propInfo == null ? null : propInfo.GetSetMethod();
                if (setterInfo == null)
                {
                    clip._setter = unused => { };
                    clip._getter = () => new Vector4();
                }
                if (propInfo.PropertyType == typeof(float))
                {
                    var setter = Delegate.CreateDelegate(typeof(Action<float>), clip.context, setterInfo) as Action<float>;
                    clip._setter = v4 => setter(v4.x);

                    var getter = Delegate.CreateDelegate(typeof(Func<float>), clip.context, propInfo.GetGetMethod()) as Func<float>;
                    clip._getter = () => new Vector4(getter(), 0, 0, 0);
                }
                else if (propInfo.PropertyType == typeof(Vector2))
                {
                    var setter = Delegate.CreateDelegate(typeof(Action<Vector2>), clip.context, setterInfo) as Action<Vector2>;
                    var getter = Delegate.CreateDelegate(typeof(Func<Vector2>), clip.context, propInfo.GetGetMethod()) as Func<Vector2>;

                    if (postfix == "x")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.x = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "y")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.y = v4.x;
                            setter(value);
                        };
                    }
                    else
                    {
                        clip._setter = v4 => setter(v4);
                    }
                    clip._getter = () => getter();
                }
                else if (propInfo.PropertyType == typeof(Vector3))
                {
                    var setter = Delegate.CreateDelegate(typeof(Action<Vector3>), clip.context, setterInfo) as Action<Vector3>;
                    var getter = Delegate.CreateDelegate(typeof(Func<Vector3>), clip.context, propInfo.GetGetMethod()) as Func<Vector3>;

                    if (postfix == "x")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.x = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "y")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.y = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "z")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.z = v4.x;
                            setter(value);
                        };
                    }
                    else
                    {
                        clip._setter = v4 => setter(v4);
                    }
                    clip._getter = () => getter();
                }
                else if (propInfo.PropertyType == typeof(Vector4))
                {
                    var setter = Delegate.CreateDelegate(typeof(Action<Vector4>), clip.context, setterInfo) as Action<Vector4>;
                    var getter = Delegate.CreateDelegate(typeof(Func<Vector4>), clip.context, propInfo.GetGetMethod()) as Func<Vector4>;

                    if (postfix == "x")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.x = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "y")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.y = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "z")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.z = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "w")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.w = v4.x;
                            setter(value);
                        };
                    }
                    else
                    {
                        clip._setter = v4 => setter(v4);
                    }
                    clip._getter = () => getter();
                }
                else if (propInfo.PropertyType == typeof(Color))
                {
                    var setter = Delegate.CreateDelegate(typeof(Action<Color>), clip.context, setterInfo) as Action<Color>;
                    var getter = Delegate.CreateDelegate(typeof(Func<Color>), clip.context, propInfo.GetGetMethod()) as Func<Color>;

                    if (postfix == "x")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.r = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "y")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.g = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "z")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.b = v4.x;
                            setter(value);
                        };
                    }
                    else if (postfix == "w")
                    {
                        clip._setter = v4 =>
                        {
                            var value = getter();
                            value.a = v4.x;
                            setter(value);
                        };
                    }
                    else
                    {
                        clip._setter = v4 => setter(v4);
                    }
                    clip._getter = () => getter();
                }
                else if (propInfo.PropertyType == typeof(bool))
                {
                    if (contextType == typeof(GameObject) && propInfo.Name == "active")
                    {
                        // hack for active, which is obsoleted
                        var go = clip.context as GameObject;
                        clip._setter = v4 => go.SetActive(v4.x != 0.0f);
                        clip._getter = () => go.activeSelf ? new Vector4(1, 0, 0, 0) : new Vector4();
                    }
                    else
                    {
                        var setter = Delegate.CreateDelegate(typeof(Action<bool>), clip.context, setterInfo) as Action<bool>;
                        clip._setter = v4 => setter(v4.x != 0.0f);

                        var getter = Delegate.CreateDelegate(typeof(Func<bool>), clip.context, propInfo.GetGetMethod()) as Func<bool>;
                        clip._getter = () => getter() ? new Vector4(1, 0, 0, 0) : new Vector4();
                    }
                }
                else if (propInfo.PropertyType == typeof(int))
                {
                    var setter = Delegate.CreateDelegate(typeof(Action<int>), clip.context, setterInfo) as Action<int>;
                    clip._setter = v4 => setter((int)v4.x);

                    var getter = Delegate.CreateDelegate(typeof(Func<int>), clip.context, propInfo.GetGetMethod()) as Func<int>;
                    clip._getter = () => new Vector4(getter(), 0, 0, 0);
                }
                else
                {
                    // TODO: support types that implemented a custom interface
                    clip._setter = unused => { };
                    clip._getter = () => new Vector4();
                }
            }
        }
        public Clip[] clips;
        public float duration = 1.0f;

        // public enum UpdateMethod { GameTime, UnscaledGameTime, Manual}
        protected override void Update()
        {
            if (clips == null)
            {
                t = 1;
                return;
            }

            t += Time.unscaledDeltaTime / duration;

            for (int i = 0; i < clips.Length; ++i)
            {
                // if (clips[i]._setter == null) continue;
                clips[i].Evaluate(t);
            }
        }

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < clips.Length; ++i)
            {
                Clip.Init(ref clips[i]);
            }
        }

        [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
        public class EnumAttribute : Attribute
        {
            public Type enumType;
            public bool isFlagMask;
            public EnumAttribute(Type t, bool isFlagMask = false)
            {
                this.enumType = t;
                this.isFlagMask = isFlagMask;
            }
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TweenerTrack.Clip))]
    public class TweenerTrackClipDrawer : PropertyDrawer
    {
        public static TweenerTrack.Clip Get(SerializedProperty property)
        {
            // if (property.type != "")
            var path = property.FindPropertyRelative("propertyPath");
            var context = property.FindPropertyRelative("context");
            var startValue = property.FindPropertyRelative("startValue");
            var endValue = property.FindPropertyRelative("endValue");
            var startTime = property.FindPropertyRelative("startTime");
            var endTime = property.FindPropertyRelative("endTime");
            var profile = property.FindPropertyRelative("profile");
            var ease = profile.FindPropertyRelative("ease");
            var ex = profile.FindPropertyRelative("ex");
            var ey = profile.FindPropertyRelative("ey");
            var ez = profile.FindPropertyRelative("ez");

            return new TweenerTrack.Clip
            {
                propertyPath = path.stringValue,
                context = context.objectReferenceValue,
                startValue = startValue.vector4Value,
                endValue = endValue.vector4Value,
                startTime = startTime.floatValue,
                endTime = endTime.floatValue,
                profile = new EaseProfile
                {
                    ease = (Easing)ease.intValue,
                    ex = ex.floatValue,
                    ey = ey.floatValue,
                    ez = ez.floatValue,
                },
            };
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // label
            EditorGUI.BeginProperty(position, label, property);
            // var baseRect = position;
            // baseRect.height = base.GetPropertyHeight(property, label);
            // base.OnGUI(position, property, label);
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label);
            rect.x += 12f;
            rect.width -= 12f;
            if (property.isExpanded)
            {
                // popup for context options
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                if (property.serializedObject.isEditingMultipleObjects)
                {
                    EditorGUI.LabelField(rect, "Can't MultiEdit", EditorStyles.centeredGreyMiniLabel);
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    var target = property.serializedObject.targetObject;
                    if (target != contextRoot)
                    {
                        contextRoot = target;
                        RefreshContexts();
                    }

                    var path = property.FindPropertyRelative("propertyPath");
                    var context = property.FindPropertyRelative("context");

                    int selected = -1;
                    for (int i = 0; i < contexts.Count; ++i)
                    {
                        if (
                            contexts[i].obj == context.objectReferenceValue
                            && contexts[i].path == path.stringValue
                        )
                        {
                            selected = i;
                            break;
                        }
                    }
                    var newSelected = EditorGUI.Popup(rect, "Context", selected, pathNames);
                    if (newSelected != selected)
                    {
                        path.stringValue = contexts[newSelected].path;
                        context.objectReferenceValue = contexts[newSelected].obj;
                        // ? notify changes?
                    }

                    if (newSelected != -1)
                    {
                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                        var startValue = property.FindPropertyRelative("startValue");
                        ShowPropAsTyped(rect, startValue, contexts[newSelected], property);

                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                        var endValue = property.FindPropertyRelative("endValue");
                        ShowPropAsTyped(rect, endValue, contexts[newSelected], property);
                    }
                    else
                    {
                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }

                // time
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                var sliderRect = rect;
                var sliderSpacing = 0.05f * sliderRect.width;
                sliderRect.width *= 0.45f;
                var startTime = property.FindPropertyRelative("startTime");
                EditorGUI.PropertyField(sliderRect, startTime, GUIContent.none);
                sliderRect.x += sliderSpacing + sliderRect.width;
                var endTime = property.FindPropertyRelative("endTime");
                EditorGUI.PropertyField(sliderRect, endTime, GUIContent.none);

                // profile
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                var dy = rect.y - position.y;
                rect.height = position.height - dy;
                var profile = property.FindPropertyRelative("profile");
                EditorGUI.PropertyField(rect, profile);
            }

            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                height += 4 * height;
                var profile = property.FindPropertyRelative("profile");
                height += EditorGUI.GetPropertyHeight(profile);
            }
            return height;
        }

        UnityEngine.Object contextRoot;
        string[] pathNames = new string[0];

        class Context
        {
            public UnityEngine.Object obj;
            public string prefix;
            public string path;
            public Type valueType;
            public PropertyInfo propInfo;
            public TweenerTrack.EnumAttribute attribute;
        }
        List<Context> contexts = new List<Context>();

        void RefreshContexts()
        {
            // Debug.Log("Refreshing contents");
            contexts.Clear();
            if (contextRoot == null)
            {
                Array.Resize(ref pathNames, 0);
                return;
            }
            GameObject rootGo = null;
            if (contextRoot is GameObject)
            {
                rootGo = (GameObject)contextRoot;
            }
            else if (contextRoot is Component)
            {
                rootGo = ((Component)contextRoot).gameObject;
            }

            if (rootGo == null)
            {
                AddContext(contextRoot, contexts, "");
            }
            else
            {
                AddContextRecursive(rootGo, contexts, "");
            }

            Array.Resize(ref pathNames, contexts.Count);
            for (int i = 0; i < pathNames.Length; ++i)
            {
                var attributes = contexts[i].propInfo.GetCustomAttributes(typeof(TweenerTrack.EnumAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    contexts[i].attribute = attributes[0] as TweenerTrack.EnumAttribute;
                }
                pathNames[i] = contexts[i].prefix + contexts[i].path;
            }
        }

        static void AddContextRecursive(GameObject go, List<Context> list, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith("/"))
            {
                prefix += '/';
            }

            AddContext(go, list, prefix);

            var components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                AddContext(component, list, prefix + component.GetType().Name + '/');
            }

            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i).gameObject;
                AddContextRecursive(child, list, prefix + child.name);
            }
        }

        static void AddContext(UnityEngine.Object obj, List<Context> list, string prefix)
        {

            var objType = obj.GetType();
            // TODO: iterate through valid properties
            var properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var propInfo in properties)
            {
                var setterInfo = propInfo == null ? null : propInfo.GetSetMethod();
                if (setterInfo == null) continue;
                if (propInfo.GetGetMethod() == null) continue;
                if (
                    propInfo.PropertyType == typeof(float)
                    || propInfo.PropertyType == typeof(bool)
                    || propInfo.PropertyType == typeof(int)
                )
                {
                    list.Add(new Context
                    {
                        propInfo = propInfo,
                        obj = obj,
                        prefix = prefix,
                        path = propInfo.Name,
                        valueType = propInfo.PropertyType,
                    });
                }

                if (propInfo.PropertyType == typeof(Vector2)
                    || propInfo.PropertyType == typeof(Vector3)
                    || propInfo.PropertyType == typeof(Vector4)
                    || propInfo.PropertyType == typeof(Color))
                {
                    list.Add(new Context
                    {
                        propInfo = propInfo,
                        obj = obj,
                        prefix = prefix,
                        path = propInfo.Name,
                        valueType = propInfo.PropertyType
                    });
                    list.Add(new Context
                    {
                        propInfo = propInfo,
                        obj = obj,
                        prefix = prefix,
                        path = propInfo.Name + "/x",
                        valueType = typeof(float)
                    });
                    list.Add(new Context
                    {
                        propInfo = propInfo,
                        obj = obj,
                        prefix = prefix,
                        path = propInfo.Name + "/y",
                        valueType = typeof(float)
                    });

                    if (propInfo.PropertyType == typeof(Vector3)
                        || propInfo.PropertyType == typeof(Vector4)
                        || propInfo.PropertyType == typeof(Color))
                    {
                        list.Add(new Context
                        {
                            propInfo = propInfo,
                            obj = obj,
                            prefix = prefix,
                            path = propInfo.Name + "/z",
                            valueType = typeof(float)
                        });

                        if (propInfo.PropertyType == typeof(Vector4)
                            || propInfo.PropertyType == typeof(Color))
                        {
                            list.Add(new Context
                            {
                                propInfo = propInfo,
                                obj = obj,
                                prefix = prefix,
                                path = propInfo.Name + "/w",
                                valueType = typeof(float)
                            });
                        }
                    }
                }
            }
        }

        static void ShowPropAsTyped(Rect rect, SerializedProperty sp, Context context, SerializedProperty clipSp)
        {
            var v4 = sp.vector4Value;
            var vRect = rect;
            vRect.width -= 25;
            var bRect = rect;
            bRect.x += vRect.width + 5;
            bRect.width = 20;

            if (context.valueType == typeof(float))
            {
                v4.x = EditorGUI.FloatField(vRect, GUIContent.none, v4.x);
            }
            else if (context.valueType == typeof(Vector2))
            {
                v4 = EditorGUI.Vector2Field(vRect, GUIContent.none, v4);
            }
            else if (context.valueType == typeof(Vector3))
            {
                v4 = EditorGUI.Vector3Field(vRect, GUIContent.none, v4);
            }
            else if (context.valueType == typeof(Vector4))
            {
                v4 = EditorGUI.Vector4Field(vRect, GUIContent.none, v4);
            }
            else if (context.valueType == typeof(Color))
            {
                v4 = EditorGUI.ColorField(vRect, GUIContent.none, v4);
            }
            else if (context.valueType == typeof(bool))
            {
                var v = (v4.x != 0);
                v4.x = EditorGUI.Toggle(vRect, GUIContent.none, v) ? 1 : 0;
            }
            else if (context.valueType == typeof(int))
            {
                if (context.attribute != null && context.attribute.enumType != null && context.attribute.enumType.IsEnum)
                {
                    if (context.attribute.isFlagMask)
                    {
                        v4.x = (int)(object)(EditorGUI.EnumFlagsField(vRect, GUIContent.none, (Enum)Enum.ToObject(context.attribute.enumType, (int)v4.x)));
                    }
                    else
                    {
                        v4.x = (int)(object)(EditorGUI.EnumPopup(vRect, GUIContent.none, (Enum)Enum.ToObject(context.attribute.enumType, (int)v4.x)));
                    }
                }
                else
                {
                    v4.x = EditorGUI.IntField(vRect, GUIContent.none, (int)v4.x);
                }
            }
            sp.vector4Value = v4;
            if (GUI.Button(bRect, "R"))
            {
                var clip = Get(clipSp);
                TweenerTrack.Clip.Init(ref clip);
                sp.vector4Value = clip._getter();
            }
        }
    }

    [CustomPropertyDrawer(typeof(TweenerTrack))]
    public class TweenTrackDrawer : PropertyDrawer
    {
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return !_previewPlaying && !_previewJustDone;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelRect = position;
            labelRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);
            if (!property.isExpanded) return;

            var clips = property.FindPropertyRelative("clips");

            var baseHeight = (clips.arraySize + 3) * EditorGUIUtility.singleLineHeight;
            var baseRect = position;
            baseRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            baseRect.height = baseHeight;

            PrepareClipsViewer(clips, property);
            _clipsViewer.DoList(baseRect);
        }

        EditorTools.PrettyListViewer _clipsViewer;
        EditorTools.SerializedPropertyPopupContent _clipPopup = new EditorTools.SerializedPropertyPopupContent();
        bool _drawCurve;
        float HeaderHeight
        {
            get
            {
                var ret = EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
                if (_drawCurve) ret += 100;
                return ret;
            }
        }
        static EditorTools.CurveDrawer _curveDrawer = new EditorTools.CurveDrawer(new Rect(0, -0.5f, 1, 2));
        void PrepareClipsViewer(SerializedProperty clips, SerializedProperty property)
        {
            if (_clipsViewer == null)
            {
                _clipsViewer = new EditorTools.PrettyListViewer(
                    clips.serializedObject, clips,
                    onDrawHeader: (rect) =>
                    {
                        var sliderRect = rect;
                        sliderRect.height = EditorGUIUtility.singleLineHeight;
                        sliderRect.width -= 25;
                        DrawPreviewSlider(sliderRect, property);
                        sliderRect.x += sliderRect.width + 5;
                        sliderRect.width = 20;
                        var ndc = GUI.Toggle(sliderRect, _drawCurve, "C", EditorStyles.miniButton);
                        if (ndc != _drawCurve)
                        {
                            _drawCurve = ndc;
                            _clipsViewer.headerHeight = HeaderHeight;
                        }
                        if (!_drawCurve) return;

                        var previewRect = rect;
                        previewRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        previewRect.height -= EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
                        _curveDrawer.ViewPort = previewRect;
                        _curveDrawer.DrawRect(_curveDrawer.ValueRange, 0.05f, 0.4f);
                        _curveDrawer.DrawLine(new Vector2(0, 0), new Vector2(1, 0), Color.blue * 0.5f);
                        _curveDrawer.DrawLine(new Vector2(0, 1), new Vector2(1, 1), Color.red * 0.5f);
                        if (InPreview)
                        {
                            _curveDrawer.DrawLine(new Vector2(_previewCounter, -0.5f), new Vector2(_previewCounter, 1.5f), Color.yellow);
                        }
                        for (int i = 0; i < clips.arraySize; ++i)
                        {
                            var clip = clips.GetArrayElementAtIndex(i);
                            var startTime = clip.FindPropertyRelative("startTime").floatValue;
                            var endTime = clip.FindPropertyRelative("endTime").floatValue;
                            var profile = clip.FindPropertyRelative("profile");
                            var ease = profile.FindPropertyRelative("ease").intValue;
                            var ex = profile.FindPropertyRelative("ex").floatValue;
                            var ey = profile.FindPropertyRelative("ey").floatValue;
                            var ez = profile.FindPropertyRelative("ez").floatValue;
                            var ep = new EaseProfile((Easing)ease, ex, ey, ez);
                            var iF = (float)i / clips.arraySize;
                            var color = new Color(iF, Mathf.Abs(iF - 0.5f), 1 - iF, 0.6f);
                            if (i == _clipsViewer.index) color = Color.white;
                            _curveDrawer.DrawCurve(
                                x =>
                                {
                                    var x1 = (x - startTime) / (endTime - startTime);
                                    if (x1 < 0) x1 = 0;
                                    else if (x1 < 1) { }
                                    else x1 = 1;
                                    return ep.Ease(x1);
                                }, color
                            );
                        }
                    },
                    onDrawElement: (rect, index, isActive, isFocused) =>
                    {
                        var boxRect = rect;
                        boxRect.width = 20;
                        var oc = GUI.color;
                        var iF = (float)index / _clipsViewer.count;
                        GUI.color = new Color(iF, Mathf.Abs(iF - 0.5f), 1 - iF, 0.6f);
                        GUI.Box(boxRect, "");
                        GUI.color = oc;

                        rect.x += 20;
                        rect.width -= 20;
                        var child = clips.GetArrayElementAtIndex(index);

                        if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
                        {
                            _clipPopup.Show(child, rect);
                            Event.current.Use();
                        }

                        rect.width *= 0.5f;
                        var childContext = child.FindPropertyRelative("context");
                        var childPath = child.FindPropertyRelative("propertyPath");
                        EditorTools.InspectorUtils.PinLabel(rect, childContext.objectReferenceValue);
                        rect.x += rect.width;
                        GUI.Label(rect, childPath.stringValue);

                    },
                    elementHeightGetter: (index) =>
                    {
                        return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    },
                    onReorder: (list) =>
                    {
                        list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    },
                    onAdd: (list) =>
                    {
                        list.serializedProperty.InsertArrayElementAtIndex(list.serializedProperty.arraySize);
                        list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    },
                    onRemove: (list) =>
                    {
                        list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                        list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                );
                _clipsViewer.headerHeight = HeaderHeight;
            }
        }


        void DrawPreviewSlider(Rect ctrlRect, SerializedProperty property)
        {
            if (!property.isExpanded) return;
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EnterPreview(null);
                EditorGUI.LabelField(ctrlRect, "Preview Disabled for Multiple Targets", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var durationRect = ctrlRect;
            durationRect.width = 30;
            var duration = property.FindPropertyRelative("duration");
            EditorGUI.PropertyField(durationRect, duration, GUIContent.none);

            var btnRect = ctrlRect;
            btnRect.width = 20f;
            btnRect.x += 35;

            var inPreview = InPreview;
            var nip = GUI.Toggle(btnRect, inPreview, "P", EditorStyles.miniButton);
            if (nip != inPreview)
            {
                if (nip)
                {
                    EnterPreview(property);
                }
                else
                {
                    EnterPreview(null);
                }
            }

            if (_pSp != null && (_pSp.serializedObject.targetObject != property.serializedObject.targetObject || _pSp.propertyPath != property.propertyPath))
            {
                EditorGUI.LabelField(ctrlRect, "Previewing Other Stuff...", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            btnRect.x += 25;
            if (!inPreview) return;

            _previewPlaying = GUI.Toggle(btnRect, _previewPlaying, "▶", EditorStyles.miniButton);

            btnRect.x += 25;
            ctrlRect.width -= btnRect.x - ctrlRect.x;
            ctrlRect.x = btnRect.x;
            var newCounter = EditorGUI.Slider(ctrlRect, _previewCounter, 0, 1);
            if (newCounter != _previewCounter || _previewPlaying)
            {
                _previewCounter = newCounter;
                if (_previewCounter >= 1 && Event.current.type == EventType.Repaint)
                {
                    _previewCounter = 1;
                    _previewPlaying = false;
                    _previewJustDone = true;
                }
                EvaluateClips();
            }


        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // var ret = EditorGUI.GetPropertyHeight(property);
            var ret = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                var clips = property.FindPropertyRelative("clips");
                var baseHeight = (clips.arraySize + 2) * EditorGUIUtility.singleLineHeight;
                ret += baseHeight + HeaderHeight;
            }
            return ret;
        }

        struct ClipCache
        {
            public TweenerTrack.Clip clip;
            public Vector4 value;
        }

        static List<ClipCache> _previewing = new List<ClipCache>();
        static bool _previewJustDone;
        static SerializedProperty _pSp;
        static SerializedObject _pSo;
        static float _previewDuration;
        static float _previewCounter;
        static bool InPreview
        {
            get
            {
                return _pSp != null;
            }
        }

        internal static void EnterPreview(SerializedProperty track)
        {
            // Debug.Log("Enter preview");
            // restore old
            foreach (var cache in _previewing)
            {
                if (cache.clip.context == null) continue;
                if (cache.clip._setter == null) continue;
                cache.clip._setter(cache.value);
            }
            _previewing.Clear();

            // enter new
            if (_pSo != null) _pSo.Dispose();
            _pSp = track;
            _previewCounter = 0;
            _previewPlaying = false;
            if (track == null) return;

            _pSo = new SerializedObject(_pSp.serializedObject.targetObject);
            _pSp = _pSo.FindProperty(_pSp.propertyPath);

            var clipsSp = track.FindPropertyRelative("clips");

            for (int i = 0; i < clipsSp.arraySize; ++i)
            {
                var clip = TweenerTrackClipDrawer.Get(clipsSp.GetArrayElementAtIndex(i));
                TweenerTrack.Clip.Init(ref clip);
                _previewing.Add(new ClipCache
                {
                    clip = clip,
                    value = clip._getter()
                });
            }

            EvaluateClips();
            _previewDuration = track.FindPropertyRelative("duration").floatValue;
            _lastUpdate = EditorApplication.timeSinceStartup;
        }

        static void EvaluateClips()
        {
            foreach (var p in _previewing)
            {
                p.clip.Evaluate(_previewCounter);
            }
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        [InitializeOnLoadMethod]
        static void SetupUpdateAndReloadMethods()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAsmReload;
            EditorApplication.update += OnEditorUpdate;
        }

        static void OnBeforeAsmReload()
        {
            EnterPreview(null);
        }

        static double _lastUpdate; // emm
        static bool _previewPlaying;
        static float _unscaledPreviewCounter;
        static void OnEditorUpdate()
        {
            if (!InPreview) return;
            if (_previewJustDone)
            {
                SceneView.RepaintAll();
                _previewJustDone = false;
            }
            if (_pSp.serializedObject.targetObject == null)
            {
                _pSp = null;
                EnterPreview(null);
            }

            var rt = EditorApplication.timeSinceStartup;
            if (!_previewPlaying)
            {
                _lastUpdate = rt; // time lost
                return;
            }
            if (rt - _lastUpdate < 0.01f) return;
            var dt = (float)(rt - _lastUpdate);
            _lastUpdate = rt;
            var scaledDt = _previewDuration == 0 ? 0 : dt / _previewDuration;
            _previewCounter += scaledDt;

            // EvaluateClips();
            // Debug.Log("Update");
            EditorApplication.QueuePlayerLoopUpdate();
            Luxko.EditorTools.InspectorUtils.RepaintAllInspectors();
        }
    }

    public class FileModificationWarning : UnityEditor.AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            TweenTrackDrawer.EnterPreview(null);
            return paths;
        }
    }
#endif

}
