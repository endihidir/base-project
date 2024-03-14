using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityBase.Command;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CommandManager : ICommandDataService, IAppPresenterDataService
    {
        private IDictionary<string, CommandRecorderGroup> _commandRecorders = new Dictionary<string, CommandRecorderGroup>();

        public CommandManager()
        {
            
        }
        
        ~CommandManager() => Dispose();

        public void Initialize() { }
        public void Start() { }
        public void Dispose()
        {
            _commandRecorders.ForEach(x => x.Value.Dispose());
            
            _commandRecorders = null;
            
            GC.SuppressFinalize(this);
        }

        public void AddRecorder(string groupName, ICommandRecorder commandRecorder)
        {
            if (!_commandRecorders.TryGetValue(groupName, out var commandGroup))
            {
                commandGroup = new CommandRecorderGroup();

                _commandRecorders.Add(groupName, commandGroup);
            }

            commandGroup.Add(commandRecorder);
        }

        public void RemoveRecorder(string groupName, ICommandRecorder commandRecorder)
        {
            if (!TryGetCommandGroup(groupName, out var commandGroup)) return;
            
            commandGroup.Remove(commandRecorder);
        }
        
        public void ExecuteAllRecords(string groupName, ICommand command, Action onComplete)
        {
            if (!TryGetCommandGroup(groupName, out var commandGroup)) return;
            
            commandGroup.ExecuteAllRecords(command, onComplete);
        }

        public void UndoAllRecords(string groupName, bool directly, Action onComplete)
        {
            if (!TryGetCommandGroup(groupName, out var commandGroup)) return;
            
            commandGroup.UndoAllRecords(directly, onComplete);
        }

        public void RedoAllRecords(string groupName, bool directly, Action onComplete)
        {
            if (!TryGetCommandGroup(groupName, out var commandGroup)) return;

            commandGroup.RedoAllRecords(directly, onComplete);
        }
        
        public void RecordAllCommands(string groupName, ICommand command)
        {
            if (!TryGetCommandGroup(groupName, out var commandGroup)) return;

            commandGroup.RecordAllCommands(command);
        }
        
        public void ExecuteAllRecordedCommands(string groupName, bool directly = false)
        {
            if (!TryGetCommandGroup(groupName, out var commandGroup)) return;

            commandGroup.ExecuteAllRecordedCommands(directly);
        }

        private bool TryGetCommandGroup(string groupName, out CommandRecorderGroup commandRecorderGroup)
        {
            if (_commandRecorders.TryGetValue(groupName, out commandRecorderGroup)) return true;
            
            Debug.LogError($" - {groupName} - command group name not exist.");
            
            return false;
        }
    }
}