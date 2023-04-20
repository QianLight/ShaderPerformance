using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Scene", "�����й̶�λ����Ⱦ��Ϣ���,����only�����߼��ڷ������ϼ�����û����", "t:scene", "")]
    public class SceneStandPositionCheck : RuleBase, CSVOutput
    {
        [PublicParam("triangles", eGUIType.Input)]
        public int maxTriangles = 100000;
        [PublicParam("batch", eGUIType.Input)]
        public int maxBatch = 500;
        [PublicParam("drawcall", eGUIType.Input)]
        public int maxDrawcall = 200;
        [PublicParam("setpasscall", eGUIType.Input)]
        public int maxSetpasscall = 500;
        class TransformMat
        {
            public Vector3 pos;
            public Quaternion rotation;
            public string nodeName;
        }

        class OutputInfo
        {
            public string sceneName;
            public string nodePosName;
            public Vector3 cameraPos;
            public Quaternion cameraRotation;
            public int triangles;
            public int batch;
            public int drawcall;
            public int setpasscall;

            public List<string> ToLine(int maxTriangles, int maxBatch, int maxDrawcall, int maxSetpasscall)
            {
                List<string> line = new List<string>();
                line.Add(sceneName);
                line.Add(nodePosName);
                line.Add($"{cameraPos.x}��{cameraPos.y}��{cameraPos.z}");
                line.Add($"{cameraRotation.eulerAngles.x}��{cameraRotation.eulerAngles.y}��{cameraRotation.eulerAngles.z}");
                if(triangles > maxTriangles)
                {
                    line.Add($"{triangles} > {maxTriangles}");
                }
                else
                {
                    line.Add(string.Empty);
                }
                if (batch > maxBatch)
                {
                    line.Add($"{batch} > {maxBatch}");
                }
                else
                {
                    line.Add(string.Empty);
                }
                if (drawcall > maxDrawcall)
                {
                    line.Add($"{drawcall} > {maxDrawcall}");
                }
                else
                {
                    line.Add(string.Empty);
                }
                if (setpasscall > maxSetpasscall)
                {
                    line.Add($"{setpasscall} > {maxSetpasscall}");
                }
                else
                {
                    line.Add(string.Empty);
                }
                return line;
            }
        }
        private Action<bool, string, string, string> actionAsyncMethod;
        private string scenePath = string.Empty;
        private Queue<TransformMat> listCameraMat = new Queue<TransformMat>();
        private Camera camera;
        private OutputInfo currentOutputInfo;
        private List<OutputInfo> allStats = new List<OutputInfo>();
        private int CheckPosCount = 0;
        public int currentCheckStateFrame = 0;
        public List<List<string>> ResultOutput(out string fileName)
        {
            fileName = "ScenePosRenderCheck";
            List<List<string>> array = new List<List<string>>();
            List<string> title = new List<string>();
            title.Add("��������");
            title.Add("�ڵ�����");
            title.Add("λ��");
            title.Add("��ת");
            title.Add("triangles");
            title.Add("batch");
            title.Add("drawcall");
            title.Add("setpasscall");
            array.Add(title);
            int faildTriangles = 0;
            int faildBatch = 0;
            int faildDrawcall = 0;
            int faildSetpasscall = 0;
            foreach (var output in allStats)
            {
                array.Add(output.ToLine(maxTriangles, maxBatch, maxDrawcall, maxSetpasscall));
                if (output.triangles > maxTriangles)
                    faildTriangles++;
                if (output.batch > maxBatch)
                    faildBatch++;
                if (output.drawcall > maxDrawcall)
                    faildDrawcall++;
                if (output.setpasscall > maxSetpasscall)
                    faildSetpasscall++;
            }
            List<string> stats = new List<string>();
            stats.Add("�ܼ�");
            stats.Add(string.Empty);
            stats.Add(string.Empty);
            stats.Add(string.Empty);
            stats.Add($"{faildTriangles}/{CheckPosCount}\t");
            stats.Add($"{faildBatch}/{CheckPosCount}\t");
            stats.Add($"{faildDrawcall}/{CheckPosCount}\t");
            stats.Add($"{faildSetpasscall}/{CheckPosCount}\t");
            array.Add(stats);
            return array;
        }

        void Clear()
        {
            listCameraMat.Clear();
            camera = null;
            currentOutputInfo = null;
            currentCheckStateFrame = 0;
        }

        [PublicMethod(true)]
        public bool Check(string path, Action<bool, string, string, string> callback)
        {
            Clear();
            scenePath = path;
            actionAsyncMethod = callback;

            AssetHelper.OpenScene(path);

            camera = Camera.main;
            if (camera == null)
            {// û������������ǲ����Ե�
                TaskEnd();
                return true;
            }
            GameObject standRoot = GameObject.Find("SceneMeshTest");
            if (standRoot == null)
            { // û�л�λ�Ľڵ�
                TaskEnd();
                return true;
            }
            Transform[] tfs = standRoot.GetComponentsInChildren<Transform>(true);
            if (tfs.Length == 0)
            { // ��λ����Ϊ0
                TaskEnd();
                return true;
            }
            foreach(var tf in tfs)
            {
                listCameraMat.Enqueue(new TransformMat() { pos = tf.position, rotation = tf.rotation, nodeName = tf.name });
            }
            CheckPosCount += tfs.Length;
            currentCheckStateFrame = 0;
            NextPos();
            return true;
        }

        void Update()
        {
            currentCheckStateFrame++;

            if (currentCheckStateFrame == 4)
            {
                currentOutputInfo.triangles = UnityEditor.UnityStats.triangles;
                currentOutputInfo.batch = UnityEditor.UnityStats.batches;
                currentOutputInfo.drawcall = UnityEditor.UnityStats.drawCalls;
                currentOutputInfo.setpasscall = UnityEditor.UnityStats.setPassCalls;

                if (currentOutputInfo.triangles > maxTriangles
                    || currentOutputInfo.batch > maxBatch
                    || currentOutputInfo.drawcall > maxDrawcall
                    || currentOutputInfo.setpasscall > maxSetpasscall)
                {
                    allStats.Add(currentOutputInfo);
                }
            }

            if(currentCheckStateFrame >= 5)
            {
                currentCheckStateFrame = 0;
                NextPos();
            }
        }

        void NextPos()
        {
            if (listCameraMat.Count == 0)
            {
                TaskEnd();
                return;
            }
            TransformMat mat = listCameraMat.Dequeue();
            camera.transform.position = mat.pos;
            camera.transform.rotation = mat.rotation;
            currentOutputInfo = new OutputInfo();
            currentOutputInfo.nodePosName = mat.nodeName;
            currentOutputInfo.sceneName = scenePath;
            currentOutputInfo.cameraPos = mat.pos;
            currentOutputInfo.cameraRotation = mat.rotation;
        }

        void TaskEnd()
        {
            actionAsyncMethod(true, scenePath, this.GetType().Name, string.Empty);
            AssetHelper.BackLastScene();
        }
    }
}