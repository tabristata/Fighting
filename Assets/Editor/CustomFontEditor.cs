using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

public class CustomFontEditor : EditorWindow {

	// 输出路径
	private string m_exportPath = "Assets/Resources/Fonts/";
	// 文字内容
	private string m_fontText = "0123456789";
    // 字体名字
	private string m_fontName = "NewFont";
	// 字体贴图
	[SerializeField]
	private Texture2D m_fontTexture;

	private CharacterInfo[] m_charInfos;

	private Rect[] m_charRects;

	private Vector2 scrollPos;

	[MenuItem("Window/" + "自定义字体")]
	static void Init() {
		EditorWindow window = GetWindow(typeof(CustomFontEditor));
		window.Show();
	}

	public void Awake () 
	{

	}

	void OnGUI() {

		m_exportPath = EditorGUILayout.TextField("Export Path:",m_exportPath);

		m_fontText = EditorGUILayout.TextField("Font Text:",m_fontText);

		m_fontName = EditorGUILayout.TextField("Font Name:",m_fontName);

		m_fontTexture =  EditorGUILayout.ObjectField("Font Sprite",m_fontTexture,typeof(Texture2D),true) as Texture2D;


		if (m_fontTexture != null &&  m_fontText.Length > 0) {

			if (m_charInfos == null || m_charInfos.Length < 1) {
				
				if (GUILayout.Button ("CreateFont", GUILayout.Width (200))) {
					CreateFont ();
				}
					
			} else {

				scrollPos = EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Height (300));

				char[] chars = m_fontText.ToCharArray ();

				float texW = m_fontTexture.width;

				float texH = m_fontTexture.height;

				int tileW  = (int) texW / chars.Length;

				Rect r;

				for (int i = 0; i < m_charInfos.Length; i++) {
					
					Rect uv = m_charInfos [i].uv;

					Rect vert = m_charInfos [i].vert;

					EditorGUILayout.BeginHorizontal ();

                    float rectHeight = 100.0f;

                    float rectWidth = Mathf.Abs(rectHeight * m_charRects[i].width / m_charRects[i].height);

					m_charRects [i] = EditorGUILayout.RectField (chars[i] + "_Rect" , m_charRects [i], GUILayout.Width (400), GUILayout.Height (100));

                    GUI.DrawTextureWithTexCoords (new Rect(500, i * rectHeight, rectWidth, rectHeight), m_fontTexture, m_charInfos [i].uv);

					EditorGUILayout.EndHorizontal();

					r = new Rect ();
					r.x = m_charRects [i].x / texW;
					r.y = m_charRects [i].y / texH;
					r.width = m_charRects [i].width / texW;
					r.height = m_charRects [i].height / texH;
					r.y = 1f - r.y - r.height;
					m_charInfos [i].uvBottomLeft = new Vector2(r.xMin, r.yMin);
					m_charInfos [i].uvBottomRight = new Vector2(r.xMax, r.yMin);
					m_charInfos [i].uvTopLeft = new Vector2(r.xMin, r.yMax);
					m_charInfos [i].uvTopRight = new Vector2(r.xMax, r.yMax);

					r = new Rect ();
					r.x = 0f;
					r.y = 0f;
                    r.width = m_charRects[i].width;
					r.height = m_charRects [i].height;
					r.y = -r.y;
					r.height = -r.height;
					m_charInfos [i].minX = (int)r.xMin;
					m_charInfos [i].maxX = (int)r.xMax;
					m_charInfos [i].minY = (int)r.yMax;
					m_charInfos [i].maxY = (int)r.yMin;

                    m_charInfos[i].advance = (int)m_charRects[i].width;
				}

				EditorGUILayout.EndScrollView();

				if (GUILayout.Button ("ExportFont", GUILayout.Width (200))) {
					ExportFont ();
				}
		   }
		}
	}

	void CreateFont() {

		char[] chars = m_fontText.ToCharArray ();

		float texW = m_fontTexture.width;

		float texH = m_fontTexture.height;

		m_charInfos = new CharacterInfo[chars.Length];

		m_charRects = new Rect[chars.Length];
	
		int tileW  = (int) texW / chars.Length;

		Rect r;

		for (int i = 0; i < chars.Length; i++) {

			CharacterInfo charInfo = new CharacterInfo ();

			charInfo.index = (int)chars [i];

			charInfo.advance = tileW;

			Rect charRect = new Rect ();
			charRect.x = tileW * i;
			charRect.y = 0f;
			charRect.width = tileW;
			charRect.height = texH;

			r = new Rect ();
			r.x = charRect.x / texW;
			r.y = charRect.y / texH;
			r.width = charRect.width / texW;
			r.height = charRect.height / texH;
			r.y = 1f - r.y - r.height;
			charInfo.uvBottomLeft = new Vector2(r.xMin, r.yMin);
			charInfo.uvBottomRight = new Vector2(r.xMax, r.yMin);
			charInfo.uvTopLeft = new Vector2(r.xMin, r.yMax);
			charInfo.uvTopRight = new Vector2(r.xMax, r.yMax);

			r = new Rect ();
			r.x = 0f;
			r.y = 0f;
			r.width = charRect.width;
			r.height = charRect.height;
			r.y = -r.y;
			r.height = -r.height;
			charInfo.minX = (int)r.xMin;
			charInfo.maxX = (int)r.xMax;
			charInfo.minY = (int)r.yMax;
			charInfo.maxY = (int)r.yMin;

			m_charInfos [i] = charInfo;
			m_charRects [i] = charRect;
		}

		m_fontName = m_fontTexture.name;
	}

	void ExportFont() {

		Shader shader = Shader.Find ("UI/Default");
		Material material = new Material (shader);
		material.mainTexture = m_fontTexture;
		AssetDatabase.CreateAsset (material, m_exportPath + m_fontName + ".mat");

		Font font = new Font ();
		font.material = material;
		font.name = m_fontName;
		font.characterInfo = m_charInfos;

		SerializedObject mFont = new SerializedObject(font);
		mFont.FindProperty("m_LineSpacing").floatValue = m_fontTexture.height;
		mFont.ApplyModifiedProperties();

		AssetDatabase.CreateAsset (font, m_exportPath + m_fontName + ".fontsettings");

		AssetDatabase.Refresh ();
	}
		
	void Update()
	{

	}

	void OnFocus()
	{
		//Debug.Log("窗口获得焦点时调用一次");
	}

	void OnLostFocus()
	{
		//Debug.Log("当窗口丢失焦点时调用一次");
	}

	void OnHierarchyChange()
	{
		//Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
	}

	void OnProjectChange()
	{
		//Debug.Log("当Project视图中的资源发生改变时调用一次");
	}

	void OnInspectorUpdate()
	{
		//Debug.Log("窗口面板的更新");
		//这里开启窗口的重绘，不然窗口信息不会刷新
		this.Repaint();
	}

	void OnSelectionChange()
	{
		//当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
		foreach(Transform t in Selection.transforms)
		{
			//有可能是多选，这里开启一个循环打印选中游戏对象的名称
			Debug.Log("OnSelectionChange" + t.name);
		}
	}

	void OnDestroy()
	{
		//Debug.Log("当窗口关闭时调用");
	}
}