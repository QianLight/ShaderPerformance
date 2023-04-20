using CFClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {

    [System.Serializable]
    public class Connection {
        public ResourceHyperlink From;
        public InspectorElement To;

        public Connection(ResourceHyperlink from, InspectorElement to) {
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// 显示节点
    /// </summary>
    [System.Serializable]
    public class InspectorElement {
        public VisualElement Handle;
        public VisualElement Root;
        public VisualElement Container;
        protected string currentRow;
        public string CurrentColumn;
        public Label TitleLabel;
        public NodeType InspectorNodeType; 

        private bool _isDragging;

        Vector2 _targetStartPosition;
        Vector3 _pointerStartPosition;

        public Vector2 Position;

        [SerializeField]
        private ResourceReferenceInspector _owner;
        
        [SerializeField]
        static readonly Color borderColorNormal = Color.black;
        [SerializeField]
        static readonly Color borderColorHightlight = Color.yellow;
        [SerializeField]
        static readonly Color borderColorFreeze = Color.green;

        string filterText = "";
        public string CurrnetMouseOver = "";

        public string ID = "";
        public string Title;

        bool freezeFilter = false;

        public ResourceHyperlink LinkedFrom;
        public Connection ConnectionFrom;
        public VisualElement Pin;

        public List<Connection> Connections;

        public string LinkedID;

        public InspectorElement(ResourceReferenceInspector owner, Vector2 position, string id, string linkedID = "") {
            _owner = owner;
            Connections = new List<Connection>();
            Position = position;
            ID = id;
            LinkedID = linkedID;
            CreateUI();
        }

        public virtual void CreateUI() {
            Handle = new Toolbar();
            Handle.style.backgroundColor = Color.black;
            Handle.style.justifyContent = Justify.SpaceBetween;

            _owner.OnDrag += OnOwnerDrag;

            Container = new VisualElement();

            Root = new VisualElement();
            Root.style.position = UnityEngine.UIElements.Position.Absolute;
            Root.Add(Handle);
            Root.Add(Container);

            SetBorderColor(borderColorNormal);
            Root.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            Root.transform.position = Position + _owner.Offsets;

            Container.RegisterCallback<PointerDownEvent>(e => {
                SetBorderColor(borderColorHightlight);
                _owner.SelectNode(this);
            });

            Container.RegisterCallback<PointerUpEvent>(e => {
                Container.ReleasePointer(e.pointerId);
            });

            Root.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            Root.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            Root.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            Root.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);

            Root.RegisterCallback<ContextClickEvent>(obj => {
                var menu = ContextClick(obj);
                menu.ShowAsContext();
            });

            VisualElement titlePinVE = new VisualElement();
            titlePinVE.style.flexDirection = FlexDirection.Row;
            Handle.Add(titlePinVE);

            Image image = new Image();
            image.image = new Texture2D(12, 12);
            image.style.marginLeft = 2f;
            image.style.marginRight = 2f;
            image.scaleMode = ScaleMode.ScaleToFit;
            Pin = image;
            titlePinVE.Add(image);

            TitleLabel = new Label(Title);

            TitleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            titlePinVE.Add(TitleLabel);

            VisualElement buttonsVE = new VisualElement();
            buttonsVE.style.flexDirection = FlexDirection.Row;
            Handle.Add(buttonsVE);

            Button buttonClose = new Button();
            buttonClose.text = "X";
            buttonClose.style.flexShrink = 0;
            buttonClose.style.flexGrow = 0;
            buttonClose.style.width = 16;
            buttonClose.style.height = 16;
            buttonClose.clicked += Close;

            Button buttonMin = new Button();
            buttonMin.text = "-";
            bool min = false;
            buttonMin.style.flexShrink = 0;
            buttonMin.style.flexGrow = 0;
            buttonMin.style.width = 16;
            buttonMin.style.height = 16;
            buttonMin.clicked += () => {
                min = !min;
                if (min) {
                    Container.style.display = DisplayStyle.None;
                    int t = UnityEngine.Random.Range(0, 100);
                    if (t > 97)
                        buttonMin.text = "囧";
                    else
                        buttonMin.text = "口";
                } else {
                    Container.style.display = DisplayStyle.Flex;
                    buttonMin.text = "-";
                }
            };

            buttonsVE.Add(buttonMin);
            buttonsVE.Add(buttonClose);
            BuildContent();
        }

        public virtual void BuildContent() {
            Container.Clear();
        }

        public void SetBorderColor(Color color) {
            Root.style.borderBottomColor = color;
            Root.style.borderTopColor = color;
            Root.style.borderLeftColor = color;
            Root.style.borderRightColor = color;
            Root.style.borderTopWidth = 1f;
            Root.style.borderRightWidth = 1f;
            Root.style.borderBottomWidth = 1f;
            Root.style.borderLeftWidth = 1f;
        }

        protected virtual GenericMenu ContextClick(ContextClickEvent evt) {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("关闭显示"), false, Close);
            menu.AddItem(new GUIContent("冻结筛选"), freezeFilter, ()=> { 
                freezeFilter = !freezeFilter;
                if (!freezeFilter) {
                    BuildContent();
                    SetBorderColor(borderColorNormal);
                } else {
                    SetBorderColor(borderColorFreeze);
                }
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("查找引用"), false, () => { FindAllReference(); });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent($"复制: {ID.Replace("/", " \u2215 ")}"), false, () => { CopyToClipBoard(ID); });
            if (CurrnetMouseOver != "") {
                string text = CurrnetMouseOver;
                menu.AddItem(new GUIContent($"复制: {text.Replace("/", " \u2215 ")}"), false, () => { CopyToClipBoard(text); });
            }
            AddGMMenu(menu, InspectorNodeType, ID);
            return menu;
            //menu.ShowAsContext();
        }

        public virtual void Close() {
            _owner.RemoveNode(this);
        }

        protected bool Match(string text) {
            if (filterText == "")
                return true;
            string target = text.ToLower();
            string[] s = filterText.Split(' ');
            for (int i = 0; i < s.Length; i++)
                if (target.Contains(s[i]))
                    return true;
            return false;
        }

        public void SetFilterText(string text) {
            filterText = text.ToLower();
            if (!freezeFilter)
                BuildContent();
        }

        void CopyToClipBoard(string value) {
            GUIUtility.systemCopyBuffer = value;
        }

        protected async virtual void FindAllReference() {
            await Task.Yield();
        }

        public void UnSelect() {
            if (!freezeFilter)
                SetBorderColor(borderColorNormal);
            else
                SetBorderColor(borderColorFreeze);
        }

        public async Task ScanReference(ReferenceRecord key) {
            var wnd = EditorWindow.GetWindow<ResourceReferenceScanner>("引用扫描", false, typeof(ResourceReferenceInspector));
            EditorWindow.GetWindow<ResourceReferenceInspector>().Focus();

            if (_owner.ReferenceRecords == null) {
                await wnd.ScanAll();
                _owner.ReferenceRecords = wnd.Reference;
            }

            if (wnd.Reference.ContainsKey(key)) {
                var list = wnd.Reference[key];
                _owner.SetScanResult(list);
            }
        }

        public static void AddGMMenu(GenericMenu menu, NodeType type, string id) {
#if USE_GM
            TableNode table;
            Dictionary<string, string> row;
            switch (type) {
                case NodeType.Map:
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent($"GM指令: mm {id}"), false, () => {
                        CFCommand.singleton.ProcessClientCommand($"mm {id}");
                    });
                    break;
                case NodeType.Enemy:
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent($"GM指令: mob {id}"), false, () => {
                        CFCommand.singleton.ProcessServerCommand($"mob {id}");
                    });
                    break;
                case NodeType.Buff:
                    menu.AddSeparator("");
                    string[] s = id.Split('=');
                    menu.AddItem(new GUIContent($"GM指令: buff {s[0]} {s[1]}"), false, () => {
                        CFCommand.singleton.ProcessServerCommand($"buff {s[0]} {s[1]}");
                    });
                    break;
                case NodeType.Scene:
                    table = TableDatabase.Instance.GetTable("SceneList", "SceneID");
                    row = table.Table[table.Dic[id]];
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent($"GM指令: mm {row["MapID"]}"), false, () => {
                        CFCommand.singleton.ProcessClientCommand($"mm {row["MapID"]}");
                    });
                    break;
                case NodeType.PartnerSkill:
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent($"GM指令: fireskill {id}"), false, () => {
                        CFCommand.singleton.ProcessClientCommand($"fireskill {id}");
                    });
                    break;
                case NodeType.Partner:
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent($"GM指令: partner add {id}"), false, () => {
                        CFCommand.singleton.ProcessServerCommand($"partner add {id}");
                    });
                    break;
                case NodeType.XEntitySkill:
                    table = TableDatabase.Instance.GetTable("SkillListForEnemy", "SkillScript");
                    row = table.Table[table.Dic[id]];
                    if (row["XEntityStatisticsID"] != "") {
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent($"GM指令: mob {row["XEntityStatisticsID"]}"), false, () => {
                            CFCommand.singleton.ProcessServerCommand($"mob {row["XEntityStatisticsID"]}");
                        });
                        menu.AddItem(new GUIContent($"GM指令: monstercastskill {row["XEntityStatisticsID"]} {id}"), false, () => {
                            CFCommand.singleton.ProcessServerCommand($"monstercastskill {row["XEntityStatisticsID"]} {id}");
                        });
                    }
                    break;
            }
