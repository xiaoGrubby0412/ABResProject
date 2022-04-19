
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;




public class CheckSameFiles : EditorWindow
{

	private static string[] resDirectoryArray = new string[]
	{
		"Atlas",
		"Medias",
		"TankTrack",
		"Resources"
	};

	//for test same name
	private static Dictionary<string, string> allResKeyExt = new Dictionary<string, string>();
	private static List<string> sameNameRes = new List<string>();

	//end
	private static string checkFolderPath = "";
	[MenuItem("CheckSameNameFiles/Check")]
	public static void check()
	{


		checkFolderPath = EditorUtility.OpenFolderPanel("Check Folder", "Check Folder Select", "");

		//Debug.Log("Check Folder=" + checkFolderPath);

		if (!checkFolderPath.StartsWith( Application.dataPath))
			return;

		DirectoryInfo dirInfo = new DirectoryInfo(checkFolderPath);
		if (!dirInfo.Exists)
			return;
		
		//检查在同一目录下是否有相同名字，不同类型的文件

		ScanDirectory(checkFolderPath, checkHaveSameNameFile);
		
//		for (int i = 0; i < resDirectoryArray.Length; i++)
//		{
//		checkFolderPath += resDirectoryArray[i] + "\r\n";
//			ScanDirectory(Application.dataPath + "/" + resDirectoryArray[i], checkHaveSameNameFile);
//		}

		int fileNum = sameNameRes.Count;
		outSameFileInfo();
		AssetDatabase.Refresh();

		EditorUtility.DisplayDialog("Check Over", ""+fileNum+" files have same name!", "OK");
		
	}


	public delegate void ScanFileAction(FileInfo file);
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
		FileInfo[] files = dirInfo.GetFiles();
		for (i = 0; i < files.Length; i++)
		{
			fileAction(files[i]);
		}
		DirectoryInfo[] diA = dirInfo.GetDirectories();
		for (i = 0; i < diA.Length; i++)
		{
			ScanDirectory(diA[i], fileAction);
		}
	}

	private static void checkHaveSameNameFile(FileInfo fileInfo)
	{
		string fileName = fileInfo.FullName;

		if (fileName.EndsWith(".meta")) return;
		if (fileName.EndsWith(".cs")) return;
		if (fileName.EndsWith(".js")) return;

		string assetRelativeName = GetRelativeName(fileName, Application.dataPath + "/");

		string tmpKey = string.Format("{0}/{1}", Path.GetDirectoryName(assetRelativeName), Path.GetFileNameWithoutExtension(assetRelativeName));
		string ext = Path.GetExtension(fileName);

		if (allResKeyExt.ContainsKey(tmpKey))
		{
			string newPath = tmpKey + ext;

			string oldExt = allResKeyExt[tmpKey];
			string oldPath = tmpKey + "" + oldExt;

			if (!sameNameRes.Contains(oldPath))
			{
				sameNameRes.Add(oldPath);
			}

			sameNameRes.Add(newPath);
		}
		else
		{
			allResKeyExt.Add(tmpKey, ext);
		}

	}

	private static void outSameFileInfo()
	{
		StringBuilder txt = new StringBuilder();
		StringWriter writer = new StringWriter(txt);

		sameNameRes.Sort();

		System.DateTime now = System.DateTime.Now;
		string checkTime="CHECK TIME:" + now.Year + "/" + now.Month + "/" + now.Day + " " + now.Hour + ":" + now.Minute;
		writer.Write(checkTime);
		writer.Write("\r\n");

		writer.Write("CHECK FOLDER:" + checkFolderPath);
		writer.Write("\r\n");

		for (int i = 0; i < sameNameRes.Count; i++)
		{
			string fileName = sameNameRes[i];

			
			writer.Write("\r\n");
			
			writer.Write(fileName);
		}

		SaveStringToFile(Application.dataPath + "/SameNameFiles.txt", txt.ToString());

		sameNameRes.Clear();
		allResKeyExt.Clear();

	}

	public static void SaveStringToFile(string FilePath, string Content)
	{
		System.IO.StreamWriter Sw = new System.IO.StreamWriter(FilePath);
		{
			Sw.Write(Content);
			Sw.Close();
		}
	}

	private static string GetRelativeName(string assetPath, string sourceRootPath)
	{
		assetPath = assetPath.Replace("\\", "/");
		return assetPath.Substring(sourceRootPath.Length);
	}


}
