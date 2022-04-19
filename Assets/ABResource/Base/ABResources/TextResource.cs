﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Asgard.Resource
{
    public class TextResource : BaseResource
    {
        public TextResource(ABResMapItemScriptObj abResMapItem, ResourceState resourceState, Storage storage)
            : base(abResMapItem, resourceState, storage)
        {

        }
        public string Text
        {
            get
            {
                return this.assetBundle.LoadAsset<TextAsset>(loadName).text;
            }

        }
    }
}
