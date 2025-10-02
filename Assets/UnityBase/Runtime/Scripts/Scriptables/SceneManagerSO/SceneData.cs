using System;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;

namespace UnityBase.Managers.SO
{
    [Serializable]
    public class SceneData
    {
        public SceneReference reference;
        public string Name => reference.Name;
    }
}