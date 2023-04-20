using CFEngine.Editor;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

public class XGuideWindow:EditorWindow
{
    [MenuItem("Tools/Guide/GuideWindow")]
    public static void OpenGuideWindow()
    {
        EditorWindow window = GetWindow<XGuideWindow>(@"Guide Editor");
        window.position = new Rect(300, 200, 1000, 500);
        window.wantsMouseMove = true;
        window.Show();
        window.Repaint();
    }

    
    private XDataSoDrawer dataSo = null;
    private XProcssSoDrawer processSo = null;


    private bool ShowCatalogue = true;

    protected virtual void OnEnable()
    {
        ShowCatalogue = true;
        XGuideSoUtility.LoadSkin();
        dataSo = new XDataSoDrawer();
        dataSo.Begin(this);
        processSo = new XProcssSoDrawer();
        processSo.Begin(this);
    }

    protected virtual void OnDisable()
    {
        if (!ShowCatalogue)
        {
            if(processSo != null)
            {
                processSo.Save();
                processSo.End();
                processSo = null;
            }
        }

        if(dataSo != null)
        {
            dataSo.Save();
            dataSo.End();
            dataSo = null;
        }
    }

    public void Back()
    {
        ShowCatalogue = true;
        if (processSo != null)
            processSo.Save();
    }

    public void Go(XDataSO so)
    {
        processSo.SetData(so.script);
        processSo.Begin(this);
        ShowCatalogue = false;
    }

    public void OnGUI()
    {
        
        if (ShowCatalogue) 
        {
            dataSo?.OnGUI(position);
        }
        else
        {
            processSo?.OnGUI(position);
        }
    }

}

class XProcssSoDrawer:XDrawer
{

    private static XGuideProcessSO copy = null;
    [MenuItem("Assets/Guide/Create ProcessSo")]
    public static void CreateGuideProcessSO()
    {
        XProcessListSo so = new XProcessListSo();
        DataIO.SerializeData("Assets/Table/Guide/NewProcess.bytes", so);
    }

    private string url = string.Empty;
    private XProcessListSo dataSo = new XProcessListSo();

    public void SetData(string name)
    {
        url = string.Format("Assets/Table/Guide/{0}.bytes", name);
        dataSo.name = name;
        XProcessListSo so = DataIO.DeserializeData<XProcessListSo>(url);
        if (so != null)
        {
            dataSo.processSo.Clear();
            dataSo.processSo.AddRange(so.processSo);
        }
    }

    public override void ToBinary()
    {
        dataSo.EditorToData();
        EditorUtility.DisplayDialog("export", string.Format("Export {0} sucess!", url), "ok");
    }

    public override void Begin(XGuideWindow window)
    {
        base.Begin(window);
        SetSerializedObject(new ReorderableList(dataSo.processSo, typeof(XProcessListSo), true, true, true, false));
    }

    public override void Save()
    {
        DataIO.SerializeData(url, dataSo);
        XDebug.singleton.AddBlueLog(string.Format("Save {0} Sucuss!",url));
    }

    protected override void Sorting()
    {
        dataSo.processSo.Sort(Compare);
    }

    private int Compare(XGuideProcessSO f, XGuideProcessSO l)
    {
        return f.step - l.step;
    }

    protected override void Sort(int index)
    {
        if(index < dataSo.processSo.Count)
        {
            dataSo.processSo[index].Sort();
        }
    }

    protected override void Copy(int index)
    {
        if (index < dataSo.processSo.Count)
        {
            copy = dataSo.processSo[index];
        }
    }

    protected override void Paste(int index)
    {
        XGuideProcessSO paste = null;
        if (index < dataSo.processSo.Count)
        {
            paste = dataSo.processSo[index];
        }

        if (copy == null || paste == null)
        {
            EditorUtility.DisplayDialog("copy", "no copy!", "ok");
            return;
        }

        paste.CopyTo(copy);
        copy = null;
        EditorUtility.DisplayDialog("copy", "copy seccuss!", "ok");

    }

    protected override void drawHeaderCallback(Rect rect)
    {
        base.drawHeaderCallback(rect);
        Rect backRect = new Rect(rect)
        {
            x = 220,
            y = 3,
            width = 60,
            height = 20
        };
        if (GUI.Button(backRect, new GUIContent("Back")))
        {
            root.Back();
            return;
        }
    }


    protected override float elementHeightCallback(int index)
    {
        if (dataSo == null) return 80;
        float ns = 0;
        if (index < dataSo.processSo.Count)
        {
            var ele = dataSo.processSo[index].others;
            ns = ele.Count * (EditorGUIUtility.singleLineHeight + space);
        }
        return base.elementHeightCallback(index) + ns;
    }

