#if UNITY_EDITOR

namespace BuildReportTool.Window
{
	public static class Labels
	{
		// GUI messages, labels

		public const string WAITING_FOR_BUILD_TO_COMPLETE_MSG =
			"正在等待生成完成…如果不在焦点中，请单击此窗口进行刷新.";

		public const string NO_BUILD_INFO_FOUND_MSG =
			"无生成信息。\n\n单击“获取日志”可从编辑器日志中检索上次生成信息。\n\n单击“打开”可手动打开以前保存的生成报告文件。";

		public const string FOUND_NO_LOG_ARGUMENT_MSG =
			"警告：Unity是使用-nolog参数启动的。如果没有日志，则生成报告工具无法获取生成信息。若要生成生成报告，请在不使用-nlog参数的情况下重新启动Unity。";


		public const string MONO_DLLS_LABEL = "系统DLL：";
		public const string UNITY_ENGINE_DLLS_LABEL = "UnityEngine DLL：";
		public const string SCRIPT_DLLS_LABEL = "脚本DLL：";


		public const string TIME_OF_BUILD_LABEL = "构建时间：";


		public const string UNUSED_TOTAL_SIZE_LABEL = "未使用的总资产大小：";
		public const string USED_TOTAL_SIZE_LABEL = "使用的总资产大小：";
		public const string STREAMING_ASSETS_TOTAL_SIZE_LABEL = "Streaming\nAssets Size:";
		public const string BUILD_TOTAL_SIZE_LABEL = "总生成大小：";
		public const string BUILD_XCODE_SIZE_LABEL = "XCODE项目文件夹的大小";

		public const string WEB_UNITY3D_FILE_SIZE_LABEL = "UNITY3D文件的大小：";

		public const string ANDROID_APK_FILE_SIZE_LABEL = ".APK文件大小：";
		public const string ANDROID_OBB_FILE_SIZE_LABEL = ".OBB文件大小：";


		public const string UNUSED_TOTAL_SIZE_DESC = "生成中未包含的项目资产的总大小。";
		public const string UNUSED_TOTAL_IS_FROM_BATCH = "由于<b><color=white>未使用资产批处理</color></b>，此大小仅占项目中的前{0:N0}个资产。您可以从“选项”（在<b><color=white>“未使用资产列表”</color></b>部分下）关闭此选项，然后重新生成生成报告。";

		public const string USED_TOTAL_SIZE_DESC =
			"打包前使用的资产的总大小。\n还包括已编译的Mono脚本的大小。\n不包括StreamingAssets中的文件大小。";

		public const string STREAMING_ASSETS_SIZE_DESC = "StreamingAssets文件夹中所有文件的总大小。";


		public const string BUILD_SIZE_STANDALONE_DESC =
			"可执行文件和附带的Data文件夹的文件大小。";

		public const string BUILD_SIZE_WINDOWS_DESC = ".exe文件及其附带的Data文件夹的文件大小。";
		public const string BUILD_SIZE_MACOSX_DESC = ".app文件的文件大小。";

		public const string BUILD_SIZE_LINUX_UNIVERSAL_DESC =
			"32位和64位可执行文件的文件大小，以及附带的Data文件夹。";

		public const string BUILD_SIZE_WEB_DESC = "整个WEB构建文件夹的文件大小。";

		public const string BUILD_SIZE_IOS_DESC = "Xcode项目文件夹的文件大小。";

		public const string BUILD_SIZE_ANDROID_DESC = "生成的.apk文件的文件大小。";
		public const string BUILD_SIZE_ANDROID_WITH_OBB_DESC = "生成的.apk和.OBB文件的文件大小。";
		public const string BUILD_SIZE_ANDROID_WITH_PROJECT_DESC = "生成的Eclipse项目文件夹的文件大小。";


		public const string OPEN_SERIALIZED_BUILD_INFO_TITLE = "打开生成报告XML文件";

		public const string TOTAL_SIZE_BREAKDOWN_LABEL = "已用资产大小细分：";

		public const string ASSET_SIZE_BREAKDOWN_LABEL = "资产细分：";


		public const string ASSET_SIZE_BREAKDOWN_MSG_PRE_BOLD = "排序依据";
		public const string ASSET_SIZE_BREAKDOWN_MSG_BOLD = "未压缩";

		public const string ASSET_SIZE_BREAKDOWN_MSG_POST_BOLD =
			"大小。单击资产的名称可将其包括在大小计算或批量删除中。按住Shift键并单击可选择多个。按住Ctrl键并单击以切换选择。";

		public const string TOTAL_SIZE_BREAKDOWN_MSG_PRE_BOLD = "基于";
		public const string TOTAL_SIZE_BREAKDOWN_MSG_BOLD = "未压缩";
		public const string TOTAL_SIZE_BREAKDOWN_MSG_POST_BOLD = "构建大小";


		public const string NO_FILES_FOR_THIS_CATEGORY_LABEL = "无";

		public const string NON_APPLICABLE_PERCENTAGE_LABEL = "N/A";

		public const string OVERVIEW_CATEGORY_LABEL = "概述";
		public const string BUILD_SETTINGS_CATEGORY_LABEL = "项目设置";
		public const string SIZE_STATS_CATEGORY_LABEL = "总大小";
		public const string USED_ASSETS_CATEGORY_LABEL = "已用资产";
		public const string UNUSED_ASSETS_CATEGORY_LABEL = "未使用的资产";
		public const string OPTIONS_CATEGORY_LABEL = "选项";
		public const string HELP_CATEGORY_LABEL = "帮助和信息";


