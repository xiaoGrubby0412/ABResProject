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

public class OutAtlas : AssetPostprocessor
{
    private static Dictionary<string, UnityEngine.Object> dic;

    private static List<DirectoryInfo> outList = new List<DirectoryInfo>();

    [MenuItem("Assets/打包Atlas")]
    public static void BuildAtlasByDic()
    {
        outList.Clear();
        dic = null;
        dic = new Dictionary<string, Object>();

        List<UnityEngine.Object> selectedDics = new List<UnityEngine.Object>(Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets));
        selectedDics.RemoveAll(item =>
        {
            bool ifFloder = !Directory.Exists(AssetDatabase.GetAssetPath(item.GetInstanceID()));
            return ifFloder;
        });

        string FilePathPrefix = "Assets/GUiResource/Src/";
        for (int i = 0; i < selectedDics.Count; i++)
        {
            UnityEngine.Object item = selectedDics[i];
            string assetPath = AssetDatabase.GetAssetPath(item.GetInstanceID());
            if (!assetPath.StartsWith(FilePathPrefix))
            {
                EditorUtility.DisplayDialog("错误", "请选择 " + FilePathPrefix + " 文件夹下面的文件夹", "确定");
                return;
            }
            dic.Add(assetPath, item);

        }
        if (dic.Count <= 0)
        {
            EditorUtility.DisplayDialog("错误", "选择的文件夹数量为零", "确定");
            return;
        }

        foreach (KeyValuePair<string, Object> pair in dic)
        {
            DirectoryInfo dicInfo = new DirectoryInfo(pair.Key);
            FileInfo[] files = dicInfo.GetFiles("*.png");
            if (files == null || files.Length <= 0) continue;
            if (files.Length > 0)
            {
                outList.Add(dicInfo);
            }
        }

        StartExportAtlas();
    }

    [MenuItem("AllResExport/ExportAtlas")]
    public static void ExportAtlas()
    {
        outList.Clear();
        DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/GUiResource/Src");
        if (dirInfo == null || !dirInfo.Exists)
        {
            return;
        }
        DirectoryInfo[] diA = dirInfo.GetDirectories();
        if (diA == null || diA.Length <= 0) return;
        for (int j = 0; j < diA.Length; j++)
        {
            FileInfo[] files = diA[j].GetFiles("*.png");
            if (files == null || files.Length <= 0) continue;
            if (files.Length > 0)
            {
                outList.Add(diA[j]);
            }
        }
        StartExportAtlas();
    }

    /// <summary>
    /// outList加入完dicInfo之后 开始打包Atlas
    /// </summary>
    public static void StartExportAtlas()
    {
        if (outList.Count <= 0) return;
        for (int l = 0; l < outList.Count; l++)
        {
            string delAtlasPath1 = Application.dataPath + "/UI/GUiResource/Atlas/" + outList[l].Name + ".png";
            string delAtlasPath2 = Application.dataPath + "/UI/GUiResource/Atlas/" + outList[l].Name + ".png.meta";
            if (File.Exists(delAtlasPath1))
            {
                File.Delete(delAtlasPath1);
            }
            if (File.Exists(delAtlasPath2))
            {
                File.Delete(delAtlasPath2);
            }
        }

        AssetDatabase.Refresh();
        RealOutAtlasFiles();
        AssetDatabase.Refresh();

        if (Directory.Exists(Application.dataPath + "/UI/GUiResource/Atlas"))
        {
            for (int k = 0; k < outList.Count; k++)
            {
                string file = Application.dataPath + "/UI/GUiResource/Atlas/" + outList[k].Name + ".tpsheet";
                if (File.Exists(file)) File.Delete(file);

                AssetDatabase.Refresh();

                string pngFile = Application.dataPath + "/UI/GUiResource/Atlas/" + outList[k].Name + ".png";
                string assetsPath = "Assets/" + GetRelativeName(pngFile, Application.dataPath + "/");
                //Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetsPath);
                //获取原始尺寸
                int atlasPngWidth = 1024;
                int atlasPngHeight = 1024;

                Debug.Log("assetsPath111111111:" + assetsPath);
                Texture atlasPngTexture = (Texture)AssetDatabase.LoadAssetAtPath(assetsPath, typeof(Texture));
                atlasPngWidth = atlasPngTexture.width;
                atlasPngHeight = atlasPngTexture.height;

                int atlasPngSize = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(Mathf.Max(atlasPngWidth, atlasPngHeight), 2)));

                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetsPath);
                if (!(sprites[0] is UnityEngine.Texture2D))
                {
                    for (int p = 1; p < sprites.Length; p++)
                    {
                        if (sprites[p] is UnityEngine.Texture2D)
                        {
                            Object temp = sprites[p];
                            sprites[p] = sprites[0];
                            sprites[0] = temp;
                        }
                    }
                }
                string srcAssetsDir = assetsPath.Replace("/Atlas/", "/Src/");
                srcAssetsDir = srcAssetsDir.Replace("UI/GUiResource", "GUiResource");
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
                    Debug.Log("srcSpritePath:" + srcSpritePath);
                    //TextureImporter textureImporterSrc = AssetImporter.GetAtPath(srcSpritePath) as TextureImporter;
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
                textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                textureImporter.maxTextureSize = atlasPngSize;

                //textureImporter.compressionQuality = 100;
                textureImporter.SetPlatformTextureSettings("Standalone", atlasPngSize, TextureImporterFormat.DXT5Crunched, 50, true);
                EditorUtility.SetDirty(textureImporter);
                textureImporter.SaveAndReimport();
                AssetDatabase.ImportAsset(assetsPath);
                Debug.Log("assetPath:" + assetsPath);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Export Atlas Over");
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
        outDirPath = outDirPath.Replace("GUiResourc", "UI\\GUiResourc");
        string outDataPath = outDirPath + ".tpsheet";
        string sheetPath = outDirPath + ".png";



        string cmdExe = "TexturePacker";
        string cmdStr = "--algorithm Basic " +
                        "--texture-format png " +
                        "--basic-sort-by Best " +
                        "--basic-order Ascending " +
                        "--multipack " +
                        "--disable-rotation " +
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
                //string dir = sheetPath.Substring("I:\\tankweb\\Tank_Client\\5xProject\\".Length);
                //Debug.Log("dir:"+dir);
                //AssetDatabase.ImportAsset(dir);

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

}

