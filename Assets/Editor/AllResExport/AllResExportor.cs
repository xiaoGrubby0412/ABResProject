
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Object = UnityEngine.Object;

namespace Asgard
{
	public class AllResExportor : EditorWindow
	{
		[MenuItem("AllResExport/Export")]
		public static int Init()
		{
			EditorWindow.GetWindow<AllResExportor>().Show();
			return 0;
		}

		private static string _exportStatus = "Prepare exporting all resource\r\n";
		private BuildTarget _uiBuildTarget = BuildTarget.Android;
		private static int _curVersion=1;
        public static int CurVersion
        {
            set { _curVersion = value; }
        }

		public void OnGUI()
		{
			EditorGUILayout.TextArea(_exportStatus);
			_uiBuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Target platform", _uiBuildTarget);
			if (GUILayout.Button("Export Patch"))
			{
				_exportStatus = "Exporting,please wait a moment\r\n";
				DoExport(_uiBuildTarget);
				_exportStatus = "patch exported successfully\r\n";
			}

			_curVersion = EditorGUILayout.IntField("Version",_curVersion);
		}

		//**********************************************************




		private static string[] _resDirectoryArray = new string[]
	    {
			//"ParticlesResource",
		    "Medias/Audios",
			"Medias/Fonts",
			"Medias/Scenes/tongyong/Prefabs/Modles/BattleBuilding",
		    "TankTrack/prefab/zhongxi",
			//"GUiResource/UIPanelPrefabs",
            "GUiResource/Sprites",
			//"GUiResource/Atlas",
            "GUiResource/UIMaterial",
			"ConfigDatas",
	    };


		private static BuildTarget _buildTarget = BuildTarget.Android;
		private static BundlesMap _bundlesMap = new BundlesMap();

		private static List<string> _allResource = new List<string>();
		private static List<string> _allIgnoreResource = new List<string>();
		private static Dictionary<string, int> _allResDependenciesMap = new Dictionary<string, int>();


		//for test same name
		private static Dictionary<string, string> _allResKeyExt = new Dictionary<string, string>();
		private static List<string> _sameNameRes = new List<string>();

		private static StringBuilder _logStringBuilder = new StringBuilder();
		private static StringWriter _logWriter = new StringWriter(_logStringBuilder);

		//end

		private static int _pushAssetDependenciesNum = 0;

