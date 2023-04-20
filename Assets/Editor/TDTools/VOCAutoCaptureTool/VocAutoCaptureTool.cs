using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System;
using System.Threading;

public class VocAutoCaptureTool : EditorWindow
{

    #region UI Class
    class PathTextfield {
        public VisualElement root;
        string prefs;

        public string Path { 
            get {
                return _pathTextfield.value;
            }

            set {
                _pathTextfield.value = value;
            }
        }

        TextField _pathTextfield;

        public PathTextfield(string label, bool folder = false, string prefs = "") {
            this.prefs = prefs;

            root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.flexGrow = 100;

            Button buttonChosePath = new Button();
            if (folder) {
                buttonChosePath.text = "选择目录";
            } else {
                buttonChosePath.text = "选择文件";
            }
            buttonChosePath.clicked += () => {
                if (folder) {
                    Path = EditorUtility.OpenFolderPanel("选择目录", "", "");
                } else {
                    Path = EditorUtility.OpenFilePanel("选择文件", "", "");
                }
            };
            root.Add(buttonChosePath);

            _pathTextfield = new TextField();
            _pathTextfield.label = label;
            _pathTextfield.style.flexGrow = 100;
            if (prefs != "") {
                _pathTextfield.value = EditorPrefs.GetString(prefs);
                _pathTextfield.RegisterValueChangedCallback(obj => {
                    string newPath = obj.newValue;
                    if (folder)
                        newPath = @$"{newPath}\";
                    EditorPrefs.SetString(prefs, newPath);
                    _pathTextfield.SetValueWithoutNotify(newPath);
                });
            }
            root.Add(_pathTextfield);
        }
    }
    #endregion

    #region Variables
    private PathTextfield _animationFolder;
    private PathTextfield _outputFolder;
    private PathTextfield _ffmpegPath;
    private PathTextfield _tablePath;
    private PathTextfield _pythonPath;

    private static bool _isRunning = false;

    private static bool _debug = false;

    private Task _currentTask;

    #endregion

    #region UI
    [MenuItem("Tools/TDTools/音频工具/VOC自动截取提纯工具")]
    public static void ShowWindow()
    {
        VocAutoCaptureTool wnd = GetWindow<VocAutoCaptureTool>();
        wnd.titleContent = new GUIContent("VOC自动截取提纯工具");
    }

    public void CreateGUI(){
        VisualElement root = rootVisualElement;

        Toolbar bar = new Toolbar();
        bar.style.flexShrink = 0;
        root.Add(bar);

        VisualElement toolContainer = new VisualElement();
        toolContainer.style.borderBottomWidth = 0.5f;
        toolContainer.style.borderBottomColor = Color.black;
        root.Add(toolContainer);

        _pythonPath = new PathTextfield("Python执行文件目录", false, "VOC_AUTO_CAPTURE_PYTHON_PATH");
        toolContainer.Add(_pythonPath.root);

        _ffmpegPath = new PathTextfield("FFmpeg目录", true, "VOC_AUTO_CAPTURE_FFMPEG");
        toolContainer.Add(_ffmpegPath.root);

        _animationFolder = new PathTextfield("动画源目录", true, "VOC_AUTO_CAPTURE_ANIMATION_FOLDER");
        toolContainer.Add(_animationFolder.root);

        _outputFolder = new PathTextfield("输出目录", true, "VOC_AUTO_CAPTURE_OUTPUT_FOLDER");
        toolContainer.Add(_outputFolder.root);

        _tablePath = new PathTextfield("表格地址", false, "VOC_AUTO_CAPTURE_TABLE_PATH");
        toolContainer.Add(_tablePath.root);

        ToolbarButton testRun = new ToolbarButton();
        testRun.text = "开始运行";
        testRun.clicked += async ()=> {
            if (!_isRunning) {
                _isRunning = true;
                try {
                    _currentTask = TestRun();
                    await _currentTask;
                } catch (Exception e){
                    Debug.Log($"<color=red>{e.Message}\n{e.StackTrace}</color>");
                }
                //Debug.LogWarning("Finishing");
                _isRunning = false;
            }
        };
        bar.Add(testRun);

        ToolbarToggle toggle = new ToolbarToggle();
        toggle.text = "显示程序Log";
        toggle.RegisterValueChangedCallback(obj => {
            _debug = obj.newValue;
        });
        _debug = toggle.value;
        bar.Add(toggle);

        ToolbarButton buttonStop = new ToolbarButton();
        buttonStop.text = "终止运行(到现在这个音频结束)";
        buttonStop.clicked += () => {
            //Debug.LogWarning("Clicked");
            _isRunning = false;
        };
        bar.Add(buttonStop);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        //Debug.LogWarning("OnScriptsReloaded");
        _isRunning = false;
    }

