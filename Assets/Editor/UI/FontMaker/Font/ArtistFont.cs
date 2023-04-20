using UnityEngine;
using System.Collections;
using UnityEditor;

public class ArtistFont : MonoBehaviour
{	public static void BatchCreateArtistFont()
	{
		string dirName = "";
		string fntname = EditorUtils.SelectObjectPathInfo(ref dirName).Split('.')[0];
		Debug.Log(fntname);
		Debug.Log(dirName);

		string fntFileName = dirName + fntname + ".fnt";
		
		Font CustomFont = new Font();
		{
			AssetDatabase.CreateAsset(CustomFont, dirName + fntname + ".fontsettings");
			AssetDatabase.SaveAssets();
		}

		TextAsset BMFontText = null;
		{
			BMFontText = AssetDatabase.LoadAssetAtPath(fntFileName, typeof(TextAsset)) as TextAsset;
		}

		BMFont mbFont = new BMFont();
		BMFontReader.Load(mbFont, BMFontText.name, BMFontText.bytes);  // 借用NGUI封装的读取类
		CharacterInfo[] characterInfo = new CharacterInfo[mbFont.glyphs.Count];
		for (int i = 0; i < mbFont.glyphs.Count; i++)
		{
			BMGlyph bmInfo = mbFont.glyphs[i];
			CharacterInfo info = new CharacterInfo();
			info.index = bmInfo.index;
            info.glyphWidth = mbFont.texWidth;
            info.glyphHeight = mbFont.texHeight;
            float texWidth = (float)mbFont.texWidth;
            float texHeight = (float)mbFont.texHeight;
            float x = bmInfo.x;
            float y = bmInfo.y;
            float w = bmInfo.width;
            float h = bmInfo.height;
            info.uvTopLeft =  new Vector2(x/texWidth,1- (y/ texHeight));
            info.uvTopRight = new Vector2((x+w )/ texWidth, 1-(y/texHeight));
            info.uvBottomLeft = new Vector2(x/texWidth,1-( (y+h)/ texHeight));
            info.uvBottomRight = new Vector2((x+w) / texWidth,1-( (y + h)/ texHeight));
            info.minX = bmInfo.offsetX;
            info.minY = -bmInfo.height;
            info.maxX = bmInfo.width;
            info.maxY = 0;
            info.advance = bmInfo.advance;
			characterInfo[i] = info;
		}
		CustomFont.characterInfo = characterInfo;


		string textureFilename = dirName + mbFont.spriteName + ".png";
		Material mat = null;
		{
			Shader shader = Shader.Find("Unlit/Transparent");
			mat = new Material(shader);
			Texture tex = AssetDatabase.LoadAssetAtPath(textureFilename, typeof(Texture)) as Texture;
			mat.SetTexture("_MainTex", tex);
			AssetDatabase.CreateAsset(mat, dirName + fntname + ".mat");
			AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(mat);

        }
		CustomFont.material = mat;
     
        EditorUtility.SetDirty(CustomFont);
       
	}
}
