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

public class AudioSeperationTool : EditorWindow {

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
                buttonChosePath.text = "ѡ��Ŀ¼";
            } else {
                buttonChosePath.text = "ѡ���ļ�";
            }
            buttonChosePath.clicked += () => {
                if (folder) {
                    Path = EditorUtility.OpenFolderPanel("ѡ��Ŀ¼", "", "");
                } else {
                    Path = EditorUtility.OpenFilePanel("ѡ���ļ�", "", "");
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

    private static bool _isRunning = false;

    private static bool _debug = false;

    private Task _currentTask;

    #endregion

    #region UI
    [MenuItem("Tools/TDTools/��Ƶ����/��Ƶ��ȡ����")]
    public static void ShowWindow() {
        AudioSeperationTool wnd = GetWindow<AudioSeperationTool>();
        wnd.titleContent = new GUIContent("��Ƶ��ȡ����");
    }

    public void CreateGUI() {
        VisualElement root = rootVisualElement;

        Toolbar bar = new Toolbar();
        bar.style.flexShrink = 0;
        root.Add(bar);

        VisualElement toolContainer = new VisualElement();
        toolContainer.style.borderBottomWidth = 0.5f;
        toolContainer.style.borderBottomColor = Color.black;
        root.Add(toolContainer);

        _ffmpegPath = new PathTextfield("FFmpegĿ¼", true, "VOC_AUTO_CAPTURE_FFMPEG");
        toolContainer.Add(_ffmpegPath.root);

        _animationFolder = new PathTextfield("����ԴĿ¼", true);
        toolContainer.Add(_animationFolder.root);

        _outputFolder = new PathTextfield("���Ŀ¼", true);
        toolContainer.Add(_outputFolder.root);

        ToolbarButton testRun = new ToolbarButton();
        testRun.text = "��ʼ����";
        testRun.clicked += async () => {
            if (!_isRunning) {
                _isRunning = true;
                try {
                    _currentTask = Run();
                    await _currentTask;
                } catch (Exception e) {
                    Debug.Log($"<color=red>{e.Message}\n{e.StackTrace}</color>");
                }
                //Debug.LogWarning("Finishing");
                _isRunning = false;
            }
        };
        bar.Add(testRun);

        ToolbarToggle toggle = new ToolbarToggle();
        toggle.text = "��ʾ����Log";
        toggle.RegisterValueChangedCallback(obj => {
            _debug = obj.newValue;
        });
        _debug = toggle.value;
        bar.Add(toggle);

        ToolbarButton buttonStop = new ToolbarButton();
        buttonStop.text = "��ֹ����(�����������Ƶ����)";
        buttonStop.clicked += () => {
            //Debug.LogWarning("Clicked");
            _isRunning = false;
        };
        bar.Add(buttonStop);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        _isRunning = false;
    }

    private void OnEnable() {
        _isRunning = false;
    }

    #endregion

    List<FileInfo> GetAllFiles(DirectoryInfo rootDir, string searchPattern) {
        List<FileInfo> result = new List<FileInfo>();
        result.AddRange(rootDir.GetFiles(searchPattern));
        var dirs = rootDir.GetDirectories();
        for (int i = 0; i < dirs.Length; i++) {
            result.AddRange(GetAllFiles(dirs[i], searchPattern));
        }

        return result;
    }

    async Task Run() {
        int progressID = Progress.Start("��Ƶ��ȡ�ᴿ...");
        var files = GetAllFiles(new DirectoryInfo(_animationFolder.Path), "*.mp4");
        Directory.CreateDirectory(_outputFolder.Path);
        for (int i = 0; i < files.Count; i++) {
            if (!_isRunning) {
                break;
            }
            Progress.Report(progressID, i / (float)files.Count);
            var subID = Progress.Start(files[i].Name, files[i].FullName, Progress.Options.None, progressID);
            await Cut(files[i].FullName, $@"{_outputFolder.Path}\{files[i].Name.Substring(0, files[i].Name.IndexOf("."))}");
            Progress.Remove(subID);
        }
        Progress.Remove(progressID);
    }

    async Task Cut(string path, string name) {
        await Task.Run(() => {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.Arguments = $@"-y -i {path} -vn {name}.mp3";
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
}