    private void OnEnable() {
        //Debug.LogWarning("OnEnable");
        _isRunning = false;
    }

    #endregion

    List<Dictionary<string, string>> GetTable() {
        List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
        using var fs = new FileStream(_tablePath.Path, FileMode.Open ,FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs, Encoding.UTF8);
        string[] headers = sr.ReadLine().Split('\t');
        sr.ReadLine();
        sr.ReadLine();
        while (!sr.EndOfStream) {
            string[] lines = sr.ReadLine().Split('\t');
            var dic = new Dictionary<string, string>();
            for (int i = 0; i < lines.Length; i++) {
                dic[headers[i]] = lines[i];
            }
            result.Add(dic);
        }
        return result;
    }

    string TimeProcessing(string input) {
        string s = input.Replace("：", ":").Replace("；", ".");
        float frame = float.Parse(s.Substring(s.IndexOf(".") + 1));
        string time = (frame / 30).ToString(".00");
        s = s.Substring(0, s.IndexOf("."));
        s = $"{s}{time}";
        return s;
    }
    
    async Task TestRun() {
        var table = GetTable();
        int progressID = Progress.Start("音频截取提纯...");
        int count = table.Count;
        //Debug.LogWarning(count);
        Directory.CreateDirectory($@"{_outputFolder.Path}TEMP\");
        for (int i = 0; i < count; i++) {
            if (!_isRunning) {
                //Debug.LogWarning("Halting");
                break;
            }
            //Debug.LogWarning(i);
            var row = table[i];
            if (!row["Source"].Equals("动画"))
                continue;
            try {
                Directory.CreateDirectory($@"{_outputFolder.Path}Source\{row["Chapter"]}\");
                Directory.CreateDirectory($@"{_outputFolder.Path}VOC\{row["Chapter"]}\");
                Progress.Report(progressID, i / (float)count);
                Progress.SetDescription(progressID, row["AudioName"]);
                string start = TimeProcessing(row["Start"]);
                string end = TimeProcessing(row["End"]);
                string path = row["Episode"];
                path = $"{_animationFolder.Path}{path.Substring(path.IndexOf("第") + 1, path.IndexOf("集") - 1)}.mp4";
                await Cut(path, start, end, $@"{_outputFolder.Path}Source\{row["Chapter"]}\{row["AudioName"]}");
                await Seperate($@"{_outputFolder.Path}Source\{row["Chapter"]}\{row["AudioName"]}.wav");
                File.Copy($@"{_outputFolder.Path}TEMP\mdx_extra_q\{row["AudioName"]}\vocals.wav", $@"{_outputFolder.Path}VOC\{row["Chapter"]}\{row["AudioName"]}.wav", true);
            } catch (Exception e){
                Debug.Log($"<color=red>{e.Message}\n{row["AudioName"]}\n{e.StackTrace}</color>");
            }
        }
        Directory.Delete($@"{_outputFolder.Path}TEMP\", true);
        Progress.Remove(progressID);
    }

    async Task Cut(string path, string start, string end, string name) {
        await Task.Run(() => {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.Arguments = $@"-ss {start} -to {end} -y -i {path} -vn {name}.wav";
            startInfo.FileName = _ffmpegPath.Path + "ffmpeg.exe";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            using System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
            if (_debug) {
                process.ErrorDataReceived += (sender, e) => {
                    Debug.Log(e.Data);
                };
                process.OutputDataReceived += (sender, e) => {
                    Debug.Log(e.Data);
                };
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        });
    }

    async Task Seperate(string input) {
        await Task.Run(() => {
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
            start.Arguments = $@"-u -m demucs {input} --float32 -o {_outputFolder.Path}TEMP\";
            start.WorkingDirectory = _ffmpegPath.Path;
            start.FileName = _pythonPath.Path;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            using System.Diagnostics.Process process = System.Diagnostics.Process.Start(start);
            if (_debug) {
                process.ErrorDataReceived += (sender, e) => {
                    Debug.Log(e.Data);
                };
                process.OutputDataReceived += (sender, e) => {
                    Debug.Log(e.Data);
                };
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        });
    }
}