		public static void DoExport(BuildTarget platform)
		{
			_buildTarget = platform;


			string bundleRootPath = EditorUtil.GetAssetbundleRootPath(_buildTarget);

			EditorUtil.DeleteFolder(bundleRootPath);
			EditorUtil.CheckFolder(bundleRootPath);

			Type enumType = typeof(BuildTarget);
			string platformName = Enum.GetName(enumType, platform);
			_bundlesMap.Init(platformName, _curVersion, bundleRootPath, false);


			_bundlesMap.Bundles.RemoveBundlesByType(BundleType.BUNDLE_TEXTURES, 0);
			_bundlesMap.Bundles.RemoveBundlesByType(BundleType.BUNDLE_MODEL, 0);
			_bundlesMap.Bundles.RemoveBundlesByType(BundleType.BUNDLE_SHADER, 0);
			_bundlesMap.Bundles.RemoveBundlesByType(BundleType.BUNDLE_MATERIAL, 0);
			_bundlesMap.Bundles.RemoveBundlesByType(BundleType.BUNDLE_PREFAB, 0);




			for (int i = 0; i < _resDirectoryArray.Length; i++)
			{
				EditorUtil.ScanDirectory(Application.dataPath + "/" + _resDirectoryArray[i], checkResource);
			}

		
			checkAllFilesAndOutLog();

		
			for (int i = 0; i < 5; i++)
			{
				for (int j = _allResource.Count - 1; j >= 0; j--)
				{
					Object selectObj = AssetDatabase.LoadAssetAtPath(_allResource[j], typeof(Object));//Object.DestroyImmediate(selectObj,true);
					BundleType objBundleType = getBundleType(selectObj, _allResource[j]);

                    Debug.Log(_allResource[j] + " " + objBundleType.ToString());
					if (getResLayerIndexByType(objBundleType)==(i+1))
					{
						outAsset(_allResource[j], objBundleType);

						_allResource.RemoveAt(j);

					}
				}
			}

			//检查剩余的类型
			for (int i = _allResource.Count - 1; i >= 0; i--)
			{
				if (_allResource[i].EndsWith(".unity"))
				{
					string sceneFullPath = Path.GetFullPath(_allResource[i]);
					string sceneAssetRelativeName = GetRelativeName(sceneFullPath, Application.dataPath + "/");
					string tempBundlePath = GetBundlePath(sceneAssetRelativeName, _buildTarget,BundleType.BUNDLE_SCENE);

					string[] levelsStr = new string[] { "Assets/" + sceneAssetRelativeName };



					BuildPipeline.PushAssetDependencies();
					BuildPipeline.BuildStreamedSceneAssetBundle(levelsStr, tempBundlePath, _buildTarget);

					//记录Md5
					string MD5 = getMD5(sceneFullPath);//ABResChecker.GetMd5Hash(tempBundlePath).ToLower();
					string bundleKey = GetBundleKey(sceneAssetRelativeName, BundleType.BUNDLE_SCENE);
					int size = (int)getFileSize(sceneFullPath);
					_bundlesMap.Bundles.addBundleKey(bundleKey);

					string[] depends = getBundleDependencies(sceneAssetRelativeName, bundleKey);

					_bundlesMap.Bundles.SetBundleInfo(bundleKey, MD5,size, BundleType.BUNDLE_SCENE, depends);
					BuildPipeline.PopAssetDependencies();

					_allResource.RemoveAt(i);
				}


			}

			//========== OUT SECNE ASSET FILE ===============

			_exportStatus = string.Format("patch exported  successfully\r\n");

			popResDependencies(_pushAssetDependenciesNum);

			_bundlesMap.SaveBundleMap();


			Debug.Log("========================  Ignore Info  ===================================");
			int ignoreIndex = 0;
			foreach (string tmpRes in _allIgnoreResource)
			{
				if (!(tmpRes.EndsWith(".cs") || tmpRes.EndsWith(".js") || tmpRes.EndsWith(".assets")))
				{
					Debug.Log("Ignore res: index=" + ignoreIndex + ", " + tmpRes);
				}
				ignoreIndex++;
			}

			Debug.Log("========================  Not Out Info  ===================================");
			for (int i =0;i< _allResource.Count;i++)
			{
				Debug.Log("NOT Out res: index=" + i + ", " + _allResource[i]);
			}

			//reset
			_pushAssetDependenciesNum = 0;


			_allResource.Clear();
			_allIgnoreResource.Clear();
			_allResDependenciesMap.Clear();


			SaveStringToFile(Application.dataPath + "/exportLog.txt", _logStringBuilder.ToString());

			Debug.Log("============= OVER ==============");
		}

