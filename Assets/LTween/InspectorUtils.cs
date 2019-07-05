# if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace Luxko.EditorTools
{
    public static class InspectorUtils
    {
        static MethodInfo _renderSpriteMi;
        static object[] _renderSpriteParams;
        static PropertyInfo _palettesProperty;
        static Type _paletteWindowType;
        static public Type PaletteWindowType { get { return _paletteWindowType; } }
        static MethodInfo _paletteSelector;
        static MethodInfo _paletteSaver;
        static PropertyInfo _activePaletteInstance;
        static EditorWindow _paletteWindow;

        static Type _tilemapFocusType;
        static PropertyInfo _tilemapFocus;
        static SceneView.OnSceneFunc _paletteWindowOnGui;
        static MethodInfo _paletteWindowDisableFocus;
        static MethodInfo _paletteWindowOnPaintTargetChanged;

        static Type _gridPaintingState;
        static PropertyInfo _gridPaintingStateTarget;
        static FieldInfo _gridPaintingStateInstance;
        static FieldInfo _gridPaintingStateFlushTarget;
        public delegate void GridMarqueeDrawer(GridLayout gridLayout, BoundsInt area, Color color);
        public static readonly GridMarqueeDrawer DrawGridMarquee;

        public delegate GameObject GoPicker(Vector2 pos, bool selectePrefabRoot, GameObject[] ignore, GameObject[] filter);

        public static readonly GoPicker PickGameObject;
        public static readonly Action RepaintAllInspectors;

        static InspectorUtils()
        {
            InitRenderSpriteMi();
            InitGridPaletteMi();
            var gridEditorUtility = GetType("UnityEditorInternal.GridEditorUtility", "UnityEditor");
            var drawGridMarqueeMi = gridEditorUtility.GetMethod("DrawGridMarquee", BindingFlags.Static | BindingFlags.Public);
            DrawGridMarquee = Delegate.CreateDelegate(typeof(GridMarqueeDrawer), null, drawGridMarqueeMi) as GridMarqueeDrawer;
            InitGridPaintingState();
            InitSceneViewOverlay();
            {
                var teus = GetType("UnityEditor.TilemapEditorUserSettings");
                _tilemapFocusType = GetType("UnityEditor.TilemapEditorUserSettings+FocusMode");
                _tilemapFocus = teus.GetProperty("focusMode");
            }

            {
                var thu = GetType("UnityEditor.HandleUtility");
                var mi = thu.GetMethod("PickGameObject", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Vector2), typeof(bool), typeof(GameObject[]), typeof(GameObject[]) }, null);
                PickGameObject = Delegate.CreateDelegate(typeof(GoPicker), null, mi) as GoPicker;
            }

            {
                var tiw = GetType("UnityEditor.InspectorWindow");
                var raiInfo = tiw.GetMethod("RepaintAllInspectors", BindingFlags.NonPublic | BindingFlags.Static);
                RepaintAllInspectors = Delegate.CreateDelegate(typeof(Action), null, raiInfo) as Action;
            }
        }

        public delegate void SceneViewOverlayContentFunction(UnityEngine.Object target, SceneView sceneView);
        public static void SceneViewOverlayWindow(
            GUIContent title,
            SceneViewOverlayContentFunction contentFunc,
            int order,
            UnityEngine.Object target
        )
        {
            _svoMiParams[0] = title;
            _svoMiParams[1] = Delegate.CreateDelegate(_svoWfType, contentFunc.Target, contentFunc.Method);
            _svoMiParams[2] = order;
            _svoMiParams[3] = target;
            _svoMi.Invoke(null, _svoMiParams);
        }

        static object[] _svoMiParams = new object[5];
        static MethodInfo _svoMi;
        static Type _svoWfType;

        static void InitSceneViewOverlay()
        {
            var svoType = GetType("UnityEditor.SceneViewOverlay");
            _svoWfType = GetType("UnityEditor.SceneViewOverlay+WindowFunction");
            var svoWdoType = GetType("UnityEditor.SceneViewOverlay+WindowDisplayOption");
            _svoMiParams[4] = Enum.GetValues(svoWdoType).GetValue(0);
            var wfParamTypes = new Type[] { typeof(GUIContent), _svoWfType, typeof(int), typeof(UnityEngine.Object), svoWdoType };
            _svoMi = svoType.GetMethod("Window", BindingFlags.Static | BindingFlags.Public, null, wfParamTypes, null);
        }

        static void InitGridPaintingState()
        {
            _gridPaintingState = InspectorUtils.GetType("UnityEditor.GridPaintingState");
            if (_gridPaintingState == null) return;
            _gridPaintingStateInstance = _gridPaintingState.BaseType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);
            _gridPaintingStateFlushTarget = _gridPaintingState.GetField("m_FlushPaintTargetCache", BindingFlags.Instance | BindingFlags.NonPublic);
            _gridPaintingStateTarget = _gridPaintingState.GetProperty("scenePaintTarget");
        }

        static void InitPaletteWindowProps()
        {
            if (_paletteWindowOnGui == null)
            {
                _paletteWindowOnGui = (SceneView.OnSceneFunc)Delegate.CreateDelegate(typeof(SceneView.OnSceneFunc), EditorWindow.GetWindow(_paletteWindowType), "OnSceneViewGUI");
            }
        }

        public static void FlushGridPaintingTarget()
        {
            var instance = _gridPaintingStateInstance.GetValue(null);
            _gridPaintingStateFlushTarget.SetValue(instance, true);
        }

        public static void SetGridPaintingTarget(GameObject go)
        {
            _gridPaintingStateTarget.SetValue(null, go, null);
        }

        public static void EnableTilemapFocus()
        {
            InitPaletteWindowProps();
            SceneView.onSceneGUIDelegate -= _paletteWindowOnGui;

        }

        public static void DisableTilemapFocus()
        {
            if (_paletteWindowOnGui != null)
            {
                SceneView.onSceneGUIDelegate += _paletteWindowOnGui;
                _paletteWindowOnGui = null;
            }
        }

        public static bool GetEditMode(EditMode.SceneViewEditMode mode)
        {
            return EditMode.editMode == mode;
        }

        public static void SetEditMode(EditMode.SceneViewEditMode mode, bool value, Editor caller)
        {
            SetEditMode(mode, value, caller, new Bounds(Vector3.negativeInfinity, Vector3.positiveInfinity));
        }

        public static void SetEditMode(EditMode.SceneViewEditMode mode, bool value, Editor caller, Bounds bound)
        {
            if (value)
            {
                EditMode.ChangeEditMode(mode, bound, caller);
            }
            else
            {
                EditMode.ChangeEditMode(EditMode.SceneViewEditMode.None, bound, caller);
            }
        }

        public enum TilemapFocusMode
        {
            None = 0,
            Tilemap = 1,
            Grid = 2,
        }

        static TilemapFocusMode _tmFocusMode;

        public static TilemapFocusMode TilemapFocus
        {
            get { return _tmFocusMode; }
            set
            {
                EnableTilemapFocus();
                var focusMode = System.Enum.ToObject(_tilemapFocusType, (int)value);
                _paletteWindowDisableFocus.Invoke(EditorWindow.GetWindow(_paletteWindowType), null);
                _tilemapFocus.SetValue(null, focusMode, null);
                _paletteWindowOnPaintTargetChanged.Invoke(EditorWindow.GetWindow(_paletteWindowType), new object[] { null });
                _tmFocusMode = value;
                SceneView.RepaintAll();
            }
        }

        static void InitRenderSpriteMi()
        {
            var t = GetType("UnityEditor.SpriteUtility");
            if (t == null) return;
            _renderSpriteMi = t.GetMethod("RenderStaticPreview", new Type[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int), typeof(Matrix4x4) });
            _renderSpriteParams = new object[5];
        }

        static void InitGridPaletteMi()
        {
            var gpt = GetType("UnityEditor.GridPalettes");
            if (gpt == null) return;
            _palettesProperty = gpt.GetProperty("palettes");
            if (_palettesProperty == null) return;

            _paletteWindowType = GetType("UnityEditor.GridPaintPaletteWindow");
            if (_paletteWindowType == null) return;
            _paletteSelector = _paletteWindowType.GetMethod("SelectPalette", BindingFlags.Instance | BindingFlags.NonPublic, null, new System.Type[] { typeof(int), typeof(object) }, null);
            _paletteSaver = _paletteWindowType.GetMethod("SavePalette", BindingFlags.Instance | BindingFlags.Public);
            _activePaletteInstance = _paletteWindowType.GetProperty("paletteInstance");
            _paletteWindowDisableFocus = _paletteWindowType.GetMethod("DisableFocus", BindingFlags.Instance | BindingFlags.NonPublic);
            _paletteWindowOnPaintTargetChanged = _paletteWindowType.GetMethod("OnScenePaintTargetChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static GameObject GetActiveTilePalette()
        {
            if (_paletteWindow == null)
            {
                _paletteWindow = EditorWindow.GetWindow(_paletteWindowType);
            }
            return _activePaletteInstance.GetGetMethod().Invoke(_paletteWindow, new object[0]) as GameObject;
        }

        public static void SelectTilePalette(GameObject palette)
        {
            if (_palettesProperty == null || _paletteSelector == null) InitGridPaletteMi();
            var palettes = _palettesProperty.GetValue(null, null) as List<GameObject>;
            if (palettes == null) return;
            var foundIdx = palettes.FindIndex(p => p == palette);
            if (foundIdx == -1) return;
            if (_paletteWindow == null)
            {
                _paletteWindow = EditorWindow.GetWindow(_paletteWindowType);
            }
            _paletteSelector.Invoke(_paletteWindow, new object[] { foundIdx, null });
        }

        public static void SaveSelectedPalette()
        {
            if (_paletteWindow == null)
            {
                _paletteWindow = EditorWindow.GetWindow(_paletteWindowType);
            }
            _paletteSaver.Invoke(_paletteWindow, new object[0]);
        }

        public static List<GameObject> GetAllPalette()
        {
            if (_palettesProperty == null || _paletteSelector == null) InitGridPaletteMi();
            return _palettesProperty.GetValue(null, null) as List<GameObject>;
        }

        public static Texture2D RenderStaticPreview(Sprite sprite, Color color, int width, int height, Matrix4x4 matrix)
        {
            if (_renderSpriteMi == null) InitRenderSpriteMi();
            _renderSpriteParams[0] = sprite;
            _renderSpriteParams[1] = color;
            _renderSpriteParams[2] = width;
            _renderSpriteParams[3] = height;
            _renderSpriteParams[4] = matrix;
            return _renderSpriteMi.Invoke("RenderStaticPreview", _renderSpriteParams) as Texture2D;
        }

        public static Type GetType(string TypeName)
        {
            var type = Type.GetType(TypeName);
            if (type != null) return type;

            if (TypeName.Contains("."))
            {
                var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
                type = GetType(TypeName, assemblyName);
                if (type != null) return type;
            }

            // var currentAssembly = Assembly.GetEntryAssembly();
            // var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            // foreach (var assemblyName in referencedAssemblies)
            // {
            //     var assembly = Assembly.Load(assemblyName);
            //     if (assembly != null)
            //     {
            //         type = assembly.GetType(TypeName);
            //         if (type != null)
            //             return type;
            //     }
            // }
            var referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in referencedAssemblies)
            {
                if (assembly != null)
                {
                    type = assembly.GetType(TypeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        public static Type GetType(string typeName, string assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null) return null;
            var type = assembly.GetType(typeName);
            return type;
        }

        public static int GetKeyCodeNumber(this Event e, bool includeAlpha = true, bool includeNumpad = true)
        {
            if (includeAlpha)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Alpha0:
                        return 0;
                    case KeyCode.Alpha1:
                        return 1;
                    case KeyCode.Alpha2:
                        return 2;
                    case KeyCode.Alpha3:
                        return 3;
                    case KeyCode.Alpha4:
                        return 4;
                    case KeyCode.Alpha5:
                        return 5;
                    case KeyCode.Alpha6:
                        return 6;
                    case KeyCode.Alpha7:
                        return 7;
                    case KeyCode.Alpha8:
                        return 8;
                    case KeyCode.Alpha9:
                        return 9;
                    default:
                        break;
                }
            }
            if (includeNumpad)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Keypad0:
                        return 0;
                    case KeyCode.Keypad1:
                        return 1;
                    case KeyCode.Keypad2:
                        return 2;
                    case KeyCode.Keypad3:
                        return 3;
                    case KeyCode.Keypad4:
                        return 4;
                    case KeyCode.Keypad5:
                        return 5;
                    case KeyCode.Keypad6:
                        return 6;
                    case KeyCode.Keypad7:
                        return 7;
                    case KeyCode.Keypad8:
                        return 8;
                    case KeyCode.Keypad9:
                        return 9;
                    default:
                        break;
                }
            }
            return -1;
        }

        /// Draw a sprite in screen space
        /// Should be invoked inside OnGUI upon repaint event
        /// in a 2D GUI block
        public static void DrawSprite(Rect screenPos, Sprite sprite, Color color, Material mat = null, int pass = -1)
        {
            var spriteRect = sprite.rect;
            var texRect = new Rect(
                spriteRect.x / sprite.texture.width,
                spriteRect.y / sprite.texture.height,
                spriteRect.width / sprite.texture.width,
                spriteRect.height / sprite.texture.height
            );

            Graphics.DrawTexture(screenPos, sprite.texture, texRect, 0, 0, 0, 0, color, mat, pass);
        }

        /// Draw a sprite in normalized screen space
        /// Should be invoked inside OnGUI upon repaint event
        /// in a 2D GUI blocks
        public static void DrawSpriteNorm(Rect screenPosNorm, Sprite sprite, Color color, Material mat = null, int pass = -1)
        {
            var cam = Camera.current;
            if (cam == null) return;
            screenPosNorm.x *= cam.pixelWidth;
            screenPosNorm.y *= cam.pixelHeight;
            screenPosNorm.width *= cam.pixelWidth;
            screenPosNorm.height *= cam.pixelHeight;
            DrawSprite(screenPosNorm, sprite, color, mat, pass);
        }

        public static void CircleHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            Handles.CircleHandleCap(controlID, position, rotation, size, eventType);
            switch (eventType)
            {
                case (EventType.Repaint):
                    // TODO: draw a ...
                    var forward = rotation * new Vector3(0, 0, 1);
                    Handles.DrawSolidDisc(position, forward, size);
                    break;
            }
        }

        public enum DrawLineConfig
        {
            NotShown = 0,
            Shown = 1,
            SymmetricShown = 2,
            ShownDotted = 3,
            SymmetricShownDotted = 4,
        }

        public static void DrawLine(Vector3 from, Vector3 to, DrawLineConfig config)
        {
            switch (config)
            {
                case DrawLineConfig.Shown:
                    Handles.DrawLine(from, to); break;
                case DrawLineConfig.SymmetricShown:
                    Handles.DrawLine(2 * from - to, to); break;
                case DrawLineConfig.ShownDotted:
                    Handles.DrawDottedLine(from, to, 3f); break;
                case DrawLineConfig.SymmetricShownDotted:
                    Handles.DrawDottedLine(2 * from - to, to, 3f); break;
            }
        }

        public static float DistanceDragHandle(float distance, Vector3 position, Vector3 direction, int controlID, GUIContent label, float size = 0.15f, float snap = 0, GUIStyle labelStyle = null, DrawLineConfig lineConfigWhenNear = DrawLineConfig.Shown, DrawLineConfig lineConfigWhenNotNear = DrawLineConfig.ShownDotted)
        {
            var handlePos = position + direction * distance; // TODO: normalize direction?
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    if (HandleUtility.nearestControl == controlID)
                    {
                        DrawLine(position, handlePos, lineConfigWhenNear);
                    }
                    else
                    {
                        DrawLine(position, handlePos, lineConfigWhenNotNear);
                    }

                    if (label != null)
                    {
                        if (labelStyle == null) labelStyle = GetDefaultHandleLabelStyle();
                        if (HandleUtility.nearestControl == controlID)
                        {
                            Handles.Label(handlePos - new Vector3(size, size * 1.25f, 0), label, labelStyle);
                        }
                    }
                    break;
            }
            var newHandle = Handles.FreeMoveHandle(controlID, handlePos, Quaternion.identity, size, new Vector3(snap, snap, snap), Handles.CubeHandleCap);
            GUI.changed = false;
            if (newHandle != handlePos)
            {
                var newDif = newHandle - position;
                // var oldDif = handlePos - position;
                // newHandle =  * direction + position;
                GUI.changed = true;
                var newDis = Vector3.Dot(newDif, direction); // ?
                return newDis;
            }
            return distance;
        }

        public static float RadiusDragHandle(float radius, Vector3 position, int controlId, string label)
        {
            switch (Event.current.GetTypeForControl(controlId))
            {
                case EventType.Layout:
                    {
                        var origin = HandleUtility.WorldToGUIPoint(position);
                        var worldRadius = radius;
                        var currentCam = Camera.current;
                        if (currentCam != null)
                        {
                            // align radius towards camera
                            var edgePoint = HandleUtility.WorldToGUIPoint(position + currentCam.transform.right * radius);
                            worldRadius = (origin - edgePoint).magnitude;
                        }
                        var dFromOrigin = (origin - Event.current.mousePosition).magnitude;

                        var distance = Mathf.Abs(dFromOrigin - worldRadius);
                        HandleUtility.AddControl(controlId, distance);
                    }
                    break;
                case EventType.MouseDown:
                    if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controlId)
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUI.changed = true;
                        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        var normal = new Vector3(0, 0, 1);
                        var currentCam = Camera.current;
                        if (currentCam != null)
                        {
                            normal = currentCam.transform.forward;
                        }
                        var plane = new Plane(normal, position);
                        float distanceAlongRay;
                        plane.Raycast(ray, out distanceAlongRay);
                        radius = (ray.GetPoint(distanceAlongRay) - position).magnitude;
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;
                case EventType.Repaint:
                    {
                        var oldColor = Handles.color;
                        var normal = new Vector3(0, 0, 1);
                        var currentCam = Camera.current;
                        if (currentCam != null)
                        {
                            normal = currentCam.transform.forward;
                        }
                        if (HandleUtility.nearestControl == controlId)
                        {
                            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                            var plane = new Plane(normal, position);
                            float distanceAlongRay;
                            plane.Raycast(ray, out distanceAlongRay);
                            Handles.Label(ray.GetPoint(distanceAlongRay) + new Vector3(1, 0, 0), label, GetDefaultHandleLabelStyle());
                            Handles.color = GUIUtility.hotControl == controlId ? Handles.selectedColor : Handles.preselectionColor;
                            Handles.DrawWireDisc(position, normal, radius);
                        }
                        else
                        {
                            Handles.color = Color.white;
                            Handles.DrawWireDisc(position, normal, radius);
                        }
                        Handles.color = oldColor;
                    }
                    break;
            }
            return radius;
        }

        public static void OffsetDragHandle(ref Vector3 offset, Component parent, int controlID, string name, float size = 0.1f, Vector3 snap = new Vector3(), bool offsetByAdd = true, GUIStyle style = null, Handles.CapFunction capHandle = null)
        {

            Vector3 pos;
            if (offsetByAdd)
            {
                pos = parent.transform.position + offset;
            }
            else
            {
                pos = parent.transform.TransformPoint(offset);
            }
            if (capHandle == null) capHandle = Handles.CircleHandleCap;
            var newPos = Luxko.EditorTools.InspectorUtils.FreeMoveHandle(controlID, pos, size, snap, capHandle, name, style);
            if (newPos == pos) return;
            Vector3 newOffset;
            if (offsetByAdd)
            {
                newOffset = newPos - parent.transform.position;
            }
            else
            {
                newOffset = parent.transform.InverseTransformPoint(newPos);
            }
            Undo.RecordObject(parent, "Offset Dragging");
            offset = newOffset;
        }

        public static Vector3 FreeMoveHandle(int controlID, Vector3 position, float size, Vector3 snap, Handles.CapFunction capFunction, string label, GUIStyle labelStyle = null)
        {
            _tempTextLabel.text = label;
            return FreeMoveHandle(controlID, position, size, snap, capFunction, _tempTextLabel, labelStyle);
        }

        public static GUIStyle GetDefaultHandleLabelStyle()
        {
            if (_defaultHandleLabelStyle == null)
            {
                _defaultHandleLabelStyle = new GUIStyle(EditorStyles.label);
                _defaultHandleLabelStyle.active.textColor = new Color(1, 1, 0.25f);
                _defaultHandleLabelStyle.normal.textColor = new Color(1, 1, 0.25f);
            }
            return _defaultHandleLabelStyle;
        }

        public static Vector3 FreeMoveHandle(int controlID, Vector3 position, float size, Vector3 snap, Handles.CapFunction capFunction, GUIContent label = null, GUIStyle labelStyle = null)
        {
            if (label != null)
            {
                if (labelStyle == null)
                {
                    labelStyle = GetDefaultHandleLabelStyle();
                }
                switch (Event.current.type)
                {
                    case EventType.Repaint:
                        if (HandleUtility.nearestControl == controlID)
                        {
                            Handles.Label(position - new Vector3(size, size * 1.25f, 0), label, labelStyle);
                        }
                        break;
                }
            }
            return Handles.FreeMoveHandle(controlID, position, Quaternion.identity, size, snap, capFunction);
        }

        static readonly int s_defaultControlIdHint = "LuxkoFreeMoveHandles".GetHashCode();
        public delegate void CustomMoveHandler<T>(ref T points, int idx, int controlId);
        public static void FreeMoveHandles<T>(T points, float handleSize = 0.1f, int controlIdHint = -1, CustomMoveHandler<T> customMoveHandler = null, Transform parent = null) where T : IList<Vector3>
        {
            if (controlIdHint == -1) controlIdHint = s_defaultControlIdHint;

            // insert new points
            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
            {
                Event.current.Use();

                var wr = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                var plane = new Plane(new Vector3(0, 0, 1), new Vector3(0, 0, 0));
                float enter;
                plane.Raycast(wr, out enter);
                var newPoint = wr.GetPoint(enter);
                if (parent != null) newPoint = parent.InverseTransformPoint(newPoint);
                points.Add(newPoint);
                GUI.changed = true;
            }

            for (int i = 0; i < points.Count; ++i)
            {
                var controlId = GUIUtility.GetControlID(controlIdHint, FocusType.Passive);

                if (customMoveHandler != null)
                {
                    customMoveHandler(ref points, i, controlId);
                }

                var pos = points[i];
                if (parent != null) pos = parent.TransformPoint(pos);

                var newPos = Luxko.EditorTools.InspectorUtils.FreeMoveHandle(controlId, pos, handleSize, new Vector3(), Handles.CircleHandleCap, i.ToString());

                if (newPos != pos)
                {
                    if (parent != null) newPos = parent.InverseTransformPoint(newPos);
                    points[i] = newPos;
                    GUI.changed = true;
                }

                // remove
                if (Event.current.shift && Event.current.type == EventType.MouseDown && Event.current.button == 1 && HandleUtility.nearestControl == controlId)
                {
                    Event.current.Use();
                    points.RemoveAt(i);
                    i--;
                    GUI.changed = true;
                }
            }
        }

        static readonly GUIContent _tempTextLabel = new GUIContent();
        static GUIStyle _defaultHandleLabelStyle;

        public static void GatherAllProperties<T>(UnityEngine.Timeline.IPropertyCollector driver, T component) where T : Component
        {
            var so = new SerializedObject(component);
            var iterator = so.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.hasVisibleChildren)
                    continue;

                driver.AddFromName<T>(component.gameObject, iterator.propertyPath);
            }
        }

        public static void LastLayoutHeight(ref float output, float margin = 0, float clampMin = 0, float clampMax = float.MaxValue)
        {
            if (Event.current.type != EventType.Repaint) return;
            var lastRect = GUILayoutUtility.GetLastRect();
            var yMax = lastRect.yMax + margin;
            output = Mathf.Clamp(yMax, clampMin, clampMax);
        }

        public static void LastLayoutRect(ref Rect output)
        {
            if (Event.current.type != EventType.Repaint) return;
            output = GUILayoutUtility.GetLastRect();
        }

        public static void ActuallyRemoveArrayElementAt(this SerializedProperty sp, int i)
        {
            var toRemove = sp.GetArrayElementAtIndex(i);
            if (toRemove.propertyType == SerializedPropertyType.ObjectReference)
            {
                toRemove.objectReferenceValue = null;
            }
            sp.DeleteArrayElementAtIndex(i);
        }

        static string[] sortingLayerNames;
        // static int[] sortingLayerIdIndices;
        static int[] sortingLayerIds;

        public static int SortingLayerPopup(Rect position, int layerId)
        {
            var layers = SortingLayer.layers;
            if (sortingLayerIds == null || sortingLayerIds.Length != layers.Length)
            {
                Array.Resize(ref sortingLayerNames, layers.Length);
                // Array.Resize(ref sortingLayerIdIndices, layers.Length);
                Array.Resize(ref sortingLayerIds, layers.Length);

                for (int i = 0; i < layers.Length; ++i)
                {
                    sortingLayerNames[i] = layers[i].name;
                    sortingLayerIds[i] = layers[i].id;
                    // sortingLayerIdIndices[i] = i;
                }
            }

            return EditorGUI.IntPopup(position, layerId, sortingLayerNames, sortingLayerIds);
        }

        public static void PinLabel(Rect position, UnityEngine.Object toPin, GUIStyle style = null)
        {
            if (style == null) style = EditorStyles.objectField;
            if (toPin != null && Event.current.type == EventType.MouseDown && Event.current.button == 0 && position.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                EditorGUIUtility.PingObject(toPin);
            }
            GUI.Label(position, toPin == null ? "None" : string.Format("{0}({1})", toPin.name, toPin.GetType().Name), style);
        }

        static string[] _layerNames;
        public static int AnimatorControllerLayerPopup(Rect? position, int layerIndex, UnityEditor.Animations.AnimatorController controller)
        {
            if (controller == null)
            {
                if (position.HasValue)
                {
                    GUI.Label(position.Value, "None");
                }
                else
                {
                    GUILayout.Label("None");
                }
                return layerIndex;
            }
            if (_layerNames == null || _layerNames.Length < controller.layers.Length)
            {
                System.Array.Resize(ref _layerNames, controller.layers.Length);
            }
            for (int i = 0; i < controller.layers.Length; ++i)
            {
                _layerNames[i] = controller.layers[i].name;
            }
            for (int i = controller.layers.Length; i < _layerNames.Length; ++i)
            {
                _layerNames[i] = "/"; // this hides the option. YEAH!
            }
            if (position.HasValue)
            {
                return EditorGUI.Popup(position.Value, layerIndex, _layerNames);
            }
            else
            {
                return EditorGUILayout.Popup(layerIndex, _layerNames);
            }
        }
    }
}
#endif
