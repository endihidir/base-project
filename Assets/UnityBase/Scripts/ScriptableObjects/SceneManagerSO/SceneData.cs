using System;
using Eflatun.SceneReference;

namespace UnityBase.Managers.SO
{
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public string Name => reference.Name;
    }
}