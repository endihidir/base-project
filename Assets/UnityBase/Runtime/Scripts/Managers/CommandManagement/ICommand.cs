using System;

namespace UnityBase.Command
{
    public interface ICommand
    {
        public bool IsInProgress { get; }
        public bool CanPassNextCommandInstantly { get; }
        public void Record();
        public void Execute(Action onComplete);
        public void Undo(bool directly, Action onComplete);
        public void Redo(bool directly, Action onComplete);
        public void Cancel();
        public void Dispose();
    }
}