		private static void checkAllFilesAndOutLog()
		{
			System.DateTime now = System.DateTime.Now;
			string checkTime="Check Time:" + now.Year + "/" + now.Month + "/" + now.Day + " " + now.Hour + ":" + now.Minute;
			_logWriter.Write(checkTime);
			_logWriter.Write("\r\n");

			_logWriter.Write("1：检测资源被依赖的次数：\r\n");

			Dictionary<string, int> allResDependenciesMapAsc = _allResDependenciesMap.OrderBy(o => o.Value).ToDictionary(o => o.Key, p => p.Value);


			Dictionary<int, int> tmpDict = new Dictionary<int, int>();

			int index = 0;
			int curValue = 0;
			int curValueNum = 0;
			foreach (KeyValuePair<string, int> kv in allResDependenciesMapAsc)
			{

				if (curValue == kv.Value)
				{
					curValueNum++;
					
				}
				else
				{
					
					curValue = kv.Value;
					curValueNum = 1;
				}

				if (tmpDict.ContainsKey(curValue))
				{
					tmpDict[curValue] = curValueNum;
				}
				else
				{
					tmpDict.Add(curValue, curValueNum);
				}

				_logWriter.Write("  " + index + "： " + kv.Key + "=" + kv.Value+"\r\n");
				index++;

			}

			_logWriter.Write("\r\n");
			_logWriter.Write("2：被依赖相同次数的资源数量统计：\r\n");
			index = 0;
			foreach (KeyValuePair<int, int> kv in tmpDict)
			{
				index++;
				_logWriter.Write(">"+index+"  被依赖 "+  kv.Key+ " 次数的资源数量="+ kv.Value + "\r\n");
			}

			_logWriter.Write("\r\n");
			_logWriter.Write("3：所有资源的文件后缀名统计并检测所有应该类型文件的资源类型：\r\n");
			//测试所有资源的文件后缀名
			Dictionary<string, List<string>> tmpFileTypeDict = new Dictionary<string, List<string>>();

			foreach (string tmpRes in _allResource)
			{
				string ext = Path.GetExtension(tmpRes);
				if (!tmpFileTypeDict.ContainsKey(ext))
				{
					List<string> tmpList = new List<string>();
					tmpList.Add(tmpRes);
					tmpFileTypeDict.Add(ext, tmpList);
				}
				else
				{
					tmpFileTypeDict[ext].Add(tmpRes);
				}
			}

			int fileTypeIndex = 0;
			foreach (KeyValuePair<string, List<string>> kv in tmpFileTypeDict)
			{

				_logWriter.Write(">" + fileTypeIndex + "  " + "Extension=" + kv.Key +"\r\n");
				fileTypeIndex++;
			}

			_logWriter.Write("\r\n");
			fileTypeIndex = 0;
			foreach (KeyValuePair<string, List<string>> kv in tmpFileTypeDict)
			{
				_logWriter.Write("\r\n");
				_logWriter.Write(">" + fileTypeIndex + "  " + "Extension=" + kv.Key + "\r\n");
				List<string> resList = kv.Value;
				for (int i = 0; i < resList.Count; i++)
				{
					BundleType resType = getBundleTypeByPath(resList[i]);

					_logWriter.Write(">> " + resType + "   " + resList[i] + "\r\n");
				}


				fileTypeIndex++;
			}


//			SaveStringToFile(Application.dataPath + "/exportLog.txt", _logStringBuilder.ToString());
//			Debug.Log("============== save log.txt ================");
		}

		public static void SaveStringToFile(string FilePath, string Content)
		{
			System.IO.StreamWriter Sw = new System.IO.StreamWriter(FilePath);
			{
				Sw.Write(Content);
				Sw.Close();
			}
		}



		private static void outAsset(string resPath, BundleType bundleType)
		{

			if (resPath == null) return;

			if (resPath.StartsWith("Assets/GUiResource/UIPanelPrefabs/NumText.") || resPath.StartsWith("Assets/GUiResource/UIPanelPrefabs/SmallMapPanel."))
			{
				int aa = 100;
			}

			Object selectObj = AssetDatabase.LoadAssetAtPath(resPath, typeof(Object));

			bool haveDependencies = false;
			string[] allDepends = AssetDatabase.GetDependencies(new string[] { resPath });

			if (allDepends.Length > 1)
				haveDependencies = true;

			string fullPath = Path.GetFullPath(resPath);
			string assetRelativeName = GetRelativeName(fullPath, Application.dataPath + "/");
			string tempBundlePath = GetBundlePath(assetRelativeName, _buildTarget,bundleType);

			EditorUtil.CheckFolder(tempBundlePath);

			BuildAssetBundleOptions buildOp;
			if (tempBundlePath.IndexOf("_NM_") != -1)
			{
				buildOp = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets
					| BuildAssetBundleOptions.DeterministicAssetBundle 
						| BuildAssetBundleOptions.UncompressedAssetBundle;
			} else {
			    buildOp = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets
					| BuildAssetBundleOptions.DeterministicAssetBundle 
						/*| BuildAssetBundleOptions.UncompressedAssetBundle*/;
			}

			if (bundleType != BundleType.BUNDLE_BINARY)
			{
				BuildPipeline.PushAssetDependencies();
				_pushAssetDependenciesNum++;



				BuildPipeline.BuildAssetBundle(selectObj, null, tempBundlePath, buildOp, _buildTarget);


				bool needPop = false;
				if (!_allResDependenciesMap.ContainsKey(resPath))
				{
					needPop = true;
				}


				if (needPop)
				{
					BuildPipeline.PopAssetDependencies();
					_pushAssetDependenciesNum--;

					_logWriter.Write("   ---->>> OUT AND push:" + resPath + "\r\n");
					_logWriter.Write("   <<<----         pop :" + resPath + "\r\n");
				}
				else
				{
					_logWriter.Write("   ---->>> OUT AND push:" + resPath + "\r\n");

				}
			}
			else
			{

				File.Copy(fullPath, tempBundlePath, true); 

			}

			

			

			//记录Md5
			string MD5 = getMD5(fullPath);// ABResChecker.GetMd5Hash(tempBundlePath).ToLower();
			int size = (int)getFileSize(fullPath);
			string bundleKey = GetBundleKey(assetRelativeName, bundleType);

			_bundlesMap.Bundles.addBundleKey(bundleKey);

			string[] depends = getBundleDependencies(assetRelativeName, bundleKey);

			_bundlesMap.Bundles.SetBundleInfo(bundleKey, MD5,size, bundleType, depends);



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

			if (_allResKeyExt.ContainsKey(tmpKey))
			{
				string newPath = tmpKey + ext;

				string oldExt = _allResKeyExt[tmpKey];
				string oldPath = tmpKey + "" + oldExt;

				if (!_sameNameRes.Contains(oldPath))
				{
					_sameNameRes.Add(oldPath);
				}

				_sameNameRes.Add(newPath);
			}
			else
			{
				_allResKeyExt.Add(tmpKey, ext);
			}

		}