    protected override void RemoveDataSo(int index)
    {
        if (dataSo.processSo.Count > index)
        {
            dataSo.processSo.RemoveAt(index);
        }
    }

    protected override void DrawDataSo(Rect rect, int index)
    {
        XGuideProcessSO so = dataSo.processSo[index];
        DrawDataSo(rect, so);
    }

    private void DrawDataSo(Rect position, XGuideProcessSO so)
    {
        EditorGUIUtility.labelWidth = 60;
        position.height = EditorGUIUtility.singleLineHeight;

        Rect idRect = new Rect(position)
        {
            width = 100,
            x = position.x + 10
        };

        Rect moduleRect = new Rect(idRect)
        {
            x = idRect.x + idRect.width + 10,
            width = 400
        };

        Rect othersRect = new Rect(idRect)
        {
            y = idRect.y + EditorGUIUtility.singleLineHeight + 2,
            width = position.width - 80
        };
        so.step = EditorGUI.IntField(idRect, "Step", so.step);
        so.module = EditorGUI.TextField(moduleRect, "Module", so.module);
        DrawOthers(othersRect, so.others);
    }
}

class XDataSoDrawer : XDrawer
{
    protected static XDataSO copy = null;
    [MenuItem("Assets/Guide/Create DataSo")]
    public static void CreateGuideProcessSO()
    {
        XDataListSo so = new XDataListSo();
        DataIO.SerializeData("Assets/Table/Guide/GuideTable.bytes", so);
    }


    private XDataListSo dataSo = null;
    private string dataSoUrl = "Assets/Table/Guide/GuideTable.bytes";
    
  

    public override void Begin( XGuideWindow window)
    {
        base.Begin(window);
        dataSo = DataIO.DeserializeData<XDataListSo>(dataSoUrl);
        if (dataSo == null)
        {
            dataSo = new XDataListSo();
        }
        SetSerializedObject(new ReorderableList(dataSo.dataSo, typeof(XDataSO), true, true, true, false));
    }


    public override void Save()
    {
        DataIO.SerializeData(dataSoUrl, dataSo);
        XDebug.singleton.AddBlueLog("Save GuideTable Sucuss!");
    }

    public override void ToBinary()
    {
        dataSo.EditorToData();
        EditorUtility.DisplayDialog("export", "export seccuss!", "ok");
    }

    protected override void Sorting()
    {
        dataSo.dataSo.Sort(Compare);
    }

    private int Compare( XDataSO f, XDataSO l)
    {
        return f.id - l.id;
    }

    protected override void Sort(int index)
    {
        if (index < dataSo.dataSo.Count)
        {
            dataSo.dataSo[index].Sort();
        }
    }

    protected override void Copy(int index)
    {
        if(index < dataSo.dataSo.Count)
        {
            copy = dataSo.dataSo[index];
        }
    }

    protected override void Paste(int index)
    {
        XDataSO paste = null;
        if (index < dataSo.dataSo.Count)
        {
             paste  = dataSo.dataSo[index];
        }
        
        if(copy == null || paste == null)
        {
            EditorUtility.DisplayDialog("copy", "no copy!", "ok");
            return;
        }

        paste.CopyTo(copy);
        copy = null;
        EditorUtility.DisplayDialog("copy", "copy seccuss!", "ok");

    }

    protected override void drawHeaderCallback(Rect rect)
    {
  
        base.drawHeaderCallback(rect);
        Rect backRect = new Rect(rect)
        {
            x = 230,
            y = 3,
            width = 500,
            height = 20
        };
        EditorGUI.LabelField(backRect, "目录  说明:[不使用,触发条件,完成条件,失败条件]有效");

        Rect allToBinary = new Rect(rect)
        {
            x = rect.width - 230,
            y = 3,
            width = 100,
            height = 20
        };
        if (GUI.Button(allToBinary, "All Export"))
        {
            AllToBinary();
        }
    }



    private void AllToBinary()
    {
        int total = dataSo.dataSo.Count;
        XProcessListSo so = null;
        XDataSO dSo = null;
        for (int i = 0; i < total;i++) {
         
            dSo = dataSo.dataSo[i];
            if (string.IsNullOrEmpty(dSo.script))
            {
                XDebug.singleton.AddBlueLog(string.Format("XData So [{0}:{1}] Script is Null", dSo.id, dSo.module));
            }
            else
            {
                so = DataIO.DeserializeData<XProcessListSo>(string.Format("Assets/Table/Guide/{0}.bytes",dSo.script));
                if(so == null)
                {
                    XDebug.singleton.AddErrorLog(string.Format("not found ! [{0}:{1}--{2}]", dSo.id, dSo.module,dSo.script));
                }
                else
                {
                    so.EditorToData();
                }
                so = null;
            }
            EditorUtility.DisplayProgressBar("ToBinary", " All Binary Success!", (float)i / total);
        }
        EditorUtility.ClearProgressBar();
    }

