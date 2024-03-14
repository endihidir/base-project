using System;

namespace UnityBase.Command
{
    public interface ICommandRecorder : IDisposable
    {
        public void Execute(ICommand command, Action onComplete);
        public void Undo(bool directly, Action onComplete);
        public void Redo(bool directly, Action onComplete);
        public void RecordCommand(ICommand command);
        public void ExecuteRecordedCommands(bool directly = false);
        public new void Dispose();
    }
}