		private static void outSameFileInfo()
		{
			StringBuilder txt = new StringBuilder();
			StringWriter writer = new StringWriter(txt);

			_sameNameRes.Sort();
			

			for(int i=0;i<_sameNameRes.Count;i++)
			{
				string fileName = _sameNameRes[i];

				if (i > 0)
				{
					writer.Write("\r\n");
				}
				writer.Write(fileName);
			}

			IOUtils.SaveStringToFile(Application.dataPath + "/SameNameFiles.txt", txt.ToString());

			_sameNameRes.Clear();
			_allResKeyExt.Clear();

		}

		private static void checkResource(FileInfo fileInfo)
		{


			string fileName = fileInfo.FullName;

			if (fileName.EndsWith(".meta")) return;

			string relativeName = GetRelativeNameInApplication(fileName);

			//不输出的类型，如.aseet .cs  .js等文件加入忽略列表
			if (!needOutAssetsFile(getBundleTypeByPath(relativeName)))
			{
				if (!_allIgnoreResource.Contains(relativeName))
				{
					_allIgnoreResource.Add(relativeName);
				}
				
				return;
			}

			if (!_allResource.Contains(relativeName))
			{
				_allResource.Add(relativeName);
			}



			string[] tmpDepends = AssetDatabase.GetDependencies(new string[] { relativeName });


			for (int i = 0; i < tmpDepends.Length; i++)
			{
				BundleType tmpType=getBundleTypeByPath(tmpDepends[i]);

				if (tmpDepends[i].IndexOf("SelfMemberSkill") != -1)
				{
					int aaa = 100;
				}

				//不输出的类型，如.aseet .cs  .js等文件加入忽略列表
				bool test = needOutAssetsFile(tmpType);
				if (!test/*!needOutAssetsFile(getBundleTypeByPath(tmpDepends[i]))*/)
				{
					if (!_allIgnoreResource.Contains(tmpDepends[i]))
					{
						_allIgnoreResource.Add(tmpDepends[i]);
					}
					
					continue;
				}

				if (!_allResource.Contains(tmpDepends[i]))
				{
					_allResource.Add(tmpDepends[i]);
				}

				if (tmpDepends[i] != relativeName)
				{
					addResDependenciesMapItem(tmpDepends[i]);
				}
			}


		}

		private static void addResDependenciesMapItem(string key)
		{
			if (_allResDependenciesMap.ContainsKey(key))
			{
				_allResDependenciesMap[key] += 1;
			}
			else
			{
				_allResDependenciesMap.Add(key, 0);
			}
		}