    protected override float elementHeightCallback(int index)
    {
        float ns = 0;
        if (index < dataSo.dataSo.Count)
        {
            var ele = dataSo.dataSo[index].others;
            ns = ele.Count * (EditorGUIUtility.singleLineHeight + space);
        }
        return base.elementHeightCallback(index)+ns;
    }

    protected override void RemoveDataSo(int index)
    {
        if(dataSo.dataSo.Count > index)
        {
            dataSo.dataSo.RemoveAt(index);
        }
    }

    protected override void DrawDataSo(Rect rect, int index)
    {
        XDataSO so = dataSo.dataSo[index];
        DrawDataSo(rect, so);
    }
    
 

    private void DrawDataSo(Rect position, XDataSO so)
    {
        EditorGUIUtility.labelWidth = 60;
        position.height = EditorGUIUtility.singleLineHeight;

        Rect idRect = new Rect(position)
        {
            width = 100,
            x = position.x + 10
        };
        Rect moduleRect = new Rect(idRect)
        {
            width = 400,
            x = idRect.width + 10 + idRect.x,
        };
        Rect scriptUrlRect = new Rect(moduleRect)
        {
            x = position.x + 10,
            y = moduleRect.y + EditorGUIUtility.singleLineHeight + 2,
            width = position.width - 200
        };

        Rect othersRect = new Rect(scriptUrlRect)
        {
            y = scriptUrlRect.y + EditorGUIUtility.singleLineHeight + 2,
            width = position.width - 80
        };

        Rect selRect = new Rect(scriptUrlRect)
        {
            x = scriptUrlRect.x + scriptUrlRect.width + 20,
            width = 60,
            height = 20
        };

        Rect sro = new Rect(selRect)
        {
            x = selRect.x + selRect.width + 20,
            width = 60,
            height = 20
        };
        so.id = EditorGUI.IntField(idRect, "ID", so.id);
        so.module = EditorGUI.TextField(moduleRect, "Module", so.module);
     
        EditorGUI.LabelField(scriptUrlRect, "Script", string.IsNullOrEmpty(so.script)?"":string.Format("Assets/Table/Guide/{0}.bytes", so.script));
        if (GUI.Button(selRect, "Select"))
        {
            string url = "Assets/Table/Guide";
            string path = EditorUtility.OpenFilePanel("Select Process So", url, "bytes");
            if (!string.IsNullOrEmpty(path))
            {
                int start = path.LastIndexOf('/')+1;
                int end = path.LastIndexOf('.');
                if(start > 1 && end > 0 && end > start)
                {
                    so.script = path.Substring(start, end - start);
                }
                else
                {
                    so.script = string.Empty;
                    XDebug.singleton.AddErrorLog("ProcessSo Url"+ path);
                }
            }
        }

        if (string.IsNullOrEmpty(so.script))
        {

            if (GUI.Button(sro, "Create"))
            {
                XProcessListSo spo = new XProcessListSo();
                spo.name = "Process_" + so.id;
                so.script = spo.name;
                DataIO.SerializeData(string.Format("Assets/Table/Guide/{0}.bytes", spo.name),spo);
                return;
            }
        }
        else
        {
            if (GUI.Button(sro, "Go"))
            {
                root.Go(so);
            }
        }
        DrawOthers(othersRect, so.others);
    }
}
class  XDrawer
{

    protected ReorderableList reorderable = null;
    public static float space = 2f;
    protected XGuideWindow root;
        
    public virtual void Begin(XGuideWindow window) {
        root = window;
    }

    protected void Clean()
    {
        reorderable.drawElementBackgroundCallback = null;
        reorderable.drawElementCallback = null; 
        reorderable.elementHeightCallback = null;
        reorderable.drawHeaderCallback = null;
        reorderable = null;
    }

    public virtual void Save()
    {

    }

    public virtual void End() {
        Clean();
    }

    public virtual void ToBinary()
    {

    }

    protected virtual void Sorting(){

    }

    protected virtual void Sort(int index)
    {

    }

    protected virtual void Copy(int index)
    {

    }

    protected virtual void Paste(int index)
    {

    }

