using UnityEngine;

namespace UnityBase.Command
{
    public interface IMoveEntity
    {
        public Transform Transform { get; }
        public Transform MeshHandlerTransform { get; }
        public Vector3 TargetPosition { get; }
        public float Speed { get; }
        public bool CanPassNextMovementInstantly { get; }
    }
}