		public const string REFRESH_LABEL = "获取日志";
		public const string OPEN_LABEL = "打开";
		public const string SAVE_LABEL = "保存";

		public const string SAVE_MSG = "将构建报告保存为XML";

		public const string RECALC_RAW_SIZES = "重新计算原始大小";
		public const string RECALC_IMPORTED_SIZES = "重新计算导入的大小";
		public const string RECALC_SIZE_BEFORE_BUILD = "生成前重新计算大小";

		public const string SELECT_ALL_LABEL = "全选";
		public const string SELECT_NONE_LABEL = "选择无";
		public const string SELECTED_QTY_LABEL = "选定： ";
		public const string SELECTED_SIZE_LABEL = "总大小： ";
		public const string SELECTED_PERCENT_LABEL = "总百分比： ";

		public const string BUILD_TYPE_PREFIX_MSG = "For ";
		public const string BUILD_TYPE_SUFFIX_MSG = "";
		public const string UNITY_VERSION_PREFIX_MSG = ", 内置 ";

		public const string COLLECT_BUILD_INFO_LABEL =
			"生成后自动收集并保存生成信息（不包括批处理模式生成）";

		public const string SHOW_AFTER_BUILD_LABEL = "生成后自动显示生成报告窗口";
		public const string INCLUDE_SVN_LABEL = "创建未使用资产列表时包含SVN元数据";
		public const string INCLUDE_GIT_LABEL = "创建未使用资产列表时包含GIT元数据";
		public const string INCLUDE_BRT_LABEL = "创建未使用资产列表时包括生成报告工具资产";
		public const string FILE_FILTER_DISPLAY_TYPE_LABEL = "将文件过滤器绘制为：";

		public const string FILE_FILTER_DISPLAY_TYPE_DROP_DOWN_LABEL = "下拉框";
		public const string FILE_FILTER_DISPLAY_TYPE_BUTTONS_LABEL = "按钮";

		public const string SAVE_PATH_LABEL = "当前生成报告保存路径： ";
		public const string SAVE_FOLDER_NAME_LABEL = "生成报告的文件夹名称：";
		public const string SAVE_PATH_TYPE_LABEL = "保存生成报告：";

		public const string SAVE_PATH_TYPE_PERSONAL_DEFAULT_LABEL = "在user文件夹中";
		public const string SAVE_PATH_TYPE_PERSONAL_WIN_LABEL = "在My Documents文件夹中";
		public const string SAVE_PATH_TYPE_PERSONAL_MAC_LABEL = "在Home文件夹";
		public const string SAVE_PATH_TYPE_PROJECT_LABEL = "项目文件夹旁边";

		public const string EDITOR_LOG_LABEL = "Unity Editor.log path ";
		public const string DEFAULT_EDITOR_LOG_NOT_FOUND_MSG = "警告：未找到Unity编辑器日志文件。";

		public const string OVERRIDE_LOG_NOT_FOUND_MSG =
			"找不到指定的日志文件。请单击“设置覆盖日志”更改路径";

		public const string SET_OVERRIDE_LOG_LABEL = "设置覆盖日志";
		public const string CLEAR_OVERRIDE_LOG_LABEL = "清除覆盖日志";

		public const string FILTER_GROUP_TO_USE_LABEL = "要使用的文件筛选器组：";
		public const string FILTER_GROUP_FILE_PATH_LABEL = "配置的文件筛选器组： ";

		public const string FILTER_GROUP_TO_USE_CONFIGURED_LABEL = "始终使用配置的文件过滤器组";

		public const string FILTER_GROUP_TO_USE_EMBEDDED_LABEL =
			"使用文件中嵌入的文件筛选器组（如果可用）\n（打开生成报告文件时）";

		public const string OPEN_IN_FILE_BROWSER_DEFAULT_LABEL = "在文件浏览器中打开";
		public const string OPEN_IN_FILE_BROWSER_WIN_LABEL = "Show in Explorer";
		public const string OPEN_IN_FILE_BROWSER_MAC_LABEL = "Reveal in Finder";


		public const string CALCULATION_LEVEL_FULL_NAME = "4-完整报告（完整计算）";

		public const string CALCULATION_LEVEL_FULL_DESC =
			"计算所有内容。将显示大小细分、“已用资源”和“未用资源”列表。\n\n如果您有一个大型项目，场景中有数千个文件或对象，这可能会很慢。如果内存不足，请尝试较低的计算级别。";

		public const string CALCULATION_LEVEL_NO_PREFAB_NAME = "“3-不计算未使用的预制件";

		public const string CALCULATION_LEVEL_NO_PREFAB_DESC =
			"将计算所有内容，但它不会确定预制是否未使用。它仍将显示哪些其他资源未使用。\n\n如果您有使用数百到数千个预制的场景，并且在生成生成报告时出现内存不足错误，请尝试此设置。";

		public const string CALCULATION_LEVEL_NO_UNUSED_NAME = "2-不计算未使用的资产";

		public const string CALCULATION_LEVEL_NO_UNUSED_DESC =
			"将仅显示概览数据和“已用资产”列表。它不会确定哪些资产未使用。\n\n它不会在“已用资源”列表中显示流式资产文件，但它们的总大小仍将显示在概览中。";

		public const string CALCULATION_LEVEL_MINIMAL_NAME = "1-仅概述（最小计算）";

		public const string CALCULATION_LEVEL_MINIMAL_DESC =
			"仅显示概览数据。这是最快的，但显示的信息最少。";
	}
}

#endif