		private static void popResDependencies(int size)
		{

			if (size > 0)
			{
				for (int i = 0; i < size; i++)
				{
					BuildPipeline.PopAssetDependencies();
					_logWriter.Write("   <<<----         pop :"  + "\r\n");
				}
			}
		}


		private static string[] getBundleDependencies(string bundlePathName, string bundleKey)
		{
			string[] allDepends = AssetDatabase.GetDependencies(new string[] { "Assets/" + bundlePathName });

			if (allDepends.Length < 2)
			{
				return null;
			}

			string[] realDepends = new string[allDepends.Length - 1];
			int index = 0;


			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < allDepends.Length; j++)
				{
					Object selectObj = AssetDatabase.LoadAssetAtPath(allDepends[j], typeof(Object));//Object.DestroyImmediate(selectObj,true);
					BundleType objBundleType = getBundleType(selectObj, allDepends[j]);


					if (getResLayerIndexByType(objBundleType) == (i + 1))
					{
						string assetRelativeName = GetRelativeName(allDepends[j], "Assets/");
						string tmpBundleKey = GetBundleKey(assetRelativeName, objBundleType);

						if (bundleKey != tmpBundleKey)
						{
							realDepends[index] = tmpBundleKey;
							index++;
						}

					}
				}
			}


			return realDepends;
		}

		private static int getResLayerIndexByType(BundleType bundleType)
		{
			int layerIndex = -1;
			switch (bundleType)
			{

				case BundleType.BUNDLE_SHADER:    //shader
				case BundleType.BUNDLE_TEXTURES:  //贴图
				case BundleType.BUNDLE_PHYSIC_MATERIAL:// PhysicMaterial
				case BundleType.BUNDLE_AUDIOCLIP: //音频
				
				case BundleType.BUNDLE_TEXTASSET: //TextAsset
				case BundleType.BUNDLE_BINARY:    //二进制
					layerIndex= 1;
					break;


				case BundleType.BUNDLE_MATERIAL:  //材质
				
					layerIndex = 2;
					break;

				case BundleType.BUNDLE_FONT:
					layerIndex = 3;
					break;

				case BundleType.BUNDLE_MODEL:     //模型
					layerIndex = 4;
					break;


				case BundleType.BUNDLE_PREFAB:    //预制物
				case BundleType.BUNDLE_GAMEOBJ:
					layerIndex = 5;
					break;


				case BundleType.BUNDLE_SCENE:    //场景
					layerIndex = 6;
					break;

				
				case BundleType.BUNDLE_ANIMATOR_CONTROLLER:
				case BundleType.BUNDLE_ANIMATION_CLIP:
				case BundleType.BUNDLE_TEXT:      //文本
				case BundleType.BUNDLE_SPRITE:    //精灵
				case BundleType.BUNDLE_EFFECT:    //特效
				case BundleType.BUNDLE_LIGHTMAP:  //光照贴图
				case BundleType.BUNDLE_GUI_SKIN://GUISkin
				case BundleType.BUNDLE_ASSET:
				case BundleType.BUNDLE_SCRIPT:
				case BundleType.BUNDLE_OTHER:
					break;

			}
			return layerIndex;
		}

