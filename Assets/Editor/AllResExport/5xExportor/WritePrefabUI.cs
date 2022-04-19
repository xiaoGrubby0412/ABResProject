using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using Asgard;
using Asgard.Resource;
using System;

public class WritePrefabUI
{
    [MenuItem("Assets/所有字体变黑")]
    public static void ChangeText()
    {
        List<UnityEngine.Object> selectedDics = new List<UnityEngine.Object>(Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets));
        selectedDics.RemoveAll(item =>
        {
            bool condition1 = Directory.Exists(AssetDatabase.GetAssetPath(item.GetInstanceID()));
            bool condition2 = !(item is GameObject);
            return condition1 || condition2;
        });

        GameObject go = GameObject.Instantiate(selectedDics[0]) as GameObject;
        UnityEngine.UI.Text[] tr = go.GetComponentsInChildren<UnityEngine.UI.Text>();
        if (tr != null && tr.Length > 0)
        {
            for (int i = 0; i < tr.Length; i++)
            {
                tr[i].color = UnityEngine.Color.black;
            }
        }
        PrefabUtility.ReplacePrefab(go, selectedDics[0]);
        GameObject.DestroyImmediate(go);
    }

    [MenuItem("Assets/生成UI变量代码 主UI")]
    public static void CreateUIStrs()
    {
        ifMainUI = true;
        StartCreateStr();
    }

    [MenuItem("Assets/生成UI变量代码 子UI")]
    public static void CreateSubUIStr()
    {
        ifMainUI = false;
        StartCreateStr();
    }

    private static void StartCreateStr()
    {
        List<UnityEngine.Object> selectedDics = new List<UnityEngine.Object>(Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets));
        selectedDics.RemoveAll(item =>
        {
            bool condition1 = Directory.Exists(AssetDatabase.GetAssetPath(item.GetInstanceID()));
            bool condition2 = !(item is GameObject);
            return condition1 || condition2;
        });

        GameObject go = GameObject.Instantiate(selectedDics[0]) as GameObject;

        list = new List<UIItem>();
        result = "";

        StartRecursive(go);
        PrefabUtility.ReplacePrefab(go, selectedDics[0]);
        GameObject.DestroyImmediate(go);

    }

    private static void StartRecursive(GameObject go)
    {
        ButtonTitleStr1 = ButtonTitleStr;
        TextTitleStr1 = TextTitleStr;
        ImageTitleStr1 = ImageTitleStr;
        ToggleTitleStr1 = ToggleTitleStr;
        ObjTitleStr1 = ObjTitleStr;
        ScrollBarTitleStr1 = ScrollBarTitleStr;
        ContainerTitleStr1 = ContainerTitleStr;

        Recursive(go);

        BaseDynamicUIData dyn = go.GetComponent<BaseDynamicUIData>();
        if (dyn == null) dyn = go.AddComponent<BaseDynamicUIData>();
        dyn.DataList = new List<UIControlData>();

        int containerIndex = 0;
        list.ForEach(item =>
        {
            UIControlData data = new UIControlData();
            data.obj = item.obj;
            data.DataType = item.type;
            dyn.DataList.Add(data);

            if (item.type == UIDataType.Container)
            {
                UIItemContainer container = item.obj.GetComponent<UIItemContainer>();
                if (container == null) container = item.obj.AddComponent<UIItemContainer>();
                container.index = containerIndex++;
                container.prefabName = item.name.Split('_')[1];
            }

            result = result + item.Str + "\n";
        });


        result = result + "\n";
        result += ifMainUI ? MainTitleStr : MainTitleStrLayout;
        list.ForEach(item =>
        {
            //result = result + item.FindStr + "\n"; ;
            if (item.type == UIDataType.Button)
            {
                ButtonTitleStr1 += "\n" + item.FindStr;
            }
            else if (item.type == UIDataType.Image)
            {
                ImageTitleStr1 += "\n" + item.FindStr;
            }
            else if (item.type == UIDataType.Text)
            {
                TextTitleStr1 += "\n" + item.FindStr;
            }
            else if (item.type == UIDataType.Toggle)
            {
                ToggleTitleStr1 += "\n" + item.FindStr;
            }
            else if (item.type == UIDataType.GameObject)
            {
                ObjTitleStr1 += "\n" + item.FindStr;
            }
            else if (item.type == UIDataType.ScrollBar)
            {
                ScrollBarTitleStr1 += "\n" + item.FindStr;
            }
            else if (item.type == UIDataType.Container)
            {
                ContainerTitleStr1 += "\n" + item.FindStr;
            }
        });
        ButtonTitleStr1 += subEnd;
        ImageTitleStr1 += subEnd;
        TextTitleStr1 += subEnd;
        ToggleTitleStr1 += subEnd;
        ObjTitleStr1 += subEnd;
        ScrollBarTitleStr1 += subEnd;
        ContainerTitleStr1 += subEnd;

        result += ButtonTitleStr1;
        result += ImageTitleStr1;
        result += TextTitleStr1;
        result += ToggleTitleStr1;
        result += ObjTitleStr1;
        result += ScrollBarTitleStr1;
        result += ContainerTitleStr1;

        result += MainEndStr;

        result += "\n";
        list.ForEach(item =>
        {
            if (item.type == UIDataType.Button)
            {
                result += item.name + BtnRemoveAllListener + "\n";
                result += item.name + BtnAddListenerTitle + item.name + "OnClickCallBack" + BtnAddListenerEnd + "\n";
            }
        });

        result += "\n";

        list.ForEach(item =>
        {
            if (item.type == UIDataType.Button)
            {
                result += BtnClickMethodTitle + item.name + "OnClickCallBack()" + BtnClickMethodEnd + "\n";
            }
        });

        Debug.LogError(result);
    }

    private static bool ifMainUI = true;
    private static List<UIItem> list;
    private static string result;

    private static string ButtonTitleStr1;
    private static string TextTitleStr1;
    private static string ImageTitleStr1;
    private static string ToggleTitleStr1;
    private static string ObjTitleStr1;
    private static string ScrollBarTitleStr1;
    private static string ContainerTitleStr1;

    private static void Recursive(GameObject parentGameObject)
    {
        string name = parentGameObject.name;
        if (name.StartsWith("txt"))
        {
            list.Add(new UIItem(UIDataType.Text, name, parentGameObject));
        }
        else if (name.StartsWith("img"))
        {
            list.Add(new UIItem(UIDataType.Image, name, parentGameObject));
        }
        else if (name.StartsWith("btn"))
        {
            list.Add(new UIItem(UIDataType.Button, name, parentGameObject));
        }
        else if (name.StartsWith("toggleBtn") || (name.StartsWith("togBtn")))
        {
            list.Add(new UIItem(UIDataType.Toggle, name, parentGameObject));
        }
        else if (name.StartsWith("obj"))
        {
            list.Add(new UIItem(UIDataType.GameObject, name, parentGameObject));
        }
        else if (name.StartsWith("scrollBar"))
        {
            list.Add(new UIItem(UIDataType.ScrollBar, name, parentGameObject));
        }
        else if (name.StartsWith("layoutContainer"))
        {
            list.Add(new UIItem(UIDataType.Container, name, parentGameObject));
        }

        foreach (Transform child in parentGameObject.transform)
        {
            Recursive(child.gameObject);
        }
    }


    private class UIItem
    {
        public UIItem(UIDataType type, string name, GameObject obj)
        {
            this.type = type;
            this.name = name;
            this.obj = obj;
        }

        public UIDataType type;
        public string name;
        public GameObject obj;

        public string Str
        {
            get
            {
                string typeStr = "";
                if (type == UIDataType.Button)
                {
                    typeStr = "Button";
                }
                else if (type == UIDataType.Image)
                {
                    typeStr = "Image";
                }
                else if (type == UIDataType.Text)
                {
                    typeStr = "Text";
                }
                else if (type == UIDataType.Toggle)
                {
                    typeStr = "Toggle";
                }
                else if (type == UIDataType.GameObject)
                {
                    typeStr = "GameObject";
                }
                else if (type == UIDataType.ScrollBar)
                {
                    typeStr = "Scrollbar";
                }
                else if (type == UIDataType.Container)
                {
                    typeStr = "UIItemContainer";
                }
                return "private " + typeStr + " " + this.name + ";";
            }
        }
        public string FindStr
        {
            get
            {
                if (this.type == UIDataType.Container)
                {
                    string str = this.name + ".SetUIBaseItem (typeof(" + this.name.Split('_')[1] + "));";
                    return "case" + "\"" + this.name + "\"" + ": \n" + this.name + " = " + Enum.GetName(typeof(UIDataType), this.type) + "; \n" + str + " break;";
                }
                else
                {
                    return "case" + "\"" + this.name + "\"" + ": " + this.name + " = " + Enum.GetName(typeof(UIDataType), this.type) + "; break;";
                }
            }
        }
    }

    private static string MainTitleStr =
    @"public override void OnPostUIAssemble(BaseUIData data)
      {
        switch (data.Type)
        {";

    private static string MainTitleStrLayout =
    @"protected override void OnPostUIAssemble(BaseUIData data)
      {
        switch (data.Type)
        {";

    private static string MainEndStr =
    @"}
    }";

    private static string subEnd =
    @"}
      break;";

    private const string ButtonTitleStr =
    @"case UIDataType.Button:
           Button Button = data.Component as Button;
           switch (data.Name)
           {";

    private const string TextTitleStr =
    @"case UIDataType.Text:
           Text Text = data.Component as Text;
           switch (data.Name)
           {";

    private const string ImageTitleStr =
    @"case UIDataType.Image:
           Image Image = data.Component as Image;
           switch (data.Name)
           {";

    private const string ToggleTitleStr =
    @"case UIDataType.Toggle:
           Toggle Toggle = data.Component as Toggle;
           switch(data.Name)
           {";

    private const string ObjTitleStr =
    @"case UIDataType.GameObject:
           GameObject GameObject = data.Obj;
           switch (data.Name)
           {";

    private const string ContainerTitleStr =
    @"case UIDataType.Container:
           UIItemContainer Container = data.Component as UIItemContainer;
           switch (data.Name)
           {";

    private const string ScrollBarTitleStr =
    @"case UIDataType.ScrollBar:
           Scrollbar ScrollBar = data.Component as Scrollbar;
           switch(data.Name)
           {";

    private const string BtnRemoveAllListener =
        @".onClick.RemoveAllListeners();";
    private const string BtnAddListenerTitle =
        @".onClick.AddListener(";
    private const string BtnAddListenerEnd =
        @");";

    private const string BtnClickMethodTitle =
        @"public void ";

    private const string BtnClickMethodEnd =
    @"
    {

    }";
}
