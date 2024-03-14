using System;

namespace UnityBase.Command
{
    public abstract class MoveCommand : ICommand
    {
        protected readonly IMoveEntity _moveEntity;
        public abstract bool IsInProgress { get; }
        public abstract bool CanPassNextCommandInstantly { get; }
        protected MoveCommand(IMoveEntity moveEntity) => _moveEntity = moveEntity;
        public abstract void Record();
        public abstract void Execute(Action onComplete);
        public abstract void Undo(bool directly, Action onComplete);
        public abstract void Redo(bool directly, Action onComplete);
        public abstract void Cancel();
        public abstract void Dispose();
        public static T Create<T>(IMoveEntity moveEntity) where T : MoveCommand => (T)Activator.CreateInstance(typeof(T), moveEntity);
    }
}