//		private static bool isLayerWithId(BundleType bundleType, int layerId)
//		{
//			if (layerId == 0)
//			{
//				return isLayerOne(bundleType);
//			}
//			else if (layerId == 1)
//			{
//				return isLayerTwo(bundleType);
//			}
//			else if (layerId == 2)
//			{
//				return isLayerThree(bundleType);
//			}
//			else if (layerId == 3)
//			{
//				return isLayerFour(bundleType);
//			}
//			return false;
//		}
//
//		
//
//
//		private static bool isLayerOne(BundleType bundleType)
//		{
//			if (bundleType == BundleType.BUNDLE_TEXTURES) return true;
//			if (bundleType == BundleType.BUNDLE_SHADER) return true;
//			if (bundleType == BundleType.BUNDLE_PHYSIC_MATERIAL) return true;
//			if (bundleType == BundleType.BUNDLE_AUDIOCLIP) return true;
//			if (bundleType == BundleType.BUNDLE_TEXTASSET) return true;
//			if (bundleType == BundleType.BUNDLE_ANIMATION_CLIP) return true;
//
//			return false;
//		}
//
//		private static bool isLayerTwo(BundleType bundleType)
//		{
//			if (bundleType == BundleType.BUNDLE_FONT) return true;
//			if (bundleType == BundleType.BUNDLE_ANIMATOR_CONTROLLER) return true;
//			if (bundleType == BundleType.BUNDLE_MATERIAL) return true;
//
//
//			return false;
//		}
//
//		private static bool isLayerThree(BundleType bundleType)
//		{
//			if (bundleType == BundleType.BUNDLE_MODEL) return true;
//			
//			return false;
//		}
//
//		private static bool isLayerFour(BundleType bundleType)
//		{
//		
//			if (bundleType == BundleType.BUNDLE_PREFAB) return true;
//			if (bundleType == BundleType.BUNDLE_GAMEOBJ) return true;
//
//
//			return false;
//		}

		private static bool needOutAssetsFile(BundleType bundleType)
		{
		
			if (bundleType == BundleType.BUNDLE_ANIMATION_CLIP) return false;
			if (bundleType == BundleType.BUNDLE_GUI_SKIN) return false;
			if (bundleType == BundleType.BUNDLE_ASSET) return false;
			if (bundleType == BundleType.BUNDLE_SCRIPT) return false;
			if (bundleType == BundleType.BUNDLE_OTHER) return false;
			if (bundleType == BundleType.BUNDLE_ANIMATOR_CONTROLLER) return false;
			return true;
		}

		private static BundleType getBundleTypeByPath(string ObjPath)
		{
			if (ObjPath.EndsWith(".unity")) return BundleType.BUNDLE_SCENE;
			if (ObjPath.EndsWith(".asset")) return BundleType.BUNDLE_ASSET;
			if (ObjPath.EndsWith(".assets")) return BundleType.BUNDLE_ASSET;

			if (ObjPath.EndsWith(".bytes")) return BundleType.BUNDLE_BINARY;

			if (ObjPath.EndsWith(".cs")) return BundleType.BUNDLE_SCRIPT;
			if (ObjPath.EndsWith(".js")) return BundleType.BUNDLE_SCRIPT;
			if (ObjPath.EndsWith(".shader")) return BundleType.BUNDLE_SHADER;
			//if (ObjPath.EndsWith(".shader")) return BundleType.BUNDLE_SHADER;
			
			Object bundleObj = AssetDatabase.LoadAssetAtPath(ObjPath, typeof(Object));
			BundleType objBundleType = getBundleType(bundleObj, ObjPath);
			return objBundleType;
		}

		private static BundleType getBundleType(Object Obj, string ObjPath)
		{
			if (ObjPath.EndsWith(".unity")) return BundleType.BUNDLE_SCENE;
			if (ObjPath.EndsWith(".asset")) return BundleType.BUNDLE_ASSET;
			if (ObjPath.EndsWith(".assets")) return BundleType.BUNDLE_ASSET;

			if (ObjPath.EndsWith(".bytes")) return BundleType.BUNDLE_BINARY;

			if (ObjPath.EndsWith(".cs")) return BundleType.BUNDLE_SCRIPT;
			if (ObjPath.EndsWith(".js")) return BundleType.BUNDLE_SCRIPT;
			if (ObjPath.EndsWith(".shader")) return BundleType.BUNDLE_SHADER;

			//if (ObjPath.EndsWith(".shader")) return BundleType.BUNDLE_SHADER;


			// layer one
			if (Obj is UnityEngine.Texture)
			{
				return BundleType.BUNDLE_TEXTURES;//"texture";
			}
			else if (Obj is UnityEngine.Shader)
			{
				return BundleType.BUNDLE_SHADER;//"shader";
			}
			else if (Obj is UnityEngine.PhysicMaterial)
			{
				return BundleType.BUNDLE_PHYSIC_MATERIAL;
			}
			else if (Obj is UnityEngine.AudioClip)
			{
				return BundleType.BUNDLE_AUDIOCLIP;
			}
			else if (Obj is UnityEngine.TextAsset)
			{
				return BundleType.BUNDLE_TEXTASSET;
			}
			else if (Obj is UnityEngine.AnimationClip)
			{
				return BundleType.BUNDLE_ANIMATION_CLIP;
			}

			//layer two
			else if (Obj is UnityEngine.Font)
			{
				return BundleType.BUNDLE_FONT;
			}
			else if (Obj is UnityEditor.Animations.AnimatorController)
			{
				return BundleType.BUNDLE_ANIMATOR_CONTROLLER;
			}
			else if (Obj is UnityEngine.Material)
			{
				return BundleType.BUNDLE_MATERIAL;//"material";
			}

			else if (Obj is UnityEngine.GUISkin)
			{
				return BundleType.BUNDLE_GUI_SKIN;
			}
			else if (Obj is UnityEngine.Mesh)
			{
				return BundleType.BUNDLE_MODEL;//"mesh";
			}
			else if (Obj is UnityEngine.GameObject)
			{
				PrefabType tmpPrefabType = PrefabUtility.GetPrefabType(Obj);
				
				if (tmpPrefabType == PrefabType.Prefab)
				{
					return BundleType.BUNDLE_PREFAB;//"prefab";
				}else if (tmpPrefabType == PrefabType.ModelPrefab)
				{
					return BundleType.BUNDLE_MODEL;//"mesh";
				}
				else
				{
					return BundleType.BUNDLE_GAMEOBJ;//"GameObj";
				}
			}
			else
			{
				return BundleType.BUNDLE_OTHER;//"other";
			}
		}


		private static string GetBundleKey(string assetRelativePath, BundleType bundleType)
		{
			string name = "";
//			string[] dirctoryStrArray = Path.GetDirectoryName(assetRelativePath).Split("/".ToCharArray());
//			for (int i = 0; i < dirctoryStrArray.Length; i++)
//			{
//				name += dirctoryStrArray[i].Substring(0, 1);
//			}
//			name += "_";
			name += Path.GetFileNameWithoutExtension(assetRelativePath);

			return string.Format("{0}/{1}", Path.GetDirectoryName(assetRelativePath), name);
		}

		private static string GetRelativeName(string assetPath, string sourceRootPath)
		{
			assetPath = assetPath.Replace("\\", "/");
			return assetPath.Substring(sourceRootPath.Length);
		}

		private static string GetRelativeNameInApplication(string assetPath)
		{
			string sourceRootPath = Application.dataPath;
			assetPath = assetPath.Replace("\\", "/");
			return assetPath.Substring(sourceRootPath.Length - 6);
		}

		private static string GetBundlePath(string assetRelativePath, BuildTarget platform, BundleType bundleType)
		{
			string path = EditorUtil.GetAssetbundleRootPath(platform);
			path = path + assetRelativePath;
			path = path.Replace("\\", "/");

			string name = "";
//			string[] dirctoryStrArray = Path.GetDirectoryName(assetRelativePath).Split("/".ToCharArray());
//			for (int i = 0; i < dirctoryStrArray.Length; i++)
//			{
//				name += dirctoryStrArray[i].Substring(0, 1);
//			}
//			name += "_";
			name += Path.GetFileNameWithoutExtension(path);

			string md5 = getMD5(Application.dataPath + "/"+assetRelativePath);

			string ext = ".asset";
//			if (bundleType == BundleType.BUNDLE_SCENE)
//			{
//				ext = ".unity3d";
//			}

			return string.Format("{0}/{1}_{2}" + ext, Path.GetDirectoryName(path), name, md5);
		}

		private static string getMD5(string fullpath )
		{

			//string assetRelativeName = GetRelativeName(fullpath, Application.dataPath + "/");
			fullpath = fullpath.Replace("\\", "/");


			return ABResChecker.GetMd5Hash(fullpath).ToLower();
			
		}

		private static long getFileSize(string fullpath)
		{

			
			//Debug.Log("<color=#FFFF00ff> resource fullpath=" + fullpath + "</color>");

			FileInfo fileInfo = new FileInfo(fullpath);
			long size = 0;
			if (fileInfo.Exists)
			{

				size = fileInfo.Length;
			}

			return size;
		}



		

	}

}