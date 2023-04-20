using EditorNode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TDTools.SkillEditorTemplate {

    /// <summary>
    /// 技能模板类
    /// </summary>
    public class SkillEditorTemplate {

        /// <summary>
        /// 模板名
        /// </summary>
        public string TemplateName;

        /// <summary>
        /// 模板描述
        /// </summary>
        public string TemplateDescription;

        /// <summary>
        /// 模板作者
        /// </summary>
        public string TemplateAuthor;

        /// <summary>
        /// 模板的节点数量
        /// </summary>
        public string TemplateNodeCount;


        /// <summary>
        /// 模板的类型
        /// </summary>
        public string TemplateType;

        /// <summary>
        /// 模板的文件路径
        /// </summary>
        public string path;

        /// <summary>
        /// 模板的节点连线
        /// </summary>
        public List<SkillConnectionData> Connections;


        /// <summary>
        /// 模板的所有节点
        /// </summary>
        public BaseSkillNode[] Nodes;
    }
}