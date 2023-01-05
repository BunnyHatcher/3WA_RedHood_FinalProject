/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;
using System.Linq;
namespace PluginMaster
{
    #region CORE
    public static class PWBCore
    {
        public const string PARENT_COLLIDER_NAME = "PluginMasterPrefabPaintTempMeshColliders";
        private static GameObject _parentCollider = null;
        private static GameObject parentCollider
        {
            get
            {
                if (_parentCollider == null)
                {
                    _parentCollider = new GameObject(PWBCore.PARENT_COLLIDER_NAME);
                    _parentColliderId = _parentCollider.GetInstanceID();
                    _parentCollider.hideFlags = HideFlags.HideAndDontSave;
                }
                return _parentCollider;
            }
        }
        private static int _parentColliderId = -1;
        public static int parentColliderId => _parentColliderId;
        #region DATA
        private static PWBData _staticData = null;
        public static bool staticDataWasInitialized => _staticData != null;
        public static PWBData staticData
        {
            get
            {
                if (_staticData != null) return _staticData;
                _staticData = new PWBData();
                return _staticData;
            }
        }

        public static void LoadFromFile()
        {
            var text = PWBData.ReadDataText();
            if (text == null)
            {
                _staticData = new PWBData();
                _staticData.Save();
            }
            else
            {
                if (!ApplicationEventHandler.hierarchyLoaded) return;
                _staticData = JsonUtility.FromJson<PWBData>(text);
                foreach (var palette in PaletteManager.paletteData)
                    foreach (var brush in palette.brushes)
                        foreach (var item in brush.items) item.InitializeParentSettings(brush);
            }
        }

        public static void SetSavePending()
        {
            AutoSave.QuickSave();
            staticData.SetSavePending();
        }

        #endregion
        #region TEMP COLLIDERS
        private static System.Collections.Generic.Dictionary<int, GameObject> _tempCollidersIds
            = new System.Collections.Generic.Dictionary<int, GameObject>();
        private static System.Collections.Generic.Dictionary<int, GameObject> _tempCollidersTargets
            = new System.Collections.Generic.Dictionary<int, GameObject>();
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>>
            _tempCollidersTargetParentsIds
            = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>>();
        private static System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>>
            _tempCollidersTargetChildrenIds
            = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<int>>();
        public static bool CollidersContains(GameObject[] selection, string colliderName)
        {
            int objId;
            if (!int.TryParse(colliderName, out objId)) return false;
            foreach (var obj in selection)
                if (obj.GetInstanceID() == objId)
                    return true;
            return false;
        }

        public static bool IsTempCollider(int instanceId) => _tempCollidersIds.ContainsKey(instanceId);

        public static GameObject GetGameObjectFromTempColliderId(int instanceId)
        {
            if (!_tempCollidersIds.ContainsKey(instanceId)) return null;
            else if (_tempCollidersIds[instanceId] == null)
            {
                _tempCollidersIds.Remove(instanceId);
                var tempCol = UnityEditor.EditorUtility.InstanceIDToObject(instanceId);
                if (tempCol != null) Object.DestroyImmediate(tempCol);
                return null;
            }
            return _tempCollidersIds[instanceId];
        }

        public static void UpdateTempColliders()
        {
            DestroyTempColliders();
            PWBIO.UpdateOctree();
            var allTransforms = GameObject.FindObjectsOfType<Transform>();
            foreach (var transform in allTransforms)
            {
                if (!transform.gameObject.activeInHierarchy) continue;
                if (transform.parent != null) continue;
                AddTempCollider(transform.gameObject);
            }
        }

        public static void AddTempCollider(GameObject obj, Pose pose)
        {
            var currentPose = new Pose(obj.transform.position, obj.transform.rotation);
            obj.transform.SetPositionAndRotation(pose.position, pose.rotation);
            AddTempCollider(obj);
            obj.transform.SetPositionAndRotation(currentPose.position, currentPose.rotation);
        }
        public static void AddTempCollider(GameObject obj)
        {
            void AddParentsIds(GameObject target)
            {
                var parents = target.GetComponentsInParent<Transform>();
                foreach (var parent in parents)
                {
                    if (!_tempCollidersTargetParentsIds.ContainsKey(target.GetInstanceID()))
                        _tempCollidersTargetParentsIds.Add(target.GetInstanceID(), new System.Collections.Generic.List<int>());
                    _tempCollidersTargetParentsIds[target.GetInstanceID()].Add(parent.gameObject.GetInstanceID());
                    if (!_tempCollidersTargetChildrenIds.ContainsKey(parent.gameObject.GetInstanceID()))
                        _tempCollidersTargetChildrenIds.Add(parent.gameObject.GetInstanceID(),
                            new System.Collections.Generic.List<int>());
                    _tempCollidersTargetChildrenIds[parent.gameObject.GetInstanceID()].Add(target.GetInstanceID());
                }
            }

            GameObject CreateTempCollider(GameObject target, Mesh mesh)
            {
                if (target == null || mesh == null) return null;
                var differentVertices = new System.Collections.Generic.List<Vector3>();
                foreach (var vertex in mesh.vertices)
                {
                    if (!differentVertices.Contains(vertex)) differentVertices.Add(vertex);
                    if (differentVertices.Count >= 3) break;
                }
                if (differentVertices.Count < 3) return null;
                if (_tempCollidersTargets.ContainsKey(target.GetInstanceID()))
                {
                    if (_tempCollidersTargets[target.GetInstanceID()] != null)
                        return _tempCollidersTargets[target.GetInstanceID()];
                    else _tempCollidersTargets.Remove(target.GetInstanceID());
                }
                var name = target.GetInstanceID().ToString();
                var tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), target);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = target.transform.position;
                tempObj.transform.rotation = target.transform.rotation;
                tempObj.transform.localScale = target.transform.lossyScale;
                _tempCollidersTargets.Add(target.GetInstanceID(), tempObj);
                AddParentsIds(target);

                MeshUtils.AddCollider(mesh, tempObj);
                return tempObj;
            }

            bool ObjectIsActiveAndWithoutCollider(GameObject go)
            {
                if (!go.activeInHierarchy) return false;
                var collider = go.GetComponent<Collider>();
                if (collider == null) return true;
                if (collider is MeshCollider)
                {
                    var meshCollider = collider as MeshCollider;
                    if (meshCollider.sharedMesh == null) return true;
                }
                return collider.isTrigger;
            }

