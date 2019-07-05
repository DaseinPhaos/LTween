#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Luxko.EditorTools
{
    public abstract class EditorPopupContent
    {
        protected EditorPopupContent() { }
        public virtual Vector2 GetWindowSize() { return new Vector2(200, 200); }
        public virtual void OnOpen() { }
        public virtual void OnClose() { }
        public abstract void OnGUI(Rect rect);

        public void Show(Rect activatorRect)
        {
            _activatorRect = activatorRect;
            EditorPopupHolderWindow.Show(this);
        }

        public void Close()
        {
            EditorPopupHolderWindow.Close(this);
        }

        protected void Repaint()
        {
            EditorPopupHolderWindow.RepaintInst();
        }

        internal Vector2 _lastSize;
        internal Rect _activatorRect;
        internal Rect _position;
    }

    public class EditorPopupHolderWindow : EditorWindow
    {
        static EditorPopupHolderWindow _inst;
        // static MethodInfo _ShowAsDropDownFitToScreen;
        static Texture2D _texBuf;
        static EditorWindow[] _bgWindows;
        static Rect _coveredResolution;
        internal static void Show(EditorPopupContent windowContent)
        {
            if (_inst == null)
            {
                _inst = ScriptableObject.CreateInstance<EditorPopupHolderWindow>();
                // _ShowAsDropDownFitToScreen = typeof(EditorWindow).GetMethod("ShowAsDropDownFitToScreen", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (_inst._contents.Count == 0)
            {
                _coveredResolution = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
                _bgWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var window in _bgWindows)
                {
                    // Debug.LogFormat("{0} {1}", window.GetType().Name, window.name);
                    if (_inst == window) continue;
                    _coveredResolution.xMin = Mathf.Min(_coveredResolution.xMin, window.position.xMin);
                    _coveredResolution.yMin = Mathf.Min(_coveredResolution.yMin, window.position.yMin);
                    _coveredResolution.xMax = Mathf.Max(_coveredResolution.xMax, window.position.xMax);
                    _coveredResolution.yMax = Mathf.Max(_coveredResolution.yMax, window.position.yMax);
                }
                // Debug.Log(res);

                if (_texBuf == null || _texBuf.width != _coveredResolution.width || _texBuf.height != _coveredResolution.height)
                {
                    _texBuf = new Texture2D((int)_coveredResolution.width, (int)_coveredResolution.height);
                    _texBuf.hideFlags = HideFlags.DontSave;
                }

                var pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(_coveredResolution.position, (int)_coveredResolution.width, (int)_coveredResolution.height);
                _texBuf.SetPixels(pixels);
                _texBuf.Apply();
                // var rect = new Rect(0, 0, res.width, res.height);
                var rect = _coveredResolution;
                // rect.position = GUIUtility.GUIToScreenPoint(rect.position);
                _inst.minSize = new Vector2(rect.width, rect.height);
                _inst.maxSize = new Vector2(rect.width, rect.height);
                _inst.position = rect;
            }

            // _inst.ShowPopup();
            _inst.Add(windowContent);
            if (focusedWindow != _inst)
            {
                _inst.Focus();
            }
            GUIUtility.ExitGUI();
        }

        internal static void Close(EditorPopupContent windowContent)
        {
            if (_inst == null) return;
            _inst._removeBuf.Add(windowContent);
        }

        internal static void RepaintInst()
        {
            if (_inst == null) return;
            _inst.Repaint();
        }

        static Rect ScreenToGUIRect(Rect rect)
        {
            var vector = GUIUtility.ScreenToGUIPoint(new Vector2(rect.x, rect.y));
            rect.x = vector.x;
            rect.y = vector.y;
            return rect;
        }

        static Rect GUIToScreenRect(Rect rect)
        {
            Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
            rect.x = vector.x;
            rect.y = vector.y;
            return rect;
        }

        void OnFocus()
        {
            // Debug.Log("Popup window focused");
            // Repaint();
            ShowPopup();
        }

        void OnLostFocus()
        {
            // Debug.Log("Popup window focus lost");
            Repaint(); // we trigger repaint to regain focus (if needed)
        }

        EditorWindow _validPopup;

        void OnGUI()
        {
            if (_contents == null || _contents.Count == 0)
            {
                Close();
                GUIUtility.ExitGUI();
            }
            if (focusedWindow != this && focusedWindow != _validPopup)
            {
                bool focusedOnBg = false;
                foreach (var bg in _bgWindows)
                {
                    if (bg == focusedWindow)
                    {
                        Focus();
                        focusedOnBg = true;
                        break;
                    }
                }
                if (!focusedOnBg)
                {
                    _validPopup = focusedWindow;
                }
            }
            if (focusedWindow == null)
            {
                Focus();
            }
            // TODO: detect out of border mouse down events
            var oldColor = GUI.color;
            GUI.color *= 0.75f;
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), _texBuf);
            GUI.color = oldColor;
            if (GUIUtility.hotControl == 0 && Event.current.type == EventType.MouseDown)
            {
                // var mp = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                var mp = Event.current.mousePosition;
                bool hitFound = false;
                for (int i = _contents.Count - 1; i >= 0; --i)
                {
                    var rect = ScreenToGUIRect(_contents[i]._position);
                    if (rect.Contains(mp))
                    {
                        hitFound = true;
                        if (i + 1 < _contents.Count)
                        {
                            Remove(_contents[i + 1]);
                        }
                        break;
                    }
                }
                if (!hitFound && _contents.Count > 0)
                {
                    Remove(_contents[0]);
                }
            }

            foreach (var toRemove in _removeBuf)
            {
                Remove(toRemove);
            }
            _removeBuf.Clear();

            BeginWindows();
            for (int i = 0; i < _contents.Count; ++i)
            {
                var epc = _contents[i];
                var size = epc.GetWindowSize();
                if (epc._lastSize != size)
                {
                    epc._lastSize = size;
                    epc._position = GetPopupRect(epc._activatorRect, epc._lastSize);
                }

                var rect = ScreenToGUIRect(epc._position);
                GUI.Window(i, rect, OnDrawWindow, GUIContent.none, EditorStyles.textArea);
                // epc.OnGUI(rect);
                // GUI.Label(rect, GUIContent.none, "grey_border");
            }
            EndWindows();
        }

        void OnDrawWindow(int i)
        {
            _contents[i].OnGUI(new Rect(0, 0, _contents[i]._position.width, _contents[i]._position.height));
        }

        List<EditorPopupContent> _contents = new List<EditorPopupContent>();
        List<EditorPopupContent> _removeBuf = new List<EditorPopupContent>();

        void Add(EditorPopupContent epc)
        {
            if (_contents.Contains(epc)) return; // ?
            epc._lastSize = epc.GetWindowSize();
            epc._activatorRect = GUIToScreenRect(epc._activatorRect);
            // ar.position = GUIUtility.GUIToScreenPoint(ar.position);
            var contentRect = GetPopupRect(epc._activatorRect, epc._lastSize);
            // contentRect.position = GUIUtility.ScreenToGUIPoint(contentRect.position);
            epc._position = contentRect;
            epc.OnOpen();
            ShowPopup();
            _contents.Add(epc);
        }

        void Remove(EditorPopupContent epc)
        {
            var beforeCount = _contents.Count;
            for (int i = 0; i < _contents.Count; ++i)
            {
                if (_contents[i] == epc)
                {
                    // _contents.RemoveRange(i, _contents.Count - i);
                    for (int j = _contents.Count - 1; j >= i; --j)
                    {
                        _contents[j].OnClose();
                        _contents.RemoveAt(j);
                    }
                    break;
                }
            }

            if (_contents.Count == 0)
            {
                Close();
                GUIUtility.ExitGUI();
            }
            else if (_contents.Count != beforeCount)
            {
                Repaint();
            }
        }

        internal Rect GetPopupRect(Rect buttonRect, Vector2 size)
        {
            // // TODO: fix drop down fitting bug by our custom scheme
            // var refRect = (Rect)_ShowAsDropDownFitToScreen.Invoke(this, new object[]{
            //     buttonRect, size, null
            // });

            var ret = buttonRect;
            ret.size = size;

            if (ret.xMax > _coveredResolution.xMax)
            {
                ret.x -= ret.xMax - _coveredResolution.xMax;
            }
            if (ret.x < _coveredResolution.x)
            {
                ret.x += _coveredResolution.x - ret.x;
            }

            if (ret.y + buttonRect.height + ret.height <= _coveredResolution.yMax)
            {
                ret.y += buttonRect.height;
            }
            else
            {
                ret.y -= ret.height;
            }
            return ret;
        }

        void CloseWindow()
        {
            Close();
        }

        void OnEnable()
        {
            // TODO: should close on editor application quit.. but how
            AssemblyReloadEvents.beforeAssemblyReload += CloseWindow;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= CloseWindow;
        }
    }

    public class InspectorPopupContent : EditorPopupContent
    {
        public Editor editor;
        public float Width = 560;
        float Height = 480;
        Vector2 _scrollPos;
        public override void OnOpen()
        {
            _scrollPos = new Vector2();
        }
        public override Vector2 GetWindowSize()
        {
            return new Vector2(Width, Height);
        }

        public override void OnGUI(Rect rect)
        {
            if (editor.target == null) return;
            GUI.BeginGroup(rect);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            editor.OnInspectorGUI();
            GUILayout.EndScrollView();
            GUI.EndGroup();
        }

        public void InitInspectorWith(UnityEngine.Object obj)
        {
            if (this.editor == null || this.editor.target != obj)
            {
                this.editor = Editor.CreateEditor(obj);
            }
        }
    }

    public class InspectorUnityPwc : PopupWindowContent
    {
        public Editor editor;
        public float Width = 320;
        float Height = 320;
        Vector2 _scrollPos;
        public override void OnOpen()
        {
            _scrollPos = new Vector2();
        }
        public override Vector2 GetWindowSize()
        {
            return new Vector2(Width, Height);
        }

        public override void OnGUI(Rect rect)
        {
            if (editor.target == null) return;
            GUI.BeginGroup(rect);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            editor.OnInspectorGUI();
            var oldHeight = Height;
            InspectorUtils.LastLayoutHeight(ref Height, 12, 42, 320);
            if (oldHeight != Height)
            {
                editorWindow.Repaint();
            }
            GUILayout.EndScrollView();
            GUI.EndGroup();
        }

        public void InitInspectorWith(UnityEngine.Object obj)
        {
            if (this.editor == null || this.editor.target != obj)
            {
                this.editor = Editor.CreateEditor(obj);
            }
        }
    }

    public class SerializedPropertyPopupContent : EditorPopupContent
    {
        SerializedObject _so;
        SerializedProperty _sp;
        public float Width = 560;
        float Height = 480;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(Width, Height);
        }

        public override void OnGUI(Rect rect)
        {
            if (_sp == null || _so == null) return;
            GUI.BeginGroup(rect);

            EditorGUI.BeginChangeCheck();
            _sp.isExpanded = true;
            EditorGUILayout.PropertyField(_sp, true);
            if (EditorGUI.EndChangeCheck())
            {
                _so.ApplyModifiedProperties();
            }

            var oldHeight = Height;
            InspectorUtils.LastLayoutHeight(ref Height, 12, 42, 480);
            if (oldHeight != Height)
            {
                Repaint();
            }
            GUI.EndGroup();
        }

        public void Show(Object obj, string propertyPath, Rect activatorRect)
        {
            if (_so == null || _so.targetObject != obj)
            {
                _so = new SerializedObject(obj);
            }
            _sp = _so.FindProperty(propertyPath);
            base.Show(activatorRect);
        }

        public void Show(SerializedProperty sp, Rect activatorRect)
        {
            _so = sp.serializedObject;
            _sp = sp;
            base.Show(activatorRect);
        }
    }
}
#endif
