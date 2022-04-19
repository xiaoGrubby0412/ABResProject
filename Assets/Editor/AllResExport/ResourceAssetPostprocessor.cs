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

public class ResourceAssetPostprocessor : AssetPostprocessor
{
    /// <summary>
    /// 模型导入处理
    /// </summary>
    public void OnPreprocessModel()
    {
        Debug.Log("OnPreprocessModel=" + this.assetPath);
        if(this.assetPath.StartsWith("Assets/TankTrack/Resource/Tank/"))
        {
            //坦克FBX模型处理
            ModelImporter modelImporter = this.assetImporter as ModelImporter;
            modelImporter.globalScale = 0.01f;
            modelImporter.meshCompression = ModelImporterMeshCompression.Off;
			modelImporter.isReadable = assetPath.Contains("_simple")?true:false;//用作物理判断的简模需要可读
            modelImporter.optimizeMesh = true;
            modelImporter.importBlendShapes = true;
            modelImporter.addCollider = true;

            modelImporter.swapUVChannels = false;
            modelImporter.generateSecondaryUV = false;
            modelImporter.importNormals = ModelImporterNormals.Import;
            modelImporter.importTangents = ModelImporterTangents.CalculateMikk;
            modelImporter.importMaterials = true;
            modelImporter.materialName = ModelImporterMaterialName.BasedOnTextureName;
            modelImporter.materialSearch = ModelImporterMaterialSearch.RecursiveUp;

            modelImporter.animationType = ModelImporterAnimationType.Generic;
            modelImporter.optimizeGameObjects = false;
        }
        else if (this.assetPath.StartsWith("Assets/Medias/MeiShu/GongYong/Model/"))
        {
            if (assetPath.EndsWith("qizipiaodong.FBX")) return;
            //树和房子石头等等FBX模型导入处理
            ModelImporter modelImporter = this.assetImporter as ModelImporter;
            //modelImporter.globalScale = 0.01f;
            modelImporter.meshCompression = ModelImporterMeshCompression.Off;
            modelImporter.isReadable = true;
            modelImporter.optimizeMesh = true;
            modelImporter.importBlendShapes = true;
            modelImporter.addCollider = false;

            modelImporter.swapUVChannels = false;
            modelImporter.generateSecondaryUV = true;
            modelImporter.importNormals = ModelImporterNormals.Import;
            modelImporter.importTangents = ModelImporterTangents.CalculateLegacyWithSplitTangents;
            modelImporter.importMaterials = true;
            modelImporter.materialName = ModelImporterMaterialName.BasedOnTextureName;
            modelImporter.materialSearch = ModelImporterMaterialSearch.RecursiveUp;

            modelImporter.animationType = ModelImporterAnimationType.None;
        }
    }

    /// <summary>
    /// 图片导入处理 
    /// </summary>
    public void OnPreprocessTexture()
    {
        Debug.Log("OnPreprocessTexture ==" + this.assetPath);
        if(this.assetPath.StartsWith("Assets/GUiResource/Src/"))
        {
            //src目录下面的文件
            TextureImporter textureImporter = this.assetImporter as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.spritePackingTag = "";
            textureImporter.spritePixelsPerUnit = 100.0f;
            textureImporter.mipmapEnabled = false;
            textureImporter.filterMode = FilterMode.Bilinear;
            textureImporter.maxTextureSize = 1024;
            textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        }
        else if (this.assetPath.StartsWith("Assets/UI/GUiResource/Atlas/"))
        {
            //Atlas的处理 需要在打图集的时候 进行处理
        }
    }

    /// <summary>
    /// 音频文件导入处理
    /// </summary>
    public void OnPreprocessAudio()
    {
        Debug.Log("OnPreprocessAudio == " + this.assetPath);
        if(this.assetPath.StartsWith("Assets/Medias/Audios/3/"))
        {
            AudioImporter audio = this.assetImporter as AudioImporter;
            audio.forceToMono = false;
            audio.loadInBackground = false;
            AudioImporterSampleSettings setting = new AudioImporterSampleSettings();
            setting.loadType = AudioClipLoadType.Streaming;
            setting.compressionFormat = AudioCompressionFormat.Vorbis;
            setting.quality = 0.01f;
            setting.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            audio.defaultSampleSettings = setting;
        }

    }
}

