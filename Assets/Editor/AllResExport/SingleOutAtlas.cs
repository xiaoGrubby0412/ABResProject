
using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


public class SingleOutAtlas : EditorWindow
{
    private static string modifyImageDirectoryStr =	"Resources/GUiResource/Atlas";

    private static Texture modifyPngTexture;
    //[MenuItem("Tools/BeYond/单独修改图片格式为ETC(android)_PVR(ios)")]
    public static void ModifyImageToETC()
    {
        string fullPath = Application.dataPath + "/" + modifyImageDirectoryStr;
        if (!Directory.Exists(fullPath)) return;
        string aimPath = Selection.activeObject.name;
        if (string.IsNullOrEmpty(aimPath)) return;
        
        string assetsPath = "Assets/" + GetRelativeName(fullPath + "/" + aimPath + ".png", Application.dataPath + "/");
        //获取原始尺寸
        int atlasPngWidth = 0;
        int atlasPngHeight = 0;

        modifyPngTexture = (Texture)AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Texture));

        atlasPngWidth = modifyPngTexture.width;
        atlasPngHeight = modifyPngTexture.height;

        int atlasPngSize = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(Mathf.Max(atlasPngWidth, atlasPngHeight), 2)));

        //CreateMaterial(assetsPath, atlasPngSize, null);

        TextureImporter textureImporter = AssetImporter.GetAtPath(assetsPath) as TextureImporter;

        //TextureImporterSettings tis = new TextureImporterSettings();
        //tis.readable = true;
        //tis.mipmapEnabled = false;
        //tis.spritePixelsPerUnit = 100;
        //tis.spriteMeshType = SpriteMeshType.FullRect;
        //tis.wrapMode = TextureWrapMode.Clamp;
        //tis.filterMode = UnityEngine.FilterMode.Bilinear;
        //tis.alphaIsTransparency = false;
        //tis.spriteMode = 2;
        //tis.readable = false;
        //textureImporter.SetTextureSettings(tis);

        //textureImporter.textureType = TextureImporterType.Advanced;
        //textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        //textureImporter.maxTextureSize = atlasPngSize;

        textureImporter.compressionQuality = 100;
        //textureImporter.SetPlatformTextureSettings("Android", atlasPngSize, TextureImporterFormat.ETC_RGB4);
        //textureImporter.SetPlatformTextureSettings("iPhone", atlasPngSize, TextureImporterFormat.PVRTC_RGB2);
        textureImporter.SetPlatformTextureSettings("Standalone", atlasPngSize, TextureImporterFormat.DXT5);
        //AssetDatabase.ImportAsset(assetsPath);
        //textureImporter.textureType = TextureImporterType.Sprite;
        AssetDatabase.ImportAsset(assetsPath);
        AssetDatabase.Refresh();
        Debug.Log("修改图片格式为 Over");
    }

    private static List<DirectoryInfo> outList = new List<DirectoryInfo>();

    Dictionary<string, List<UnityEngine.Object>> _dictSheet = new Dictionary<string, List<UnityEngine.Object>>();
    private static Texture atlasPngTexture;

    [MenuItem("Tools/BeYond/SingleExportAtlas")]
    public static void ExportAtlas()
    {
        outList.Clear();
        if (!Selection.activeObject)
        {
            Debug.Log("Selection is Null");
            return;
        }
        string aimPath = Selection.activeObject.name;
        if (string.IsNullOrEmpty(aimPath)) return;

        Debug.Log("aimPath-->" + aimPath);

        string prefabPath = Application.dataPath + "/Resources/GUiResource/Atlas/" + aimPath + ".png";
        string matPath = Application.dataPath + "/Resources/GUiResource/Atlas/" + aimPath + "Material.mat";
        //string alphaPath = Application.dataPath + "/Resources/GUiResource/Atlas/" + aimPath + "_alpha.png";
        if (Directory.Exists(Application.dataPath + "/Resources/GUiResource/Atlas"))
        {
            if (File.Exists(prefabPath))
                File.Delete(prefabPath);
            //if (File.Exists(matPath))
            //    File.Delete(matPath);
            //if (File.Exists(alphaPath))
            //    File.Delete(alphaPath);
        }
        ScanDirectory(Application.dataPath + "/GUiResource/Src/" + aimPath, OutAtlasFiles);
        //AssetDatabase.Refresh();
       
        RealOutAtlasFiles();
        AssetDatabase.Refresh();
        //EditorUtility.DisplayDialog("ExportAtlas Over", "", "OK");

       
        if (Directory.Exists(Application.dataPath + "/Resources/GUiResource/Atlas/"))
        {
            string[] files = Directory.GetFiles(Application.dataPath + "/Resources/GUiResource/Atlas", "*.tpsheet", SearchOption.AllDirectories);
            for (int j = 0; j < files.Length; j++)
            {
                File.Delete(files[j]);
            }
            AssetDatabase.Refresh();
           
            string pngPath = Application.dataPath + "/Resources/GUiResource/Atlas/" + aimPath + ".png";

            string assetsPath = "Assets/" + GetRelativeName(pngPath, Application.dataPath + "/");
            //Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetsPath);
            //获取原始尺寸
            int atlasPngWidth = 1024;
            int atlasPngHeight = 1024;

            atlasPngTexture = (Texture)AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Texture));

            atlasPngWidth = atlasPngTexture.width;
            atlasPngHeight = atlasPngTexture.height;

            int atlasPngSize = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(Mathf.Max(atlasPngWidth, atlasPngHeight), 2)));

            //AssetDatabase.ImportAsset(assetsPath);
            //Debug.Log("assetsPath:" + assetsPath);
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetsPath);

            string srcAssetsDir = assetsPath.Replace("/Atlas/", "/Src/");
            srcAssetsDir = srcAssetsDir.Replace("Resources/GUiResourc", "GUiResourc");
            srcAssetsDir = srcAssetsDir.Replace(".png", "/");

            TextureImporter textureImporter = AssetImporter.GetAtPath(assetsPath) as TextureImporter;

            SpriteMetaData[] data = textureImporter.spritesheet;

            Dictionary<string, SpriteMetaData> dictMetaData = new Dictionary<string, SpriteMetaData>();

            for (int m = 0; m < data.Length; m++)
            {
                dictMetaData.Add(data[m].name, data[m]);
            }
            for (int m = 1; m < sprites.Length; m++)
            {
                Sprite tmpSprite = (Sprite)sprites[m];

                string srcSpritePath = srcAssetsDir + tmpSprite.name + ".png";

                TextureImporter textureImporterSrc = AssetImporter.GetAtPath(srcSpritePath) as TextureImporter;

                string name = Path.GetFileNameWithoutExtension(srcSpritePath);

                SpriteMetaData tmpMetaData = dictMetaData[name];
                tmpMetaData.pivot = textureImporterSrc.spritePivot;
                tmpMetaData.border = textureImporterSrc.spriteBorder;
                dictMetaData[name] = tmpMetaData;

            }

            for (int m = 0; m < data.Length; m++)
            {
                data[m] = dictMetaData[data[m].name];
            }

            textureImporter.spritesheet = data;

            TextureImporterSettings tis = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(tis);
            
            tis.readable = true;
            tis.mipmapEnabled = false;
            
            //tis.spritePixelsPerUnit = 100;
            //tis.spriteMeshType = SpriteMeshType.FullRect;
            //tis.wrapMode = TextureWrapMode.Clamp;
            //tis.filterMode = UnityEngine.FilterMode.Bilinear;
            tis.alphaIsTransparency = false;
            tis.spriteMode = 2;
            tis.readable = false;
            textureImporter.SetTextureSettings(tis);

            textureImporter.textureType = TextureImporterType.Advanced;
            textureImporter.textureFormat = TextureImporterFormat.DXT5;
            textureImporter.maxTextureSize = atlasPngSize;
            Debug.Log("textureImporter.maxTextureSize："+atlasPngSize);
            //textureImporter.compressionQuality = 100;
            //textureImporter.SetPlatformTextureSettings("Android", atlasPngSize, TextureImporterFormat.ETC_RGB4);
            textureImporter.SetPlatformTextureSettings("Standalone", atlasPngSize, TextureImporterFormat.DXT5);

            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
            AssetDatabase.ImportAsset(assetsPath, ImportAssetOptions.ForceUpdate);
        }
        AssetDatabase.Refresh();

        //EditorUtility.DisplayDialog("Export Atlas Over", "", "OK");
        Debug.Log("Export Atlas Over");
        //ModifyImageToETC();
        //if (aimPath.Contains("NewAllCommon") || aimPath.Contains("NewBattleUI"))
        //    UGUIChangeSpriteTexture.DoChangeSpriteTexture();
    }


    private static Texture alphaTexture;
    public static void CreateMaterial(string path, int maxSize, SpriteMetaData[] data)
    {
        string destPath = path.Replace(".png", "_alpha.png");
        CopyFile(path, destPath);


        TextureImporter textureImporter = AssetImporter.GetAtPath(destPath) as TextureImporter;

        if (data != null)
            textureImporter.spritesheet = data;

        TextureImporterSettings tis = new TextureImporterSettings();
        tis.readable = true;
        tis.mipmapEnabled = false;
        tis.spritePixelsPerUnit = 100;
        tis.spriteMeshType = SpriteMeshType.FullRect;
        tis.wrapMode = TextureWrapMode.Clamp;
        tis.filterMode = UnityEngine.FilterMode.Bilinear;
        tis.alphaIsTransparency = false;
        //tis.spriteMode = 2;
        tis.readable = false;
        textureImporter.SetTextureSettings(tis);




        textureImporter.textureType = TextureImporterType.Advanced;
        textureImporter.textureFormat = TextureImporterFormat.Alpha8;
        if (path.Contains("NewLobbyMainPanel") || path.Contains("NewMemberUI") || path.Contains("NewDepotUIPanel")
            || path.Contains("FriendsUI"))
            textureImporter.maxTextureSize = maxSize;
        else
            textureImporter.maxTextureSize = maxSize;
        //textureImporter.maxTextureSize = maxSize/2;

        AssetDatabase.ImportAsset(destPath);


        Material material = new Material(Shader.Find("Sprites/RGB+Alpha"));
        material.name = Path.GetFileName(path).Replace(".png", "") + "Material";
        string savePath = Path.GetDirectoryName(path) + "/" + material.name + ".mat";

        string alphaTexPropertyName = "_AlphaTex";
        if (material.HasProperty(alphaTexPropertyName))
        {
            Texture tex = material.GetTexture(alphaTexPropertyName);

            alphaTexture = (Texture)AssetDatabase.LoadAssetAtPath(destPath, typeof(Texture));
            material.SetTexture(alphaTexPropertyName, alphaTexture);

        }

        AssetDatabase.CreateAsset(material, savePath);
    }

    public static void setTextureSetting(string path, string srcPath)
    {

        TextureImporter textureImporterSrc = AssetImporter.GetAtPath(srcPath) as TextureImporter;

        string name = Path.GetFileNameWithoutExtension(srcPath);


        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        SpriteMetaData[] data = textureImporter.spritesheet;


        for (int i = 0; i < data.Length; i++)
        {
            if (data[i].name == name)
            {
                data[i].pivot = textureImporterSrc.spritePivot;
                data[i].border = textureImporterSrc.spriteBorder;
            }

        }

        textureImporter.spritesheet = data;
        //		TextureImporterSettings tis = new TextureImporterSettings();
        //		tis.readable = true;
        //		tis.mipmapEnabled = false;
        //		tis.spritePixelsPerUnit = 100;
        //		tis.spriteMeshType = SpriteMeshType.FullRect;
        //		tis.wrapMode = TextureWrapMode.Clamp;
        //		tis.filterMode = UnityEngine.FilterMode.Bilinear;
        //
        //		//textureImporter.ReadTextureSettings(tis);
        //		textureImporter.SetTextureSettings(tis);
        //
        //					
        //
        //
        //		textureImporter.textureType = TextureImporterType.Advanced;
        //		textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        //		textureImporter.maxTextureSize = 2048;




        AssetDatabase.ImportAsset(path);
    }


    public delegate void ScanFileAction(DirectoryInfo file);
    public static void ScanDirectory(string path, ScanFileAction fileAction)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists && fileAction != null)
        {
            ScanDirectory(dirInfo, fileAction);
        }
    }

    public static void ScanDirectory(DirectoryInfo dirInfo, ScanFileAction fileAction)
    {
        if (dirInfo == null || !dirInfo.Exists || fileAction == null)
        {
            return;
        }

        int i;
        FileInfo[] files = dirInfo.GetFiles("*.png");
        if (files.Length > 0)
        {
            fileAction(dirInfo);
        }

        DirectoryInfo[] diA = dirInfo.GetDirectories();
        for (i = 0; i < diA.Length; i++)
        {
            ScanDirectory(diA[i], fileAction);
        }

    }


    public delegate void ScanTankTexFileAction(FileInfo file);
    public static void ScanTankDirectory(string path, ScanTankTexFileAction fileAction)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists && fileAction != null)
        {
            ScanTankDirectory(dirInfo, fileAction);
        }
    }

    public static void ScanTankDirectory(DirectoryInfo dirInfo, ScanTankTexFileAction fileAction)
    {
        if (dirInfo == null || !dirInfo.Exists || fileAction == null)
        {
            return;
        }

        int i;
        FileInfo[] files = dirInfo.GetFiles("*.tga");
        for (i = 0; i < files.Length; i++)
        {
            fileAction(files[i]);
        }

        //FileInfo[] files2 = dirInfo.GetFiles(".png");
        //for (i = 0; i < files2.Length; i++)
        //{
        //	fileAction(files2[i]);
        //}

        DirectoryInfo[] diA = dirInfo.GetDirectories();
        for (i = 0; i < diA.Length; i++)
        {
            ScanTankDirectory(diA[i], fileAction);
        }

    }

    public static void outTankAlphaTex(FileInfo file)
    {

        string fullPath = file.FullName;

        if (fullPath.EndsWith("_alpha.tga")) return;
        if (fullPath.EndsWith("_NM.tga")) return;

        string assetsPath = "Assets/" + GetRelativeName(fullPath, Application.dataPath + "/");


        string destPath = assetsPath.Replace(".tga", "_alpha.tga");

        //if (File.Exists(GetFullPath(destPath)))
        //{
        //	File.Delete(GetFullPath(destPath));
        //}

        //return;//

        //CopyFile(assetsPath, destPath);


        reImportAsset(assetsPath, TextureImporterFormat.ETC_RGB4);

        if (File.Exists(GetFullPath(destPath)))
        {
            reImportAsset(destPath, TextureImporterFormat.Alpha8);
        }



    }


    public static void reImportAsset(string assetsPath, TextureImporterFormat textureFormat)
    {
        //获取原始尺寸
        int atlasPngWidth = 0;
        int atlasPngHeight = 0;

        Texture texture = (Texture)AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Texture));

        atlasPngWidth = texture.width;
        atlasPngHeight = texture.height;

        int atlasPngSize = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(Mathf.Max(atlasPngWidth, atlasPngHeight), 2)));

        TextureImporter textureImporter = AssetImporter.GetAtPath(assetsPath) as TextureImporter;

        TextureImporterSettings tis = new TextureImporterSettings();
        tis.readable = true;
        tis.mipmapEnabled = false;
        tis.spritePixelsPerUnit = 100;
        tis.spriteMeshType = SpriteMeshType.FullRect;
        tis.wrapMode = TextureWrapMode.Repeat;
        tis.filterMode = UnityEngine.FilterMode.Bilinear;
        tis.alphaIsTransparency = false;
        tis.spriteMode = 0;
        tis.mipmapEnabled = true;
        tis.readable = false;
        textureImporter.SetTextureSettings(tis);

        textureImporter.textureType = TextureImporterType.Advanced;
        textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        textureImporter.maxTextureSize = atlasPngSize;

        textureImporter.compressionQuality = 100;
        if (textureFormat == TextureImporterFormat.Alpha8)
        {
            textureImporter.SetPlatformTextureSettings("Android", atlasPngSize, textureFormat);
            textureImporter.SetPlatformTextureSettings("iPhone", atlasPngSize, textureFormat);
        }
        else
        {
            textureImporter.SetPlatformTextureSettings("Android", atlasPngSize, textureFormat);
            textureImporter.SetPlatformTextureSettings("iPhone", atlasPngSize, TextureImporterFormat.PVRTC_RGB2);
        }


        AssetDatabase.ImportAsset(assetsPath);
    }

    private static void OutAtlasFiles(DirectoryInfo dirInfo)
    {
        outList.Add(dirInfo);
    }


    private static void RealOutAtlasFiles()
    {
        for (int i = 0; i < outList.Count; )
        {
            if (RunCmd(outList[i]))
            {
                i++;
            }

        }

    }



    private static string GetRelativeName(string assetPath, string sourceRootPath)
    {
        assetPath = assetPath.Replace("\\", "/");
        return assetPath.Substring(sourceRootPath.Length);
    }


    private static bool RunCmd(DirectoryInfo dirInfo)
    {
        string dirName = dirInfo.Name;
        string dirPath = dirInfo.FullName;

        string outDirPath = dirPath.Replace("\\Src\\", "\\Atlas\\");
        outDirPath = outDirPath.Replace("GUiResourc", "Resources\\GUiResourc");
        string outDataPath = outDirPath + ".tpsheet";
        string sheetPath = outDirPath + ".png";



        string cmdExe = "TexturePacker";
        string cmdStr = "--algorithm Basic " +
                        "--texture-format png " +
                        "--basic-sort-by Best " +
                        "--basic-order Ascending " +
                        "--multipack " +
                        "--disable-rotation " +
						"--force-squared " +
                        "--scale 1 " +
                        "--opt RGBA8888 " +
                        "--max-height 2048 " +
                        "--max-width 2048 " +
                        "--border-padding 1 " +
                        "--shape-padding 1 " +
                        "--padding 0 " +
                        "--trim-mode None " +
                        "--size-constraints POT " +
                        "--data " + outDataPath + " " +
                        "--format unity-texture2d " +
                        "--sheet " + sheetPath + " " +
                        dirPath;

        cmdStr = cmdStr.Replace("\\", "/");

        Process myPro = new Process();
        string str = string.Format("/C {0} {1}", cmdExe, cmdStr);
        str = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(str));
        //string str = string.Format(@"""/C {0}"" {1}", cmdExe, cmdStr);
        //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
        //string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");

        myPro.StartInfo.FileName = "cmd.exe";
        myPro.StartInfo.UseShellExecute = false;
        myPro.StartInfo.Arguments = str;
        myPro.StartInfo.RedirectStandardInput = false;
        myPro.StartInfo.RedirectStandardOutput = true;
        myPro.StartInfo.RedirectStandardError = true;
        myPro.StartInfo.CreateNoWindow = true;

        myPro.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);


        try
        {
            if (myPro.Start())
            {
                if (0 == 0)
                {
                    myPro.WaitForExit();//这里无限等待进程结束  
                }
                else
                {
                    myPro.WaitForExit(1000); //等待进程结束，等待时间为指定的毫秒  
                }
                string output = myPro.StandardOutput.ReadToEnd();//读取进程的输出  
                Debug.Log("OUT PUT:" + output);
                string errorStr = myPro.StandardError.ReadToEnd();//读取进程的输出  
                if (errorStr.Length > 0)
                    Debug.LogError("dir:" + dirPath + ">>>>>>      \r\n" + errorStr);
            }

        }
        catch (Exception e)
        {
            Debug.Log("catch:" + e.ToString());
        }
        finally
        {
            if (myPro != null)
                myPro.Close();
        }


        return true;
    }

    private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {

            Debug.Log("OUT PUT:" + outLine.Data.ToString());
            //			StringBuilder sb = new StringBuilder(this.textBox1.Text);
            //			this.textBox1.Text = sb.AppendLine(outLine.Data).ToString();
            //			this.textBox1.SelectionStart = this.textBox1.Text.Length;
            //			this.textBox1.ScrollToCaret();
        }
    }


    public static void CopyFile(string srcFileName, string destFileName)
    {
        if (IsFileExists(srcFileName) && !srcFileName.Equals(destFileName))
        {
            int index = destFileName.LastIndexOf("/");
            string filePath = string.Empty;

            if (index != -1)
            {
                filePath = destFileName.Substring(0, index);
            }

            if (!Directory.Exists(GetFullPath(filePath)))
            {
                Directory.CreateDirectory(GetFullPath(filePath));
            }

            File.Copy(GetFullPath(srcFileName), GetFullPath(destFileName), true);

            AssetDatabase.Refresh();
        }
    }

    public static bool IsFileExists(string fileName)
    {
        if (fileName.Equals(string.Empty))
        {
            return false;
        }

        return File.Exists(GetFullPath(fileName));
    }

    private static string GetFullPath(string srcName)
    {
        if (srcName.Equals(string.Empty))
        {
            return Application.dataPath;
        }

        if (srcName.StartsWith("Assets/"))
        {
            srcName = srcName.Replace("Assets/", "");
        }

        return Application.dataPath + "/" + srcName;
    }



}

