using System;
using Eflatun.SceneReference;

namespace UnityBase.ManagerSO
{
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        
        public string Name => reference.Name;
    }
}