    protected void SetSerializedObject(ReorderableList rb)
    {
        reorderable = rb;
        reorderable.elementHeight = 100;
        reorderable.footerHeight = 10;
        reorderable.drawElementBackgroundCallback = drawElementBackgroundCallback;
        reorderable.drawElementCallback = drawElementCallback;
        reorderable.elementHeightCallback = elementHeightCallback;
        reorderable.drawHeaderCallback = drawHeaderCallback;
    }

    protected virtual void drawHeaderCallback(Rect rect)
    {
        Rect saveRect = new Rect(rect)
        {
            x = rect.x + 10,
            y = rect.y + 3,
            width = 60,
            height = 20,
        };
        if(GUI.Button(saveRect , "Save"))
        {
            Save();
        }
        Rect binaryRect = new Rect(saveRect)
        {
            x = saveRect.x + saveRect.width + 10
        };

        if (GUI.Button(binaryRect, "Export"))
        {
            ToBinary();
        }

        Rect sortRect = new Rect(binaryRect)
        {
            x = binaryRect.x + binaryRect.width + 10
        };

        if(GUI.Button(sortRect,"Sort")){
            Sorting();
        }
        
    }

    protected virtual float elementHeightCallback(int index)
    {
        return 80;
    }

    private void drawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        Rect back = new Rect(rect)
        {
            x = rect.x - space,
            y = rect.y - space,
            width = rect.width - space,
            height = rect.height - space
        };
        EditorGUI.DrawRect(back, Color.gray);
    }
    public int m_deleteIndex = -1;
    protected void drawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.height -= 3f;
        EditorGUIUtility.labelWidth = 60;
        rect.height = EditorGUIUtility.singleLineHeight;
        Rect b = rect;
        b.x = rect.width - 10;
        b.width = 20;
        b.height = 20;
        if (GUI.Button(b, new GUIContent("-")))
        {
            m_deleteIndex = index;
            return;
        }

        b.x -= 22;
        if(GUI.Button(b,new GUIContent("p")))
        {
            Paste(index);
        }

        b.x -= 22;
        if(GUI.Button(b,new GUIContent("c")))
        {
            Copy(index);
        }

        b.x -= 22;
        if(GUI.Button(b,new GUIContent("s")))
        {
            Sort(index);
        }
        b.x -= 120;
        DrawDataSo(rect, index);
    }





    protected virtual  void DrawDataSo(Rect rect, int index)
    {

    }

    protected virtual void RemoveDataSo( int index )
    {

    }

    protected void DrawOthers(Rect Position, List<XTriDataSo> so)
    {
        Rect rect = new Rect(Position)
        {
            width = 100,
            height = EditorGUIUtility.singleLineHeight

        };
        EditorGUI.LabelField(rect, "addition:");

        Rect addRect = new Rect(Position)
        {
            x = rect.x + 120,
            height = 20,
            width = 20,
        };

        if (GUI.Button(addRect, "+"))
        {
            so.Add(new XTriDataSo());
        }
        Rect prRect = new Rect(addRect);
        for (int i = 0; i < so.Count; i++)
        {
            prRect.y += addRect.height + 3;
            prRect.x = Position.x;
            prRect.height = 20;
            prRect.width = 20;
            if (GUI.Button(prRect, "-"))
            {
                so.RemoveAt(i);
                break;
            }

            prRect.x += prRect.width + 10;
            prRect.width = Position.width - 100;
            DrawOthers(prRect, so[i]);
        }
    }


    protected void DrawOthers(Rect rect, XTriDataSo so)
    {
        Rect fr = rect;
        fr.width = 120;
        so.func = EditorGUI.Popup(fr, so.func, XGuideSoUtility.func_strs);
        Rect fvr = fr;
        fvr.x += fr.width + 10;
        so.func_key = EditorGUI.Popup(fvr, so.func_key, XGuideSoUtility.GetFunKeys(so.func));
        Rect fsr = fvr;
        fsr.x += fr.width + 10;
        fsr.width = rect.width - 400;
        so.func_value = EditorGUI.TextField(fsr, so.func_value);
        GUIContent content = XGuideSoUtility.GetFunDesc(so.func, so.func_key);
        Rect fss = fsr;
        fss.x += fsr.width + 10;
        fsr.width = 250;
        EditorGUI.LabelField(fss, content);
    }

    private Vector2 m_scrollPos;

    public virtual void OnGUI(Rect position)
    {
        if(m_deleteIndex > -1)
        {
            RemoveDataSo(m_deleteIndex);
            m_deleteIndex = -1;
        }
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, true);
        reorderable.DoLayoutList();
        EditorGUILayout.Space(40);
        EditorGUILayout.EndScrollView();
    }
}