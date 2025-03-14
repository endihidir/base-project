using System;
using UnityBase.Scripts.Attributes;
using UnityEngine;

namespace __Funflare.Scripts.Editor
{
    [CreateAssetMenu(menuName = "Config/TileEditor", fileName = "TileEditorConfig")]
    public class TileEditorConfig : ScriptableObject
    {
        [SerializeField] private ObjectConfig[] objectConfigs;
        //public MatActivateController pfMatActivateController;
        

        public bool TryGet(GameObject pf, out ObjectConfig result)
        {
            for (var i = 0; i < objectConfigs.Length; i++)
            {
                if (objectConfigs[i].pfSource.Equals(pf))
                {
                    result = objectConfigs[i];
                    return true;
                }
            }

            result = default(ObjectConfig);
            return false;
        }
        
        [Serializable]
        public struct ObjectConfig
        {
            public GameObject pfSource;
            
            [ConstantDropdown(typeof(TileEditorObjectCategory))]
            public string placementCategory;

            public bool disableAutoVisualOffset;
        }
    }


    public static class TileEditorObjectCategory
    {
        public const string Other = "other";
        public const string Machine = "machine";
        public const string Environment = "environment";
        public const string Walls = "walls";
        public const string Floors = "floors";

    }
}