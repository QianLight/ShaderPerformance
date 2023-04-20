using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using UnityEngine;
using System.IO;
using TableEditor;

namespace LevelEditor
{
    class LevelCmdGenerator
    {
        //private static LevelCmdGenerator instance;

        //public static LevelCmdGenerator Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //            instance = new LevelCmdGenerator();
        //        return instance;
        //    }
        //}

        public static bool GenerateLevelCmdCode()
        {
            if(LevelEditor.Instance==null)
            {
                Debug.LogError("level editor not inited");
                return false;
            }
            try
            {
                TableFullData configData = new TableFullData();
                TableReader.ReadTable(Application.dataPath + "/Editor/Blueprint/LevelEditor/LevelNodeConfig.txt", ref configData);

                CodeTypeDeclaration levelCmdClass = new CodeTypeDeclaration("LevelCmdGather");
                CodeCompileUnit levelCmdUnit = new CodeCompileUnit();
                CodeNamespace _nameSpace = new CodeNamespace("CFClient");
                levelCmdClass.IsClass = true;
                levelCmdClass.IsPartial = false;
                levelCmdClass.TypeAttributes = TypeAttributes.Public;
                _nameSpace.Types.Add(levelCmdClass);
                levelCmdUnit.Namespaces.Add(_nameSpace);
                levelCmdClass.Comments.Add(new CodeCommentStatement("生成代码 请勿手动修改"));
                
                for(var i=0;i<(int)LevelScriptCmd.Level_Cmd_Max;i++)
                {
                    var cmd = (LevelScriptCmd)i;

                    TableData config = configData.dataList.Find(d => d.valueList[0] == cmd.ToString());
                    if (config==null||config.valueList[2] == "1")//服务端节点 不生成客户端hash
                        continue;

                    CodeMemberField f = new CodeMemberField();
                    f.Attributes = (f.Attributes&~MemberAttributes.AccessMask&~MemberAttributes.ScopeMask)|MemberAttributes.Public|MemberAttributes.Const;
                    f.Name = cmd.ToString();
                    f.Type = new CodeTypeReference(typeof(uint));
                    var ip = new CodeFieldReferenceExpression();
                    ip.FieldName = CFUtilPoolLib.XCommon.singleton.XHash(cmd.ToString()).ToString();
                    f.InitExpression = ip;

                    if(i==0)
                        f.Comments.Add(new CodeCommentStatement("LevelScriptCmd枚举生成的指令hash值"));

                    levelCmdClass.Members.Add(f);                    
                }

                for(var i=0;i<LevelHelper.dList.CustomDefineNodes.Count;i++)
                {
                    string cmd = string.Format("Level_Cmd_{0}", LevelHelper.dList.CustomDefineNodes[i].name);


                    TableData config = configData.dataList.Find(d => d.valueList[1] == LevelHelper.dList.CustomDefineNodes[i].name);
                    if (config == null || config.valueList[2] == "1")//服务端节点 不生成客户端hash
                        continue;

                    CodeMemberField f = new CodeMemberField();
                    f.Attributes = (f.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Public | MemberAttributes.Const;
                    f.Name = cmd.ToString();
                    f.Type = new CodeTypeReference(typeof(uint));
                    var ip = new CodeFieldReferenceExpression();
                    ip.FieldName = CFUtilPoolLib.XCommon.singleton.XHash(cmd.ToString()).ToString();
                    f.InitExpression = ip;

                    if (i == 0)
                        f.Comments.Add(new CodeCommentStatement("xml配置生成的指令hash值"));

                    levelCmdClass.Members.Add(f);
                }

                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CodeGeneratorOptions options = new CodeGeneratorOptions
                {
                    BracingStyle = "C"
                };
                DirectoryInfo di = new DirectoryInfo(Application.dataPath);
                string codePath = di.Parent.Parent.Parent.FullName + "\\src\\client\\CFClient\\CFClient\\Spwan\\LevelCmd.cs";
                using (StreamWriter writer = new StreamWriter(codePath))
                {
                    provider.GenerateCodeFromCompileUnit(levelCmdUnit,writer,options);
                }

                return true;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            
        }
    }
}