#endif
        }

        #region 处理拖拽

        void OnOwnerDrag() {
            Root.transform.position = Position + _owner.Offsets;
        }

        void PointerDownHandler(PointerDownEvent evt) {
            if (evt.button == 0) {
                _pointerStartPosition = evt.position;
                _targetStartPosition = Root.transform.position;
                Root.CapturePointer(evt.pointerId);
                _isDragging = true;
            }
        }
        void PointerMoveHandler(PointerMoveEvent evt) {
            if (_isDragging && Root.HasPointerCapture(evt.pointerId)) {
                Vector3 pointerDelta = evt.position - _pointerStartPosition;

                Root.transform.position = new Vector2(
                    _targetStartPosition.x + pointerDelta.x * (1 /_owner.Zoom),
                    _targetStartPosition.y + pointerDelta.y * (1 / _owner.Zoom));

                Position = Root.transform.position - new Vector3(_owner.Offsets.x, _owner.Offsets.y);
            }
        }
        void PointerUpHandler(PointerUpEvent evt) {
            Root.ReleasePointer(evt.pointerId);
            _isDragging = false;
        }
        void PointerCaptureOutHandler(PointerCaptureOutEvent evt) {
            _isDragging = false;
        }

        #endregion

        public static InspectorElement CreateElement(ResourceReferenceInspector owner, NodeType type, Vector2 position, string id, string linkedID = "") {
            InspectorElement node;
            switch (type) {
                case NodeType.Buff:
                    node = new BuffNode(owner, position, id);
                    break;
                case NodeType.Enemy:
                    node = new EnemyWidget(owner, position, id);
                    break;
                case NodeType.XEntitySkill:
                    node = new SkillForEnemyWidget(owner, position, id, linkedID);
                    break;
                case NodeType.AI:
                    node = new UnitAITaableWidget(owner, position, id, linkedID);
                    break;
                case NodeType.PartnerSkill:
                    node = new SkillForRoleWidget(owner, position, id);
                    break;
                case NodeType.Partner:
                    node = new PartnerWidget(owner, position, id);
                    break;
                case NodeType.XEntityPresentation:
                    node = new PresentationWidget(owner, position, id);
                    break;
                case NodeType.Map:
                    node = new MapWidget(owner, position, id);
                    break;
                case NodeType.Scene:
                    node = new SceneWidget(owner, position, id);
                    break;
                case NodeType.Texture2D:
                    node = new ImageWidget(owner, position, id);
                    break;
                case NodeType.Asset:
                    node = new AssetWidget(owner, position, id);
                    break;
                case NodeType.FMOD:
                    node = new FMODWidget(owner, position, id);
                    break;
                case NodeType.Animation:
                    node = new AnimationWidget(owner, position, id);
                    break;
                case NodeType.BeHit:
                    node = new BeHitWidget(owner, position, id);
                    break;
                case NodeType.Folder:
                    System.Diagnostics.Process.Start("explorer.exe", id);
                    return null;
                case NodeType.LevelScript:
                    node = new AssetWidget(owner, position, id);
                    break;
                default:
                    node = new InspectorElement(owner, position, id);
                    break;
            }

            return node;
        }
    }

}