            var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                if (!ObjectIsActiveAndWithoutCollider(meshFilter.gameObject)) continue;
                CreateTempCollider(meshFilter.gameObject, meshFilter.sharedMesh);
            }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in skinnedMeshRenderers)
            {
                if (!ObjectIsActiveAndWithoutCollider(renderer.gameObject)) continue;
                CreateTempCollider(renderer.gameObject, renderer.sharedMesh);
            }

            var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                var target = spriteRenderer.gameObject;
                if (!target.activeInHierarchy) continue;
                if (_tempCollidersTargets.ContainsKey(target.GetInstanceID()))
                {
                    if (_tempCollidersTargets[target.GetInstanceID()] != null) return;
                    else _tempCollidersTargets.Remove(target.GetInstanceID());
                }
                var name = spriteRenderer.gameObject.GetInstanceID().ToString();
                var tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), spriteRenderer.gameObject);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = spriteRenderer.transform.position;
                tempObj.transform.rotation = spriteRenderer.transform.rotation;
                tempObj.transform.localScale = spriteRenderer.transform.lossyScale;
                _tempCollidersTargets.Add(target.GetInstanceID(), tempObj);
                AddParentsIds(target);
                var boxCollider = tempObj.AddComponent<BoxCollider>();
                boxCollider.size = (Vector3)(spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit)
                    + new Vector3(0f, 0f, 0.01f);
                var collider = spriteRenderer.GetComponent<Collider2D>();
                if (collider != null && !collider.isTrigger) continue;
                tempObj = new GameObject(name);
                tempObj.hideFlags = HideFlags.HideAndDontSave;
                _tempCollidersIds.Add(tempObj.GetInstanceID(), spriteRenderer.gameObject);
                tempObj.transform.SetParent(parentCollider.transform);
                tempObj.transform.position = spriteRenderer.transform.position;
                tempObj.transform.rotation = spriteRenderer.transform.rotation;
                tempObj.transform.localScale = spriteRenderer.transform.lossyScale;
                var boxCollider2D = tempObj.AddComponent<BoxCollider2D>();
                boxCollider2D.size = spriteRenderer.sprite.rect.size / spriteRenderer.sprite.pixelsPerUnit;
            }
        }

        public static void DestroyTempCollider(int objId)
        {
            if (!_tempCollidersTargets.ContainsKey(objId)) return;
            var temCollider = _tempCollidersTargets[objId];
            if (temCollider == null)
            {
                _tempCollidersTargets.Remove(objId);
                return;
            }
            var tempId = temCollider.GetInstanceID();
            _tempCollidersIds.Remove(tempId);
            _tempCollidersTargets.Remove(objId);
            _tempCollidersTargetParentsIds.Remove(objId);
            Object.DestroyImmediate(temCollider);
        }
        public static void DestroyTempColliders()
        {
            _tempCollidersIds.Clear();
            _tempCollidersTargets.Clear();
            _tempCollidersTargetParentsIds.Clear();
            _tempCollidersTargetChildrenIds.Clear();
            var parentObj = GameObject.Find(PWBCore.PARENT_COLLIDER_NAME);
            if (parentObj != null) Object.DestroyImmediate(parentObj);
            _parentColliderId = -1;
        }


        public static void UpdateTempCollidersTransforms(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                var parentId = obj.GetInstanceID();
                bool isParent = false;
                foreach (var childId in _tempCollidersTargetParentsIds.Keys)
                {
                    var parentsIds = _tempCollidersTargetParentsIds[childId];
                    if (parentsIds.Contains(parentId))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent) continue;
                foreach (var id in _tempCollidersTargetChildrenIds[parentId].ToArray())
                {
                    if (!_tempCollidersTargets.ContainsKey(id))
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        continue;
                    }
                    var tempCollider = _tempCollidersTargets[id];
                    if (tempCollider == null)
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        _tempCollidersTargets.Remove(id);
                        continue;
                    }
                    var childObj = (GameObject)UnityEditor.EditorUtility.InstanceIDToObject(id);
                    if (childObj == null) continue;
                    tempCollider.transform.position = childObj.transform.position;
                    tempCollider.transform.rotation = childObj.transform.rotation;
                    tempCollider.transform.localScale = childObj.transform.lossyScale;
                }
            }
        }

        public static void SetActiveTempColliders(GameObject[] objects, bool value)
        {
            foreach (var obj in objects)
            {
                if (!obj.activeInHierarchy) continue;
                var parentId = obj.GetInstanceID();
                bool isParent = false;
                foreach (var childId in _tempCollidersTargetParentsIds.Keys)
                {
                    var parentsIds = _tempCollidersTargetParentsIds[childId];
                    if (parentsIds.Contains(parentId))
                    {
                        isParent = true;
                        break;
                    }
                }
                if (!isParent) continue;
                foreach (var id in _tempCollidersTargetChildrenIds[parentId].ToArray())
                {
                    if (!_tempCollidersTargets.ContainsKey(id))
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        continue;
                    }
                    var tempCollider = _tempCollidersTargets[id];
                    if (tempCollider == null)
                    {
                        _tempCollidersTargetChildrenIds[parentId].Remove(id);
                        _tempCollidersTargets.Remove(id);
                        continue;
                    }
                    var childObj = (GameObject)UnityEditor.EditorUtility.InstanceIDToObject(id);
                    if (childObj == null) continue;
                    tempCollider.SetActive(value);
                    tempCollider.transform.position = childObj.transform.position;
                    tempCollider.transform.rotation = childObj.transform.rotation;
                    tempCollider.transform.localScale = childObj.transform.lossyScale;
                }
            }
        }

        public static GameObject[] GetTempColliders(GameObject obj)
        {
            var parentId = obj.GetInstanceID();
            bool isParent = false;
            foreach (var childId in _tempCollidersTargetParentsIds.Keys)
            {
                var parentsIds = _tempCollidersTargetParentsIds[childId];
                if (parentsIds.Contains(parentId))
                {
                    isParent = true;
                    break;
                }
            }
            if (!isParent) return null;
            var tempColliders = new System.Collections.Generic.List<GameObject>();
            foreach (var id in _tempCollidersTargetChildrenIds[parentId].ToArray())
            {
                if (!_tempCollidersTargets.ContainsKey(id))
                {
                    _tempCollidersTargetChildrenIds[parentId].Remove(id);
                    continue;
                }
                var tempCollider = _tempCollidersTargets[id];
                if (tempCollider == null)
                {
                    _tempCollidersTargetChildrenIds[parentId].Remove(id);
                    _tempCollidersTargets.Remove(id);
                    continue;
                }
                tempColliders.Add(tempCollider);
            }
            return tempColliders.ToArray();
        }
        #endregion
    }

    [System.Serializable]
    public class PWBData
    {
        public const string DATA_DIR = "Data";
        public const string FILE_NAME = "PWBData";
        public const string FULL_FILE_NAME = FILE_NAME + ".txt";
        public const string RELATIVE_TOOL_DIR = "PluginMaster/DesignTools/Editor/PrefabWorldBuilder";
        public const string RELATIVE_RESOURCES_DIR = RELATIVE_TOOL_DIR + "/Resources";
        public const string RELATIVE_DATA_DIR = RELATIVE_RESOURCES_DIR + "/" + DATA_DIR;
        public const string PALETTES_DIR = "Palettes";
        public const string VERSION = "3.3";
        [SerializeField] private string _version = VERSION;
        [SerializeField] private string _rootDirectory = null;
        [SerializeField] private int _autoSavePeriodMinutes = 1;
        [SerializeField] private bool _undoBrushProperties = true;
        [SerializeField] private bool _undoPalette = true;
        [SerializeField] private int _controlPointSize = 1;
        [SerializeField] private bool _closeAllWindowsWhenClosingTheToolbar = false;
        [SerializeField] private int _thumbnailLayer = 7;
        public enum UnsavedChangesAction { ASK, SAVE, DISCARD }
        [SerializeField] private UnsavedChangesAction _unsavedChangesAction = UnsavedChangesAction.ASK;
        [SerializeField] private PaletteManager _paletteManager = PaletteManager.instance;

        [SerializeField] private PinManager pinManager = PinManager.instance as PinManager;
        [SerializeField] private BrushManager _brushManager = BrushManager.instance as BrushManager;
        [SerializeField] private GravityToolManager _gravityToolManager = GravityToolManager.instance as GravityToolManager;
        [SerializeField] private LineManager _lineManager = LineManager.instance as LineManager;
        [SerializeField] private ShapeManager _shapeManager = ShapeManager.instance as ShapeManager;
        [SerializeField] private TilingManager _tilingManager = TilingManager.instance as TilingManager;
        [SerializeField] private ReplacerManager _replacerManager = ReplacerManager.instance as ReplacerManager;
        [SerializeField] private EraserManager _eraserManager = EraserManager.instance as EraserManager;

        [SerializeField]
        private SelectionToolManager _selectionToolManager = SelectionToolManager.instance as SelectionToolManager;
        [SerializeField] private ExtrudeManager _extrudeSettings = ExtrudeManager.instance as ExtrudeManager;
        [SerializeField] private MirrorManager _mirrorManager = MirrorManager.instance as MirrorManager;

        [SerializeField] private SnapManager _snapManager = new SnapManager();
        private bool _savePending = false;
        private bool _saving = false;

        public static string palettesDirectory
        {
            get
            {
                var dir = PWBSettings.dataDir + "/" + PALETTES_DIR;
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static string dataPath => PWBSettings.dataDir + "/" + FULL_FILE_NAME;

        public string version => _version;
        public int autoSavePeriodMinutes
        {
            get => _autoSavePeriodMinutes;
            set
            {
                value = Mathf.Clamp(value, 1, 10);
                if (_autoSavePeriodMinutes == value) return;
                _autoSavePeriodMinutes = value;
                Save();
            }
        }

        public bool undoBrushProperties
        {
            get => _undoBrushProperties;
            set
            {
                if (_undoBrushProperties == value) return;
                _undoBrushProperties = value;
                Save();
            }
        }

        public bool undoPalette
        {
            get => _undoPalette;
            set
            {
                if (_undoPalette == value) return;
                _undoPalette = value;
                Save();
            }
        }

        public int controPointSize
        {
            get => _controlPointSize;
            set
            {
                if (_controlPointSize == value) return;
                _controlPointSize = value;
                Save();
            }
        }

        public bool closeAllWindowsWhenClosingTheToolbar
        {
            get => _closeAllWindowsWhenClosingTheToolbar;
            set
            {
                if (_closeAllWindowsWhenClosingTheToolbar == value) return;
                _closeAllWindowsWhenClosingTheToolbar = value;
                Save();
            }
        }

        public int thumbnailLayer
        {
            get => _thumbnailLayer;
            set
            {
                value = Mathf.Clamp(value, 0, 31);
                if (_thumbnailLayer == value) return;
                _thumbnailLayer = value;
                Save();
            }
        }

        public UnsavedChangesAction unsavedChangesAction
        {
            get => _unsavedChangesAction;
            set
            {
                if (_unsavedChangesAction == value) return;
                _unsavedChangesAction = value;
                Save();
            }
        }
        public void SetSavePending() => _savePending = true;
        public bool saving => _saving;
        public bool VersionUpdate()
        {
            var currentText = ReadDataText();
            if (currentText == null) return false;
            var dataVersion = JsonUtility.FromJson<PWBDataVersion>(currentText);
            bool V1_9()
            {
                if (dataVersion.IsOlderThan("1.10"))
                {
                    var v1_9_data = JsonUtility.FromJson<V1_9_PWBData>(currentText);
                    var v1_9_sceneItems = v1_9_data._lineManager._unsavedProfile._sceneLines;
                    if (v1_9_sceneItems == null || v1_9_sceneItems.Length == 0) return false;
                    foreach (var v1_9_sceneData in v1_9_sceneItems)
                    {
                        var v1_9_sceneLines = v1_9_sceneData._lines;
                        if (v1_9_sceneItems == null || v1_9_sceneItems.Length == 0) return false;
                        foreach (var v1_9_sceneLine in v1_9_sceneLines)
                        {
                            if (v1_9_sceneLines == null || v1_9_sceneLines.Length == 0) return false;
                            var lineData = new LineData(v1_9_sceneLine._id, v1_9_sceneLine._data._controlPoints,
                                v1_9_sceneLine._objectPoses, v1_9_sceneLine._initialBrushId,
                                v1_9_sceneLine._data._closed, v1_9_sceneLine._settings);
                            LineManager.instance.AddPersistentItem(v1_9_sceneData._sceneGUID, lineData);
                        }
                    }
                    return true;
                }
                return false;
            }
            var updated = V1_9();

            if (dataVersion.IsOlderThan("2.9"))
            {
                var v2_8_data = JsonUtility.FromJson<V2_8_PWBData>(currentText);
                if (v2_8_data._paletteManager._paletteData.Length > 0) PaletteManager.ClearPaletteList();
                foreach (var paletteData in v2_8_data._paletteManager._paletteData)
                {
                    paletteData.version = VERSION;
                    PaletteManager.AddPalette(paletteData);
                }
                var textAssets = Resources.LoadAll<TextAsset>(FILE_NAME);
                for (int i = 0; i < textAssets.Length; ++i)
                {
                    var assetPath = UnityEditor.AssetDatabase.GetAssetPath(textAssets[i]);
                    UnityEditor.AssetDatabase.DeleteAsset(assetPath);
                }
                PWBCore.staticData.Save(false);

                PrefabPalette.RepainWindow();
                updated = true;
            }
            return updated;
        }

        public void UpdateRootDirectory()
        {
            var directories = System.IO.Directory.GetDirectories(Application.dataPath, "PrefabWorldBuilder",
               System.IO.SearchOption.AllDirectories).Where(d => d.Replace("\\", "/").Contains(RELATIVE_TOOL_DIR)).ToArray();
            if (directories.Length == 0)
            {
                _rootDirectory = Application.dataPath + "/" + RELATIVE_TOOL_DIR;
                _rootDirectory = _rootDirectory.Replace("\\", "/");
                System.IO.Directory.CreateDirectory(_rootDirectory);
            }
            else _rootDirectory = System.IO.Directory.GetParent(directories[0]).FullName;
        }

        private string rootDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_rootDirectory)) UpdateRootDirectory();
                return _rootDirectory;
            }
        }

        public void Save() => Save(true);

        public void Save(bool updateVersion)
        {
            _saving = true;
            if (updateVersion) VersionUpdate();
            _version = VERSION;
            var jsonString = JsonUtility.ToJson(this);
            System.IO.File.WriteAllText(dataPath, jsonString);
            UnityEditor.AssetDatabase.Refresh();
            _savePending = false;
            _saving = false;
        }

        public static string ReadDataText()
        {
            var fullFilePath = dataPath;
            if (!System.IO.File.Exists(fullFilePath)) PWBCore.staticData.Save(false);
            return System.IO.File.ReadAllText(fullFilePath);
        }

        public void SaveIfPending() { if (_savePending) Save(); }

        public string documentationPath
        {
            get
            {
                var absolutePath = rootDirectory + "/Documentation/Prefab World Builder Documentation.pdf";
                var relativepath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
                return relativepath;
            }
        }
    }
    #endregion

    #region SHORTCUTS
    #region COMBINATION CLASSES
    [System.Serializable]
    public class PWBShortcutCombination : System.IEquatable<PWBShortcutCombination>
    {
        [SerializeField] protected EventModifiers _modifiers = EventModifiers.None;
        public EventModifiers modifiers => _modifiers;
        public bool control => (_modifiers & EventModifiers.Control) != 0;
        public bool alt => (_modifiers & EventModifiers.Alt) != 0;
        public bool shift => (_modifiers & EventModifiers.Shift) != 0;
        public static EventModifiers FilterModifiers(EventModifiers modifiers)
            => modifiers & (EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        public PWBShortcutCombination(EventModifiers modifiers) => _modifiers = FilterModifiers(modifiers);

        public virtual bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE)
        {
            if (Event.current == null) return false;
            var currentModifiers = FilterModifiers(Event.current.modifiers);
            return currentModifiers == modifiers;
        }

        public bool Equals(PWBShortcutCombination other)
        {
            if (other == null) return false;
            return _modifiers == other._modifiers;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is PWBShortcutCombination otherCombination)) return false;
            return Equals(otherCombination);
        }
        public override int GetHashCode()
        {
            int hashCode = 822824530;
            hashCode = hashCode * -1521134295 + _modifiers.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(PWBShortcutCombination lhs, PWBShortcutCombination rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(PWBShortcutCombination lhs, PWBShortcutCombination rhs) => !(lhs == rhs);
    }
    [System.Serializable]
    public class PWBKeyCombination : PWBShortcutCombination, System.IEquatable<PWBKeyCombination>
    {
        [SerializeField] private KeyCode _keyCode = KeyCode.None;
        public KeyCode keycode => _keyCode;

        public void Set(KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
        {
            _keyCode = keyCode;
            _modifiers = FilterModifiers(modifiers);
        }
        public PWBKeyCombination(KeyCode keyCode, EventModifiers modifiers = EventModifiers.None) : base(modifiers)
            => _keyCode = keyCode;
        public bool Equals(PWBKeyCombination other)
        {
            if (other == null) return false;
            return _keyCode == other._keyCode && _modifiers == other._modifiers;
        }
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is PWBKeyCombination otherCombination)) return false;
            return Equals(otherCombination);
        }
        public override int GetHashCode()
        {
            int hashCode = 822824530;
            hashCode = hashCode * -1521134295 + _modifiers.GetHashCode();
            hashCode = hashCode * -1521134295 + _keyCode.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(PWBKeyCombination lhs, PWBKeyCombination rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(PWBKeyCombination lhs, PWBKeyCombination rhs) => !(lhs == rhs);

        public override string ToString()
        {
            var result = string.Empty;
            if (keycode == KeyCode.None) return "Disabled";
            if (control) result = "Ctrl";
            if (alt) result += (result == string.Empty ? "Alt" : "+Alt");
            if (shift) result += (result == string.Empty ? "Shift" : "+Shift");
            if (result != string.Empty) result += "+";
            result += _keyCode;
            return result;
        }

        public override bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE)
        {
            if (keycode == KeyCode.None) return false;
            if (Event.current.type != EventType.KeyDown || Event.current.keyCode != keycode) return false;
            return base.Check();
        }
    }

    [System.Serializable]
    public class PWBMouseCombination : PWBShortcutCombination, System.IEquatable<PWBMouseCombination>
    {
        public enum MouseEvents
        {
            NONE,
            SCROLL_WHEEL,
            DRAG_R_H,
            DRAG_R_V,
            DRAG_M_H,
            DRAG_M_V
        }

        [SerializeField] private MouseEvents _mouseEvent = MouseEvents.NONE;

        public MouseEvents mouseEvent => _mouseEvent;
        public void Set(EventModifiers modifiers, MouseEvents mouseEvent)
        {
            _modifiers = FilterModifiers(modifiers);
            _mouseEvent = mouseEvent;
        }

        public PWBMouseCombination(EventModifiers modifiers, MouseEvents mouseEvent) : base(modifiers)
        => _mouseEvent = mouseEvent;
        public bool Equals(PWBMouseCombination other)
        {
            if (other == null) return false;
            return _mouseEvent == other._mouseEvent && _modifiers == other._modifiers;
        }
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is PWBMouseCombination otherCombination)) return false;
            return Equals(otherCombination);
        }

        public override int GetHashCode()
        {
            int hashCode = 1068782991;
            hashCode = hashCode * -1521134295 + _modifiers.GetHashCode();
            hashCode = hashCode * -1521134295 + _mouseEvent.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PWBMouseCombination lhs, PWBMouseCombination rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.Equals(rhs);
        }
        public static bool operator !=(PWBMouseCombination lhs, PWBMouseCombination rhs) => !(lhs == rhs);

        public bool isRDrag => mouseEvent == MouseEvents.DRAG_R_H || mouseEvent == MouseEvents.DRAG_R_V;
        public bool isMDrag => mouseEvent == MouseEvents.DRAG_M_H || mouseEvent == MouseEvents.DRAG_M_V;
        public bool isMouseDragEvent => mouseEvent == MouseEvents.DRAG_R_H || mouseEvent == MouseEvents.DRAG_R_V
                || mouseEvent == MouseEvents.DRAG_M_H || mouseEvent == MouseEvents.DRAG_M_V;
        public bool isHorizontalDragEvent => mouseEvent == MouseEvents.DRAG_R_H || mouseEvent == MouseEvents.DRAG_M_H;
        public float delta
            => (mouseEvent == MouseEvents.SCROLL_WHEEL ? Event.current.delta.y
            : (isHorizontalDragEvent ? Event.current.delta.x : -Event.current.delta.y));

        public override bool Check(PWBShortcut.Group group = PWBShortcut.Group.NONE)
        {
            if (mouseEvent == MouseEvents.NONE) return false;
            if (!base.Check()) return false;
            if (isMouseDragEvent)
            {
                if (Event.current.type != EventType.MouseDrag) return false;
                if (isRDrag && Event.current.button != 1)
                    return false;
                if (isMDrag && Event.current.button != 2) return false;

                var xIsGreaterThanY = Mathf.Abs(Event.current.delta.x) > Mathf.Abs(Event.current.delta.y);
                if (isHorizontalDragEvent && !xIsGreaterThanY)
                {
                    var other = new PWBMouseCombination(modifiers,
                        mouseEvent == MouseEvents.DRAG_R_H ? MouseEvents.DRAG_R_V : MouseEvents.DRAG_M_V);
                    if (!PWBSettings.shortcuts.CombinationExist(other, group)) Event.current.Use();
                    return false;
                }
                if (!isHorizontalDragEvent && xIsGreaterThanY)
                {
                    var other = new PWBMouseCombination(modifiers,
                        mouseEvent == MouseEvents.DRAG_R_V ? MouseEvents.DRAG_R_H : MouseEvents.DRAG_M_H);
                    if (!PWBSettings.shortcuts.CombinationExist(other, group)) Event.current.Use();
                    return false;
                }
            }
            if (mouseEvent == MouseEvents.SCROLL_WHEEL && !Event.current.isScrollWheel) return false;
            Event.current.Use();
            return true;
        }


    }
    #endregion

    #region SHORTCUT CLASSES
    [System.Serializable]
    public class PWBShortcut
    {
        public enum Group
        {
            NONE = 0,
            GLOBAL = 1,
            GRID = 2,
            PIN = 4,
            BRUSH = 8,
            GRAVITY = 16,
            LINE = 32,
            SHAPE = 64,
            TILING = 128,
            ERASER = 256,
            REPLACER = 512,
            SELECTION = 1024,
            PALETTE = 2048
        }
        [SerializeField] private string _name = null;
        [SerializeField] private Group _group = Group.NONE;
        [SerializeField] private bool _conflicted = false;

        public PWBShortcut(string name, Group group)
        {
            _name = name;
            _group = group;
        }

        public string name => _name;

        public Group group => _group;

        public bool conflicted { get => _conflicted; set => _conflicted = value; }
    }

    [System.Serializable]
    public class PWBKeyShortcut : PWBShortcut
    {
        [SerializeField]
        private PWBKeyCombination _keyCombination = new PWBKeyCombination(KeyCode.None, EventModifiers.None);

        public PWBKeyShortcut(string name, Group group, KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
            : base(name, group) => _keyCombination.Set(keyCode, modifiers);

        public PWBKeyCombination combination => _keyCombination;
        public bool Check() => combination.Check(group);
    }

    [System.Serializable]
    public class PWBMouseShortcut : PWBShortcut
    {
        [SerializeField]
        private PWBMouseCombination _combination
            = new PWBMouseCombination(EventModifiers.None, PWBMouseCombination.MouseEvents.NONE);

        public PWBMouseShortcut(string name, Group group,
            EventModifiers modifiers, PWBMouseCombination.MouseEvents mouseEvent)
            : base(name, group) => _combination.Set(modifiers, mouseEvent);
        public PWBMouseCombination combination => _combination;
        public bool Check() => combination.Check(group);
    }
    #endregion

    [System.Serializable]
    public class PWBShortcuts
    {
        #region PROFILE
        [SerializeField] private string _profileName = string.Empty;
        public string profileName { get => _profileName; set => _profileName = value; }
        public PWBShortcuts(string name) => _profileName = name;

        public static PWBShortcuts GetDefault(int i)
        {
            if (i == 0) return new PWBShortcuts("Default 1");
            else if (i == 1)
            {
                var d2 = new PWBShortcuts("Default 2");
                d2.pinMoveHandlesUp.combination.Set(KeyCode.PageUp);
                d2.pinMoveHandlesDown.combination.Set(KeyCode.PageDown);
                d2.pinSelectPivotHandle.combination.Set(KeyCode.Home);
                d2.pinSelectNextHandle.combination.Set(KeyCode.End);
                d2.pinResetScale.combination.Set(KeyCode.Home, EventModifiers.Control | EventModifiers.Shift);

                d2.pinRotate90YCW.combination.Set(KeyCode.LeftArrow, EventModifiers.Control);
                d2._pinRotate90YCCW.combination.Set(KeyCode.RightArrow, EventModifiers.Control);
                d2.pinRotateAStepYCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Shift);
                d2.pinRotateAStepYCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Shift);

                d2.pinRotate90XCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinRotate90XCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinRotateAStepXCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
                d2.pinRotateAStepXCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d2.pinResetRotation.combination.Set(KeyCode.Home, EventModifiers.Control);

                d2.pinAdd1UnitToSurfDist.combination.Set(KeyCode.UpArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinSubtract1UnitFromSurfDist.combination.Set(KeyCode.DownArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.pinAdd01UnitToSurfDist.combination.Set(KeyCode.UpArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
                d2.pinSubtract01UnitFromSurfDist.combination.Set(KeyCode.DownArrow,
                    EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

                d2.lineToggleCurve.combination.Set(KeyCode.PageDown);
                d2.lineToggleClosed.combination.Set(KeyCode.End);

                d2.selectionRotate90XCW.combination.Set(KeyCode.PageUp,
                    EventModifiers.Control | EventModifiers.Shift);
                d2.selectionRotate90XCCW.combination.Set(KeyCode.PageDown,
                    EventModifiers.Control | EventModifiers.Shift);
                d2.selectionRotate90YCW.combination.Set(KeyCode.LeftArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.selectionRotate90YCCW.combination.Set(KeyCode.RightArrow,
                    EventModifiers.Control | EventModifiers.Alt);
                d2.selectionRotate90ZCW.combination.Set(KeyCode.UpArrow,
                   EventModifiers.Control | EventModifiers.Alt);
                d2.selectionRotate90ZCCW.combination.Set(KeyCode.DownArrow,
                    EventModifiers.Control | EventModifiers.Alt);

                d2.brushRadius.combination.Set(EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_H);
                return d2;
            }
            return null;
        }
        #endregion

        #region KEY COMBINATIONS
        #region GRID
        [SerializeField]
        private PWBKeyShortcut _gridEnableShortcuts = new PWBKeyShortcut("Enable grid shorcuts",
           PWBShortcut.Group.GLOBAL | PWBShortcut.Group.GRID, KeyCode.G, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _gridToggle = new PWBKeyShortcut("Toggle grid",
            PWBShortcut.Group.GRID, KeyCode.G, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _gridToggleSnapping = new PWBKeyShortcut("Toggle snapping",
            PWBShortcut.Group.GRID, KeyCode.H, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _gridToggleLock = new PWBKeyShortcut("Toggle grid lock",
            PWBShortcut.Group.GRID, KeyCode.L, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _gridSetOriginPosition = new PWBKeyShortcut("Set the origin to the active gameobject position",
            PWBShortcut.Group.GRID, KeyCode.W, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _gridSetOriginRotation = new PWBKeyShortcut("Set the grid rotation to the active gameobject rotation",
            PWBShortcut.Group.GRID, KeyCode.E, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _gridSetSize = new PWBKeyShortcut("Set the snap value to the size of the active gameobject",
            PWBShortcut.Group.GRID, KeyCode.R, EventModifiers.Control);
        public PWBKeyShortcut gridEnableShortcuts => _gridEnableShortcuts;
        public PWBKeyShortcut gridToggle => _gridToggle;
        public PWBKeyShortcut gridToggleSnaping => _gridToggleSnapping;
        public PWBKeyShortcut gridToggleLock => _gridToggleLock;
        public PWBKeyShortcut gridSetOriginPosition => _gridSetOriginPosition;
        public PWBKeyShortcut gridSetOriginRotation => _gridSetOriginRotation;
        public PWBKeyShortcut gridSetSize => _gridSetSize;
        #endregion

        #region PIN
        [SerializeField]
        private PWBKeyShortcut _pinMoveHandlesUp = new PWBKeyShortcut("Move handles up",
           PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinMoveHandlesDown = new PWBKeyShortcut("Move handles down",
           PWBShortcut.Group.PIN, KeyCode.J, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSelectNextHandle = new PWBKeyShortcut("Select the next handle as active",
           PWBShortcut.Group.PIN, KeyCode.Y, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSelectPivotHandle = new PWBKeyShortcut("Set the pivot as the active handle",
           PWBShortcut.Group.PIN, KeyCode.T, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinToggleRepeatItem = new PWBKeyShortcut("Toggle repeat item option",
           PWBShortcut.Group.PIN, KeyCode.T, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinResetScale = new PWBKeyShortcut("Reset scale",
          PWBShortcut.Group.PIN, KeyCode.Period, EventModifiers.Control | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinRotate90YCW = new PWBKeyShortcut("Rotate 90º clockwise around Y axis",
          PWBShortcut.Group.PIN, KeyCode.Q, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinRotate90YCCW = new PWBKeyShortcut("Rotate 90º counterclockwise around Y axis",
          PWBShortcut.Group.PIN, KeyCode.W, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepYCW = new PWBKeyShortcut("Rotate clockwise in small steps around the Y axis",
        PWBShortcut.Group.PIN, KeyCode.Q, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepYCCW
            = new PWBKeyShortcut("Rotate counterclockwise in small steps around the Y axis",
        PWBShortcut.Group.PIN, KeyCode.W, EventModifiers.Control | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinRotate90XCW = new PWBKeyShortcut("Rotate 90º clockwise around X axis",
          PWBShortcut.Group.PIN, KeyCode.K, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinRotate90XCCW = new PWBKeyShortcut("Rotate 90º counterclockwise around X axis",
          PWBShortcut.Group.PIN, KeyCode.L, EventModifiers.Control);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepXCW = new PWBKeyShortcut("Rotate clockwise in small steps around the X axis",
        PWBShortcut.Group.PIN, KeyCode.K, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepXCCW
            = new PWBKeyShortcut("Rotate counterclockwise in small steps around the X axis",
        PWBShortcut.Group.PIN, KeyCode.L, EventModifiers.Control | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinRotate90ZCW = new PWBKeyShortcut("Rotate 90º clockwise around Z axis",
          PWBShortcut.Group.PIN, KeyCode.Period, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinRotate90ZCCW = new PWBKeyShortcut("Rotate 90º counterclockwise around Z axis",
          PWBShortcut.Group.PIN, KeyCode.Comma, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepZCW = new PWBKeyShortcut("Rotate clockwise in small steps around the Z axis",
        PWBShortcut.Group.PIN, KeyCode.Period, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinRotateAStepZCCW
            = new PWBKeyShortcut("Rotate counterclockwise in small steps around the Z axis",
        PWBShortcut.Group.PIN, KeyCode.Comma, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinResetRotation = new PWBKeyShortcut("Reset rotation to zero",
         PWBShortcut.Group.PIN, KeyCode.M, EventModifiers.Control | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinAdd1UnitToSurfDist = new PWBKeyShortcut("Increase the distance from the surface by 1 unit",
          PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinSubtract1UnitFromSurfDist
            = new PWBKeyShortcut("Decrease the distance from the surface by 1 unit",
          PWBShortcut.Group.PIN, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _pinAdd01UnitToSurfDist
            = new PWBKeyShortcut("Increase the distance from the surface by 0.1 units",
         PWBShortcut.Group.PIN, KeyCode.U, EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _pinSubtract01UnitFromSurfDist
            = new PWBKeyShortcut("Decrease the distance from the surface by 0.1 units",
          PWBShortcut.Group.PIN, KeyCode.J,
          EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        [SerializeField]
        private PWBKeyShortcut _pinResetSurfDist = new PWBKeyShortcut("Reset the distance from the surface to zero",
         PWBShortcut.Group.PIN, KeyCode.G, EventModifiers.Control | EventModifiers.Shift);

        public PWBKeyShortcut pinMoveHandlesUp => _pinMoveHandlesUp;
        public PWBKeyShortcut pinMoveHandlesDown => _pinMoveHandlesDown;
        public PWBKeyShortcut pinSelectNextHandle => _pinSelectNextHandle;
        public PWBKeyShortcut pinSelectPivotHandle => _pinSelectPivotHandle;
        public PWBKeyShortcut pinToggleRepeatItem => _pinToggleRepeatItem;
        public PWBKeyShortcut pinResetScale => _pinResetScale;

        public PWBKeyShortcut pinRotate90YCW => _pinRotate90YCW;
        public PWBKeyShortcut pinRotate90YCCW => _pinRotate90YCCW;
        public PWBKeyShortcut pinRotateAStepYCW => _pinRotateAStepYCW;
        public PWBKeyShortcut pinRotateAStepYCCW => _pinRotateAStepYCCW;

        public PWBKeyShortcut pinRotate90XCW => _pinRotate90XCW;
        public PWBKeyShortcut pinRotate90XCCW => _pinRotate90XCCW;
        public PWBKeyShortcut pinRotateAStepXCW => _pinRotateAStepXCW;
        public PWBKeyShortcut pinRotateAStepXCCW => _pinRotateAStepXCCW;

        public PWBKeyShortcut pinRotate90ZCW => _pinRotate90ZCW;
        public PWBKeyShortcut pinRotate90ZCCW => _pinRotate90ZCCW;
        public PWBKeyShortcut pinRotateAStepZCW => _pinRotateAStepZCW;
        public PWBKeyShortcut pinRotateAStepZCCW => _pinRotateAStepZCCW;

        public PWBKeyShortcut pinResetRotation => _pinResetRotation;

        public PWBKeyShortcut pinAdd1UnitToSurfDist => _pinAdd1UnitToSurfDist;
        public PWBKeyShortcut pinSubtract1UnitFromSurfDist => _pinSubtract1UnitFromSurfDist;
        public PWBKeyShortcut pinAdd01UnitToSurfDist => _pinAdd01UnitToSurfDist;
        public PWBKeyShortcut pinSubtract01UnitFromSurfDist => _pinSubtract01UnitFromSurfDist;

        public PWBKeyShortcut pinResetSurfDist => _pinResetSurfDist;
        #endregion

        #region BRUSH & GRAVITY
        [SerializeField]
        private PWBKeyShortcut _brushUpdatebrushstroke = new PWBKeyShortcut("Update brushstroke",
          PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY, KeyCode.Period, EventModifiers.Control | EventModifiers.Shift);
        public PWBKeyShortcut brushUpdatebrushstroke => _brushUpdatebrushstroke;
        #endregion

        #region GRAVITY
        [SerializeField]
        private PWBKeyShortcut _gravityAdd1UnitToSurfDist = new PWBKeyShortcut("Increase the distance from the surface by 1 unit",
          PWBShortcut.Group.GRAVITY, KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _gravitySubtract1UnitFromSurfDist = new PWBKeyShortcut("Decrease the distance from the surface by 1 unit",
          PWBShortcut.Group.GRAVITY, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _gravityAdd01UnitToSurfDist = new PWBKeyShortcut("Increase the distance from the surface by 0.1 units",
         PWBShortcut.Group.GRAVITY, KeyCode.U,
         EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _gravitySubtract01UnitFromSurfDist
            = new PWBKeyShortcut("Decrease the distance from the surface by 0.1 units",
          PWBShortcut.Group.GRAVITY, KeyCode.J,
          EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift);

        public PWBKeyShortcut gravityAdd1UnitToSurfDist => _gravityAdd1UnitToSurfDist;
        public PWBKeyShortcut gravitySubtract1UnitFromSurfDist => _gravitySubtract1UnitFromSurfDist;
        public PWBKeyShortcut gravityAdd01UnitToSurfDist => _gravityAdd01UnitToSurfDist;
        public PWBKeyShortcut gravitySubtract01UnitFromSurfDist => _gravitySubtract01UnitFromSurfDist;
        #endregion

        #region EDIT MODE
        [SerializeField]
        private PWBKeyShortcut _editModeDeleteItemAndItsChildren
            = new PWBKeyShortcut("Delete selected persistent item and its children",
           PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
           KeyCode.Delete, EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _editModeDeleteItemButNotItsChildren
            = new PWBKeyShortcut("Delete selected persistent item but not its children",
           PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
           KeyCode.Delete, EventModifiers.Alt | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _editModeSelectParent = new PWBKeyShortcut("Select parent object",
           PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
           KeyCode.T, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _editModeToggle = new PWBKeyShortcut("Toggle edit mode",
          PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE | PWBShortcut.Group.TILING,
          KeyCode.Period, EventModifiers.Alt | EventModifiers.Shift);
        public PWBKeyShortcut editModeDeleteItemAndItsChildren => _editModeDeleteItemAndItsChildren;
        public PWBKeyShortcut editModeDeleteItemButNotItsChildren => _editModeDeleteItemButNotItsChildren;
        public PWBKeyShortcut editModeSelectParent => _editModeSelectParent;
        public PWBKeyShortcut editModeToggle => _editModeToggle;
        #endregion

        #region LINE
        [SerializeField]
        private PWBKeyShortcut _lineSelectAllPoints = new PWBKeyShortcut("Select all points",
          PWBShortcut.Group.LINE, KeyCode.A, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineDeselectAllPoints = new PWBKeyShortcut("Deselect all points",
          PWBShortcut.Group.LINE, KeyCode.D, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineToggleCurve = new PWBKeyShortcut("Set the previous segment as a Curved or Straight Line",
          PWBShortcut.Group.LINE, KeyCode.Y, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _lineToggleClosed = new PWBKeyShortcut("Close or open the line",
          PWBShortcut.Group.LINE, KeyCode.O, EventModifiers.Control | EventModifiers.Shift);
        public PWBKeyShortcut lineSelectAllPoints => _lineSelectAllPoints;
        public PWBKeyShortcut lineDeselectAllPoints => _lineDeselectAllPoints;
        public PWBKeyShortcut lineToggleCurve => _lineToggleCurve;
        public PWBKeyShortcut lineToggleClosed => _lineToggleClosed;
        #endregion

        #region TILING & SELECTION
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90XCW = new PWBKeyShortcut("Rotate 90º clockwise around X axis",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.U, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90XCCW = new PWBKeyShortcut("Rotate 90º counterclockwise around X axis",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.J, EventModifiers.Control | EventModifiers.Shift);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90YCW = new PWBKeyShortcut("Rotate 90º clockwise around Y axis",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.K, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90YCCW = new PWBKeyShortcut("Rotate 90º counterclockwise around Y axis",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.L, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90ZCW = new PWBKeyShortcut("Rotate 90º clockwise around Z axis",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.U, EventModifiers.Control | EventModifiers.Alt);
        [SerializeField]
        private PWBKeyShortcut _selectionRotate90ZCCW = new PWBKeyShortcut("Rotate 90º counterclockwise around Z axis",
          PWBShortcut.Group.TILING | PWBShortcut.Group.SELECTION, KeyCode.J, EventModifiers.Control | EventModifiers.Alt);
        public PWBKeyShortcut selectionRotate90XCW => _selectionRotate90XCW;
        public PWBKeyShortcut selectionRotate90XCCW => _selectionRotate90XCCW;
        public PWBKeyShortcut selectionRotate90YCW => _selectionRotate90YCW;
        public PWBKeyShortcut selectionRotate90YCCW => _selectionRotate90YCCW;
        public PWBKeyShortcut selectionRotate90ZCW => _selectionRotate90ZCW;
        public PWBKeyShortcut selectionRotate90ZCCW => _selectionRotate90ZCCW;
        #endregion

        #region SELECTION
        [SerializeField]
        private PWBKeyShortcut _selectionTogglePositionHandle = new PWBKeyShortcut("Toggle position handle",
          PWBShortcut.Group.SELECTION, KeyCode.W);
        [SerializeField]
        private PWBKeyShortcut _selectionToggleRotationHandle = new PWBKeyShortcut("Toggle rotation handle",
          PWBShortcut.Group.SELECTION, KeyCode.E);
        [SerializeField]
        private PWBKeyShortcut _selectionToggleScaleHandle = new PWBKeyShortcut("Toggle scale handle",
          PWBShortcut.Group.SELECTION, KeyCode.R);
        [SerializeField]
        private PWBKeyShortcut _selectionEditCustomHandle = new PWBKeyShortcut("Edit custom handle",
          PWBShortcut.Group.SELECTION, KeyCode.U);
        public PWBKeyShortcut selectionTogglePositionHandle => _selectionTogglePositionHandle;
        public PWBKeyShortcut selectionToggleRotationHandle => _selectionToggleRotationHandle;
        public PWBKeyShortcut selectionToggleScaleHandle => _selectionToggleScaleHandle;
        public PWBKeyShortcut selectionEditCustomHandle => _selectionEditCustomHandle;
        #endregion

        #region PALETTE
        [SerializeField]
        private PWBKeyShortcut _paletteDeleteBrush = new PWBKeyShortcut("Delete selected brushes",
           PWBShortcut.Group.PALETTE, KeyCode.Delete, EventModifiers.Control | EventModifiers.Shift);
        public PWBKeyShortcut paletteDeleteBrush => _paletteDeleteBrush;
        #endregion

        #region CONFLICTS
        private PWBKeyShortcut[] _keyShortcuts = null;
        public PWBKeyShortcut[] keyShortcuts
        {
            get
            {
                if (_keyShortcuts == null)
                    _keyShortcuts = new PWBKeyShortcut[]
                    {
                        /*/// GRID ///*/
                        _gridEnableShortcuts,
                        _gridToggle,
                        _gridToggleSnapping,
                        _gridToggleLock,
                        _gridSetOriginPosition,
                        _gridSetOriginRotation,
                        _gridSetSize,
                        /*/// PIN ///*/
                        _pinMoveHandlesUp,
                        _pinMoveHandlesDown,
                        _pinSelectNextHandle,
                        _pinSelectPivotHandle,
                        _pinToggleRepeatItem,
                        _pinResetScale,

                        _pinRotate90YCW,
                        _pinRotate90YCCW,
                        _pinRotateAStepYCW,
                        _pinRotateAStepYCCW,

                        _pinRotate90XCW,
                        _pinRotate90XCCW,
                        _pinRotateAStepXCW,
                        _pinRotateAStepXCCW,

                        _pinRotate90ZCW,
                        _pinRotate90ZCCW,
                        _pinRotateAStepZCW,
                        _pinRotateAStepZCCW,

                        _pinResetRotation,

                        _pinAdd1UnitToSurfDist,
                        _pinSubtract1UnitFromSurfDist,
                        _pinAdd01UnitToSurfDist,
                        _pinSubtract01UnitFromSurfDist,

                        _pinResetSurfDist,
                        /*/// BRUSH & GRAVITY ///*/
                        _brushUpdatebrushstroke,
                        /*/// GRAVITY ///*/
                        _gravityAdd1UnitToSurfDist,
                        _gravitySubtract1UnitFromSurfDist,
                        _gravityAdd01UnitToSurfDist,
                        _gravitySubtract01UnitFromSurfDist,
                        /*/// EDIT MODE ///*/
                        _editModeDeleteItemAndItsChildren,
                        _editModeDeleteItemButNotItsChildren,
                        _editModeSelectParent,
                        editModeToggle,
                        /*/// LINE ///*/
                        _lineSelectAllPoints,
                        _lineDeselectAllPoints,
                        _lineToggleCurve,
                        _lineToggleClosed,
                        /*/// TILING & SELECTION ///*/
                        _selectionRotate90XCW,
                        _selectionRotate90XCCW,
                        _selectionRotate90YCW,
                        _selectionRotate90YCCW,
                        _selectionRotate90ZCW,
                        _selectionRotate90ZCCW,
                        /*/// SELECTION ///*/
                        _selectionTogglePositionHandle,
                        _selectionToggleRotationHandle,
                        _selectionToggleScaleHandle,
                        _selectionEditCustomHandle,
                         /*/// SELECTION ///*/
                         _paletteDeleteBrush
                    };
                return _keyShortcuts;
            }
        }

        public void UpdateConficts()
        {
            foreach (var shortcut in keyShortcuts) shortcut.conflicted = false;
            for (int i = 0; i < keyShortcuts.Length; ++i)
            {
                var shortcut1 = keyShortcuts[i];
                if (shortcut1.conflicted) continue;
                if (shortcut1.combination.keycode == KeyCode.None) continue;
                for (int j = i + 1; j < keyShortcuts.Length; ++j)
                {
                    var shortcut2 = keyShortcuts[j];
                    if (shortcut2.conflicted) continue;
                    if (shortcut2.combination.keycode == KeyCode.None) continue;
                    if ((shortcut1.group & shortcut2.group) == 0 && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0
                        && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0) continue;
                    if (shortcut1 == gridEnableShortcuts && (shortcut2.group & PWBShortcut.Group.GRID) != 0) continue;

                    if (shortcut1.combination == shortcut2.combination)
                    {
                        shortcut1.conflicted = true;
                        shortcut2.conflicted = true;
                    }
                }
            }
        }

        public bool CheckConflicts(PWBKeyCombination combi, PWBKeyShortcut target, out string conflicts)
        {
            conflicts = string.Empty;
            foreach (var shortcut in keyShortcuts)
            {
                if (target == shortcut) continue;
                if (target.combination.keycode == KeyCode.None || shortcut.combination.keycode == KeyCode.None) continue;
                if (combi == shortcut.combination && ((target.group & shortcut.group) != 0
                    || (shortcut.group & PWBShortcut.Group.GLOBAL) != 0 || (target.group & PWBShortcut.Group.GLOBAL) != 0))
                {
                    if (shortcut == gridEnableShortcuts && (target.group & PWBShortcut.Group.GRID) != 0) continue;
                    if (target == gridEnableShortcuts && (shortcut.group & PWBShortcut.Group.GRID) != 0) continue;
                    if (conflicts != string.Empty) conflicts += "\n";
                    conflicts += shortcut.name;
                }
            }
            return conflicts != string.Empty;
        }
        #endregion
        #endregion

        #region MOUSE COMBINATIONS

        #region PIN
        [SerializeField]
        private PWBMouseShortcut _pinScale = new PWBMouseShortcut("Edit Scale",
           PWBShortcut.Group.PIN, EventModifiers.Control, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        [SerializeField]
        private PWBMouseShortcut _pinSelectNextItem = new PWBMouseShortcut("Select next item in the multi-brush",
           PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);

        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundY = new PWBMouseShortcut("Rotate freely around local Y axis",
           PWBShortcut.Group.PIN, EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_R_H);
        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundYSnaped
            = new PWBMouseShortcut("Rotate freely around the local Y axis in steps",
                PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.DRAG_R_H);

        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundX = new PWBMouseShortcut("Rotate freely around local X axis",
           PWBShortcut.Group.PIN, EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_M_V);
        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundXSnaped
            = new PWBMouseShortcut("Rotate freely around the local X axis in steps",
                PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.DRAG_M_V);

        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundZ = new PWBMouseShortcut("Rotate freely around local Z axis",
           PWBShortcut.Group.PIN, EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_M_V);
        [SerializeField]
        private PWBMouseShortcut _pinRotateAroundZSnaped
            = new PWBMouseShortcut("Rotate freely around the local Z axis in steps", PWBShortcut.Group.PIN,
                EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_M_V);

        [SerializeField]
        private PWBMouseShortcut _pinSurfDist = new PWBMouseShortcut("Edit distance to the surface",
           PWBShortcut.Group.PIN,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_V);

        public PWBMouseShortcut pinScale => _pinScale;
        public PWBMouseShortcut pinSelectNextItem => _pinSelectNextItem;

        public PWBMouseShortcut pinRotateAroundY => _pinRotateAroundY;
        public PWBMouseShortcut pinRotateAroundYSnaped => _pinRotateAroundYSnaped;
        public PWBMouseShortcut pinRotateAroundX => _pinRotateAroundX;
        public PWBMouseShortcut pinRotateAroundXSnaped => _pinRotateAroundXSnaped;
        public PWBMouseShortcut pinRotateAroundZ => _pinRotateAroundZ;
        public PWBMouseShortcut pinRotateAroundZSnaped => _pinRotateAroundZSnaped;

        public PWBMouseShortcut pinSurfDist => _pinSurfDist;
        #endregion

        #region RADIUS
        [SerializeField]
        private PWBMouseShortcut _brushRadius = new PWBMouseShortcut("Change radius",
           PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY | PWBShortcut.Group.ERASER | PWBShortcut.Group.REPLACER,
           EventModifiers.Control, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        public PWBMouseShortcut brushRadius => _brushRadius;
        #endregion

        #region BRUSH & GRAVITY
        [SerializeField]
        private PWBMouseShortcut _brushDensity = new PWBMouseShortcut("Edit density",
           PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY,
           EventModifiers.Control | EventModifiers.Alt, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        [SerializeField]
        private PWBMouseShortcut _brushRotate = new PWBMouseShortcut("Rotate brush",
           PWBShortcut.Group.BRUSH | PWBShortcut.Group.GRAVITY,
           EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_R_H);

        public PWBMouseShortcut brushDensity => _brushDensity;
        public PWBMouseShortcut brushRotate => _brushRotate;
        #endregion

        #region GRAVITY
        [SerializeField]
        private PWBMouseShortcut _gravitySurfDist
            = new PWBMouseShortcut("Edit distance to the surface", PWBShortcut.Group.GRAVITY,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_V);
        public PWBMouseShortcut gravitySurfDist => _gravitySurfDist;
        #endregion

        #region LINE & SHAPE
        [SerializeField]
        private PWBMouseShortcut _lineEditGap
            = new PWBMouseShortcut("Edit gap size", PWBShortcut.Group.LINE | PWBShortcut.Group.SHAPE,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_H);
        public PWBMouseShortcut lineEditGap => _lineEditGap;
        #endregion

        #region TILING
        [SerializeField]
        private PWBMouseShortcut _tilingEditSpacing1 = new PWBMouseShortcut("Edit spacing on axis 1", PWBShortcut.Group.TILING,
           EventModifiers.Control, PWBMouseCombination.MouseEvents.DRAG_R_H);
        [SerializeField]
        private PWBMouseShortcut _tilingEditSpacing2 = new PWBMouseShortcut("Edit spacing on axis 2", PWBShortcut.Group.TILING,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.DRAG_R_H);
        public PWBMouseShortcut tilingEditSpacing1 => _tilingEditSpacing1;
        public PWBMouseShortcut tilingEditSpacing2 => _tilingEditSpacing2;
        #endregion

        #region PALETTE
        [SerializeField]
        private PWBMouseShortcut _paletteNextBrush = new PWBMouseShortcut("Select next brush",
            PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
           EventModifiers.Control | EventModifiers.Shift, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        [SerializeField]
        private PWBMouseShortcut _paletteNextPalette = new PWBMouseShortcut("Select next palette",
            PWBShortcut.Group.PALETTE | PWBShortcut.Group.GLOBAL,
           EventModifiers.Control | EventModifiers.Alt | EventModifiers.Shift, PWBMouseCombination.MouseEvents.SCROLL_WHEEL);
        public PWBMouseShortcut paletteNextBrush => _paletteNextBrush;
        public PWBMouseShortcut paletteNextPalette => _paletteNextPalette;
        #endregion

        #region CONFLICTS
        private PWBMouseShortcut[] _mouseShortcuts = null;
        public PWBMouseShortcut[] mouseShortcuts
        {
            get
            {
                if (_mouseShortcuts == null)
                    _mouseShortcuts = new PWBMouseShortcut[]
                    {
                        /*/// PIN ///*/
                        _pinScale,
                        _pinSelectNextItem,

                        _pinRotateAroundY,
                        _pinRotateAroundYSnaped,
                        _pinRotateAroundX,
                        _pinRotateAroundXSnaped,
                        _pinRotateAroundZ,
                        _pinRotateAroundZSnaped,

                        _pinSurfDist,
                        /*/// RADIUS ///*/
                        _brushRadius,
                        /*/// BRUSH & GRAVITY ///*/
                        _brushDensity,
                        _brushRotate,
                        /*/// BRUSH & GRAVITY ///*/
                        _gravitySurfDist,
                        /*/// LINE & SHAPE ///*/
                        _lineEditGap,
                        /*/// LINE ///*/
                        
                        /*/// TILING ///*/
                        tilingEditSpacing1,
                        tilingEditSpacing2,
                        /*/// PALETTE ///*/
                        _paletteNextBrush,
                        _paletteNextPalette

                    };
                return _mouseShortcuts;
            }
        }

        public void UpdateMouseConficts()
        {
            foreach (var scrollShortcut in mouseShortcuts) scrollShortcut.conflicted = false;
            for (int i = 0; i < mouseShortcuts.Length; ++i)
            {
                var shortcut1 = mouseShortcuts[i];
                if (shortcut1.conflicted) continue;
                if (shortcut1.combination.modifiers == EventModifiers.None) continue;
                for (int j = i + 1; j < mouseShortcuts.Length; ++j)
                {
                    var shortcut2 = mouseShortcuts[j];
                    if (shortcut2.conflicted) continue;
                    if (shortcut2.combination.modifiers == EventModifiers.None) continue;
                    if ((shortcut1.group & shortcut2.group) == 0 && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0
                        && (shortcut1.group & PWBShortcut.Group.GLOBAL) == 0) continue;
                    if (shortcut1.combination == shortcut2.combination)
                    {
                        shortcut1.conflicted = true;
                        shortcut2.conflicted = true;
                    }
                }
            }
        }

        public bool CheckMouseConflicts(PWBMouseCombination combi, PWBMouseShortcut target, out string conflicts)
        {
            conflicts = string.Empty;
            foreach (var shortcut in mouseShortcuts)
            {
                if (target == shortcut) continue;
                if (target.combination.modifiers == EventModifiers.None
                    || shortcut.combination.modifiers == EventModifiers.None) continue;
                if (combi == shortcut.combination && ((target.group & shortcut.group) != 0
                    || (shortcut.group & PWBShortcut.Group.GLOBAL) != 0 || (target.group & PWBShortcut.Group.GLOBAL) != 0))
                {
                    if (conflicts != string.Empty) conflicts += "\n";
                    conflicts += shortcut.name;
                }
            }
            return conflicts != string.Empty;
        }

        public bool CombinationExist(PWBMouseCombination combi, PWBShortcut.Group group)
        {
            foreach (var shortcut in mouseShortcuts)
            {
                if (combi == shortcut.combination && ((group & shortcut.group) != 0
                    || (shortcut.group & PWBShortcut.Group.GLOBAL) != 0 || (group & PWBShortcut.Group.GLOBAL) != 0))
                    return true;
            }
            return false;
        }
        #endregion
        #endregion
    }
    #endregion

    #region SETTINGS
    [System.Serializable]
    public class PWBSettings
    {
        #region COMMON
        private static string _settingsPath = null;
        private static PWBSettings _instance = null;
        private PWBSettings() { }

        private static PWBSettings instance
        {
            get
            {
                if (_instance == null) _instance = new PWBSettings();
                return _instance;
            }
        }
        private static string settingsPath
        {
            get
            {
                if (_settingsPath == null)
                    _settingsPath = System.IO.Directory.GetParent(Application.dataPath) + "/ProjectSettings/PWBSettings.txt";
                return _settingsPath;
            }
        }
        private void LoadFromFile()
        {
            if (!System.IO.File.Exists(settingsPath))
            {
                var files = System.IO.Directory.GetFiles(Application.dataPath,
                        PWBData.FULL_FILE_NAME, System.IO.SearchOption.AllDirectories);
                if (files.Length > 0) _dataDir = System.IO.Path.GetDirectoryName(files[0]);
                else
                {
                    _dataDir = Application.dataPath + "/" + PWBData.RELATIVE_DATA_DIR;
                    System.IO.Directory.CreateDirectory(_dataDir);
                }
                Save();
            }
            else
            {
                var settings = JsonUtility.FromJson<PWBSettings>(System.IO.File.ReadAllText(settingsPath));
                _dataDir = settings._dataDir;
                _shortcutProfiles = settings._shortcutProfiles;
                _selectedProfileIdx = settings._selectedProfileIdx;
            }
        }

        private void Save()
        {
            var jsonString = JsonUtility.ToJson(this);
            System.IO.File.WriteAllText(settingsPath, jsonString);
        }
        #endregion

        #region DATA DIR
        [SerializeField] private string _dataDir = null;
        private static bool _movingDir = false;
        public static bool movingDir => _movingDir;
        private static void CheckDataDir()
        {
            if (instance._dataDir == null) instance.LoadFromFile();
        }

        public static string dataDir
        {
            get
            {
                CheckDataDir();
                if (!System.IO.Directory.Exists(instance._dataDir))
                {
                    if (instance._dataDir.Replace("\\", "/").Contains(PWBData.RELATIVE_DATA_DIR))
                    {
                        var directories = System.IO.Directory.GetDirectories(Application.dataPath, PWBData.DATA_DIR,
                            System.IO.SearchOption.AllDirectories)
                            .Where(d => d.Replace("\\", "/").Contains(PWBData.RELATIVE_DATA_DIR)).ToArray();
                        if (directories.Length > 0)
                        {
                            instance._dataDir = directories[0].Replace("\\", "/");
                            instance.Save();
                            PaletteManager.instance.LoadPaletteFiles();
                            PrefabPalette.UpdateTabBar();
                        }
                    }
                }
                return instance._dataDir;
            }
            set
            {
                if (instance._dataDir == value) return;
                var currentDir = instance._dataDir;
                var newDir = value;
                void DeleteMeta(string path)
                {
                    var metapath = path + ".meta";
                    if (System.IO.File.Exists(metapath)) System.IO.File.Delete(metapath);
                }

                bool DeleteIfEmpty(string dirPath)
                {
                    if (System.IO.Directory.GetFiles(dirPath).Length != 0) return false;
                    System.IO.Directory.Delete(dirPath);
                    DeleteMeta(dirPath);
                    return true;
                }
                if (System.IO.Directory.Exists(currentDir))
                {
                    _movingDir = true;
                    var currentDataPath = currentDir + "/" + PWBData.FULL_FILE_NAME;
                    if (System.IO.File.Exists(currentDataPath))
                    {
                        var newDataPath = newDir + "/" + PWBData.FULL_FILE_NAME;
                        if (System.IO.File.Exists(newDataPath)) System.IO.File.Delete(newDataPath);
                        DeleteMeta(currentDataPath);
                        System.IO.File.Move(currentDataPath, newDataPath);

                        var currentPalettesDir = currentDir + "/" + PWBData.PALETTES_DIR;
                        if (System.IO.Directory.Exists(currentPalettesDir))
                        {
                            var newPalettesDir = newDir + "/" + PWBData.PALETTES_DIR;
                            if (!System.IO.Directory.Exists(newPalettesDir))
                                System.IO.Directory.CreateDirectory(newPalettesDir);
                            var palettesPaths = System.IO.Directory.GetFiles(currentPalettesDir, "*.txt");
                            foreach (var currentPalettePath in palettesPaths)
                            {
                                var fileName = System.IO.Path.GetFileName(currentPalettePath);
                                var newPalettePath = newPalettesDir + "/" + fileName;
                                if (System.IO.File.Exists(newPalettePath)) System.IO.File.Delete(newPalettePath);
                                DeleteMeta(currentPalettePath);

                                var paletteText = System.IO.File.ReadAllText(currentPalettePath);
                                var palette = JsonUtility.FromJson<PaletteData>(paletteText);
                                palette.filePath = newPalettePath;

                                System.IO.File.Move(currentPalettePath, newPalettePath);
                                System.IO.File.Delete(currentPalettePath);
                            }
                        }
                        if (DeleteIfEmpty(currentPalettesDir)) DeleteIfEmpty(currentDir);
                        UnityEditor.AssetDatabase.Refresh();
                    }
                    _movingDir = false;
                }
                instance._dataDir = value;
                instance.Save();
                PaletteManager.instance.LoadPaletteFiles();
                PrefabPalette.UpdateTabBar();
            }
        }
        #endregion

        #region SHORTCUTS
        [SerializeField]
        private System.Collections.Generic.List<PWBShortcuts> _shortcutProfiles
           = new System.Collections.Generic.List<PWBShortcuts>()
           {
                PWBShortcuts.GetDefault(0),
                PWBShortcuts.GetDefault(1)
           };
        [SerializeField] private int _selectedProfileIdx = 0;
        private PWBShortcuts selectedProfile
        {
            get
            {
                if (_selectedProfileIdx < 0 || _selectedProfileIdx > _shortcutProfiles.Count) _selectedProfileIdx = 0;
                return _shortcutProfiles[_selectedProfileIdx];
            }
        }

        public static PWBShortcuts shortcuts
        {
            get
            {
                CheckDataDir();
                return instance.selectedProfile;
            }
        }

        public static string[] shotcutProfileNames
        {
            get
            {
                CheckDataDir();
                return instance._shortcutProfiles.Select(p => p.profileName).ToArray();
            }
        }

        public static int selectedProfileIdx
        {
            get
            {
                CheckDataDir();
                return instance._selectedProfileIdx;
            }
            set
            {
                CheckDataDir();
                instance._selectedProfileIdx = value;
            }
        }

        public static void UpdateShrotcutsConflictsAndSaveFile()
        {
            CheckDataDir();
            shortcuts.UpdateConficts();
            shortcuts.UpdateMouseConficts();
            instance.Save();
        }

        public static void SetDefaultShortcut(int shortcutIdx, int defaultIdx)
        {
            CheckDataDir();
            if (shortcutIdx < 0 || shortcutIdx > instance._shortcutProfiles.Count) return;
            instance._shortcutProfiles[shortcutIdx] = PWBShortcuts.GetDefault(defaultIdx);
        }

        public static void ResetSelectedProfile()
        {
            CheckDataDir();
            if (selectedProfileIdx == 1) instance._shortcutProfiles[1] = PWBShortcuts.GetDefault(1);
            else instance._shortcutProfiles[instance._selectedProfileIdx] = PWBShortcuts.GetDefault(0);
        }

        public static void ResetShortcutToDefault(PWBKeyShortcut shortcut)
        {
            var defaultProfile = selectedProfileIdx == 1 ? PWBShortcuts.GetDefault(1) : PWBShortcuts.GetDefault(0);
            foreach (var ds in defaultProfile.keyShortcuts)
            {
                if (ds.group == shortcut.group && ds.name == shortcut.name)
                {
                    shortcut.combination.Set(ds.combination.keycode, ds.combination.modifiers);
                    return;
                }
            }
        }

        public static void ResetShortcutToDefault(PWBMouseShortcut shortcut)
        {
            var defaultProfile = selectedProfileIdx == 1 ? PWBShortcuts.GetDefault(1) : PWBShortcuts.GetDefault(0);
            foreach (var ds in defaultProfile.mouseShortcuts)
            {
                if (ds.group == shortcut.group && ds.name == shortcut.name)
                {
                    shortcut.combination.Set(ds.combination.modifiers, ds.combination.mouseEvent);
                    return;
                }
            }
        }

        public static void ResetShortcutToDefault(PWBShortcut shortcut)
        {
            if (shortcut is PWBKeyShortcut) ResetShortcutToDefault(shortcut as PWBKeyShortcut);
            else if (shortcut is PWBMouseShortcut) ResetShortcutToDefault(shortcut as PWBMouseShortcut);
        }

        #endregion
    }
    #endregion

    #region HANDLERS
    [UnityEditor.InitializeOnLoad]
    public static class ApplicationEventHandler
    {
        private static bool _hierarchyLoaded = false;
        public static bool hierarchyLoaded => _hierarchyLoaded;
        static ApplicationEventHandler()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnStateChanged;
            UnityEditor.EditorApplication.quitting += PWBCore.staticData.Save;
            UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
        private static void OnHierarchyChanged()
        {
            if (!_hierarchyLoaded)
            {
                _hierarchyLoaded = true;
                return;
            }
            if (!PWBCore.staticData.saving) PWBCore.LoadFromFile();
            UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private static void OnStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode
                || state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                PWBCore.staticData.SaveIfPending();
        }
    }

    public class DataReimportHandler : UnityEditor.AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (PWBSettings.movingDir) return;
            if (PWBCore.staticData.saving) return;
            if (PaletteManager.selectedPalette != null && PaletteManager.selectedPalette.saving) return;
            if (!PWBData.palettesDirectory.Contains(Application.dataPath)) return;
            var paths = new System.Collections.Generic.List<string>(importedAssets);
            paths.AddRange(deletedAssets);
            paths.AddRange(movedAssets);
            paths.AddRange(movedFromAssetPaths);

            var relativeDataPath = PWBSettings.dataDir.Replace(Application.dataPath, string.Empty);

            if (paths.Exists(p => p.Contains(relativeDataPath)))
            {
                PaletteManager.instance.LoadPaletteFiles();
                if (PrefabPalette.instance != null) PrefabPalette.instance.Reload();
                return;
            }
        }
    }

    #endregion

    #region AUTOSAVE
    [UnityEditor.InitializeOnLoad]
    public static class AutoSave
    {
        private static int _quickSaveCount = 3;

        static AutoSave()
        {
            PWBCore.staticData.UpdateRootDirectory();
            PeriodicSave();
            PeriodicQuickSave();
        }
        private async static void PeriodicSave()
        {
            if (PWBCore.staticDataWasInitialized)
            {
                await System.Threading.Tasks.Task.Delay(PWBCore.staticData.autoSavePeriodMinutes * 60000);
                PWBCore.staticData.SaveIfPending();
            }
            else await System.Threading.Tasks.Task.Delay(60000);
            PeriodicSave();
        }

        private async static void PeriodicQuickSave()
        {
            await System.Threading.Tasks.Task.Delay(300);
            ++_quickSaveCount;
            if (_quickSaveCount == 3 && PWBCore.staticDataWasInitialized) PWBCore.staticData.Save();
            PeriodicQuickSave();
        }

        public static void QuickSave() => _quickSaveCount = 0;
    }
    #endregion

    #region VERSION
    [System.Serializable]
    public class PWBDataVersion
    {
        [SerializeField] public string _version;
        public bool IsOlderThan(string value) => IsOlderThan(value, _version);

        public static bool IsOlderThan(string value, string referenceValue)
        {
            var intArray = GetIntArray(referenceValue);
            var otherIntArray = GetIntArray(value);
            var minLength = Mathf.Min(intArray.Length, otherIntArray.Length);
            for (int i = 0; i < minLength; ++i)
            {
                if (intArray[i] < otherIntArray[i]) return true;
                else if (intArray[i] > otherIntArray[i]) return false;
            }
            return false;
        }
        private static int[] GetIntArray(string value)
        {
            var stringArray = value.Split('.');
            if (stringArray.Length == 0) return new int[] { 1, 0 };
            var intArray = new int[stringArray.Length];
            for (int i = 0; i < intArray.Length; ++i) intArray[i] = int.Parse(stringArray[i]);
            return intArray;
        }
    }
    #endregion

    #region DATA 1.9
    [System.Serializable]
    public class V1_9_LineData
    {
        [SerializeField] public LinePoint[] _controlPoints;
        [SerializeField] public bool _closed;
    }

    [System.Serializable]
    public class V1_9_PersistentLineData
    {
        [SerializeField] public long _id;
        [SerializeField] public long _initialBrushId;
        [SerializeField] public V1_9_LineData _data;
        [SerializeField] public LineSettings _settings;
        [SerializeField] public ObjectPose[] _objectPoses;
    }

    [System.Serializable]
    public class V1_9_SceneLines
    {
        [SerializeField] public string _sceneGUID;
        [SerializeField] public V1_9_PersistentLineData[] _lines;
    }

    [System.Serializable]
    public class V1_9_Profile
    {
        [SerializeField] public V1_9_SceneLines[] _sceneLines;
    }

    [System.Serializable]
    public class V1_9_LineManager
    {
        [SerializeField] public V1_9_Profile _unsavedProfile;
    }

    [System.Serializable]
    public class V1_9_PWBData
    {
        [SerializeField] public V1_9_LineManager _lineManager;
    }
    #endregion

    #region DATA 2.8
    [System.Serializable]
    public class V2_8_PaletteManager
    {
        [SerializeField] public PaletteData[] _paletteData;
    }

    [System.Serializable]
    public class V2_8_PWBData
    {
        [SerializeField] public V2_8_PaletteManager _paletteManager;
    }
    #endregion
}