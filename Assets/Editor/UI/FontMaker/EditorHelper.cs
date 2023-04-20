using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class EditorHelper : MonoBehaviour {

	[MenuItem("Assets/UI/批量生成位图字体")]
	static public void BatchCreateArtistFont()
	{
		ArtistFont.BatchCreateArtistFont();
	}
}
