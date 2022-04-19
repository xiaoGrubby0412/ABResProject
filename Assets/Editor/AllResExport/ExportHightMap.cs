using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using Asgard;
using System.Collections.Generic;
using UnityEngine.EventSystems;


using FileMode = System.IO.FileMode;
using FileStream=System.IO.FileStream;
using BinaryWriter=System.IO.BinaryWriter;

namespace Asgard
{
	public class ExportHightMap : EditorWindow
	{

		
		

		[MenuItem("AllResExport/ExportHeihtMap")]
		public static void ExportHeihtMap()
		{
            GameObject root = GameObject.Find("SceneRoot");
            if (!root)
            {
                Debug.Log("没有找到SenceRoot");
                return;
            }
            Terrain[] terrains = root.GetComponentsInChildren<Terrain>(true);
            if (terrains.Length==0 || terrains.Length > 1)
            {
                Debug.Log("该地图地形有问题"); 
                return;
            }
            Terrain curTerrain = terrains[0];

			int heightmapResolution = curTerrain.terrainData.heightmapResolution;

			float mapw = curTerrain.terrainData.size.x;
			float maph = curTerrain.terrainData.size.z;

			float maxHight = curTerrain.terrainData.size.y;

			float cellw = mapw / (heightmapResolution - 1);
			float cellh = mapw / (heightmapResolution - 1);

            string filePath = EditorApplication.currentScene;
            filePath = filePath.Substring(0, filePath.Length - 6);
            filePath = string.Join("_", new string[] {filePath,((int)mapw).ToString(),((int)maph).ToString(), ((int)maxHight).ToString(), heightmapResolution.ToString(), "16"});
            filePath = Path.ChangeExtension(filePath, "raw");
            FileStream fs = new FileStream(filePath, FileMode.Create);
			BinaryWriter outBW = new BinaryWriter(fs);

			ushort lastHH = 0;
			
			RaycastHit[] rayHits;
            RaycastHit rayHit;
            int layerMask = 1 << AsgardConst.LAYER_GROUND;
            Ray ray = new Ray(Vector3.zero, Vector3.down);
            Debug.Log("开始导出高度图: " + Path.GetFileNameWithoutExtension(EditorApplication.currentScene));
            for (int i = 0; i < heightmapResolution; i++)
            {
                for (int j = 0; j < heightmapResolution; j++)
                {
                    ray.origin = new Vector3(cellw * j, maxHight * 2, cellh * i);

                    float tmpY = CustomPathUtility.GetHitPos(ray).y;
                    if (tmpY < 0)
                    {
                        Debug.LogError("高度图错误" + i + "  " + j + "    " + tmpY + "     " + curTerrain.terrainData.GetHeight(i, j) + "     " + curTerrain.terrainData.GetHeight(j, i));
                        return;
                    }
                    tmpY = 65535 * tmpY * 100 / maxHight;
                    lastHH = (ushort)(tmpY / 100);
                    //开始写入
                    //sw.Write("");
                    outBW.Write(lastHH);
                }
            }
			//清空缓冲区
			outBW.Flush();
			//关闭流
			outBW.Close();
            fs.Close();
            Debug.Log("为服务器导出高度图完毕 ! ");
            AssetDatabase.Refresh();
		}
	}
}
