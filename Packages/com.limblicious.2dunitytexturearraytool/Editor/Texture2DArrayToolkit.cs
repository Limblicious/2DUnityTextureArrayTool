// Assets/Editor/Texture2DArrayToolkit.cs
// Unity 2019.4+ (URP/HDRP/BRP). MIT.
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Texture2DArrayToolkit : EditorWindow
{
    [Header("Export")]
    public Texture2DArray sourceArray;
    public string exportFolder = "Assets/Exported_T2DArray";

    [Header("Rebuild")]
    public string importFolder = "Assets/Edited_T2DArray";
    public int width = 256;
    public int height = 256;
    public bool generateMipmaps = true;
    public TextureFormat format = TextureFormat.RGBA32;
    public bool linearColorSpace = false; // false = sRGB
    public FilterMode filterMode = FilterMode.Bilinear;
    public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
    public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Disable;
    public int anisoLevel = 1;
    public string savePath = "Assets/MyNewTexture2DArray.asset";

    [MenuItem("Tools/Texture2DArray Toolkit")]
    public static void ShowWindow()
    {
        GetWindow<Texture2DArrayToolkit>("Texture2DArray Toolkit");
    }

    void OnGUI()
    {
        GUILayout.Label("Export slices from Texture2DArray", EditorStyles.boldLabel);
        sourceArray = (Texture2DArray)EditorGUILayout.ObjectField("Source Array", sourceArray, typeof(Texture2DArray), false);
        exportFolder = EditorGUILayout.TextField("Export To Folder", exportFolder);

        using (new EditorGUI.DisabledScope(sourceArray == null))
        {
            if (GUILayout.Button("Export Slices → PNG"))
                ExportSlices();
        }

        EditorGUILayout.Space(12);
        GUILayout.Label("Rebuild Texture2DArray from PNGs", EditorStyles.boldLabel);
        importFolder = EditorGUILayout.TextField("Import From Folder", importFolder);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        generateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", generateMipmaps);
        format = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", format);
        linearColorSpace = EditorGUILayout.Toggle(new GUIContent("Linear (unchecked = sRGB)"), linearColorSpace);
        filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", filterMode);
        wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", wrapMode);
        anisotropicFiltering = (AnisotropicFiltering)EditorGUILayout.EnumPopup("Aniso Filtering", anisotropicFiltering);
        anisoLevel = EditorGUILayout.IntSlider("Aniso Level", anisoLevel, 1, 16);
        savePath = EditorGUILayout.TextField("Save As", savePath);

        if (GUILayout.Button("Build Texture2DArray from PNG folder"))
            BuildArrayFromPNGs();
    }

    void ExportSlices()
    {
        if (sourceArray == null) return;
        if (!AssetDatabase.IsValidFolder(exportFolder))
        {
            Directory.CreateDirectory(exportFolder.Replace("Assets/", Application.dataPath + "/"));
            AssetDatabase.Refresh();
        }

        int w = sourceArray.width;
        int h = sourceArray.height;
        int slices = sourceArray.depth;
        int mips = sourceArray.mipmapCount;

        // Create a temp Texture2D we can read/encode
        var temp = new Texture2D(w, h, TextureFormat.RGBA32, false, QualitySettings.activeColorSpace == ColorSpace.Linear);
        temp.filterMode = sourceArray.filterMode;
        temp.wrapMode = sourceArray.wrapMode;

        for (int i = 0; i < slices; i++)
        {
            // Copy GPU → GPU, then CPU-readable via Texture2D.EncodeToPNG
            Graphics.CopyTexture(sourceArray, i, 0, temp, 0, 0);
            temp.Apply(false, false);
            var png = temp.EncodeToPNG();
            var path = Path.Combine(exportFolder, $"slice_{i:D3}.png").Replace("\\", "/");
            File.WriteAllBytes(path, png);
        }

        DestroyImmediate(temp);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Export complete", $"Wrote {slices} PNGs to:\n{exportFolder}", "OK");
    }

    void BuildArrayFromPNGs()
    {
        if (!AssetDatabase.IsValidFolder(importFolder))
        {
            EditorUtility.DisplayDialog("Folder not found", importFolder, "OK");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { importFolder });
        var texPaths = guids.Select(AssetDatabase.GUIDToAssetPath)
                            .Where(p => p.EndsWith(".png") || p.EndsWith(".jpg") || p.EndsWith(".tga"))
                            .OrderBy(p => p)
                            .ToList();

        if (texPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("No images", "Place PNG/JPG/TGA frames in the import folder.", "OK");
            return;
        }

        // Load and enforce import settings (readable, no mipmaps)
        var textures = texPaths.Select(p =>
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(p);
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Default) importer.textureType = TextureImporterType.Default;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = !linearColorSpace;
                importer.filterMode = filterMode;
                importer.wrapMode = wrapMode;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
        }).ToList();

        // Optionally resize each to match (simple GPU path via Blit)
        var resized = textures.Select(t => EnsureSize(t, width, height, linearColorSpace)).ToList();

        // Create array
        var array = new Texture2DArray(width, height, resized.Count, format, generateMipmaps, linearColorSpace);
        array.filterMode = filterMode;
        array.wrapMode = wrapMode;
        array.anisoLevel = anisoLevel;

        for (int i = 0; i < resized.Count; i++)
        {
            Graphics.CopyTexture(resized[i], 0, 0, array, i, 0);
        }

        array.Apply(updateMipmaps: generateMipmaps, makeNoLongerReadable: false);

        // Save asset
        var dir = Path.GetDirectoryName(savePath)?.Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(dir))
        {
            Directory.CreateDirectory(dir.Replace("Assets/", Application.dataPath + "/"));
            AssetDatabase.Refresh();
        }
        AssetDatabase.CreateAsset(array, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(array);
        EditorUtility.DisplayDialog("Build complete", $"Created array with {resized.Count} slices:\n{savePath}", "OK");
    }

    // Ensures a Texture2D is exactly width×height (GPU-resample if needed)
    static Texture2D EnsureSize(Texture2D src, int w, int h, bool linear)
    {
        if (src.width == w && src.height == h) return src;

        var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
        var prev = RenderTexture.active;
        Graphics.Blit(src, rt);
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, linear);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
        tex.Apply(false, false);
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return tex;
    }
}