using UnityBase.Service;
using UnityEngine;
using VContainer;

namespace UnityBase.Command
{
    public class MoveCommandControllerTest : MonoBehaviour, IMoveEntity
    {
        [Inject] 
        private readonly ICommandDataService _commandDataService;

        [Inject] 
        private readonly ICommandRecorder _commandRecorder;

        private ICommand _moveCommand;
        public Transform MeshHandlerTransform => transform;
        public Transform Transform => transform;
        public Vector3 TargetPosition => Input.mousePosition;
        public float Speed => 0.2f;
        public bool CanPassNextMovementInstantly => true;

        private void Awake()
        {
            // _commandManager.AddRecorder("Endi", _commandRecorder);
        }

        private void Update()
        {
            //------------------------------------------------------------------------
            if (Input.GetKeyDown(KeyCode.S))
            {
                _moveCommand = MoveCommand.Create<ObjectMoveCommand>(this);

                _commandRecorder.RecordCommand(_moveCommand);

                //_commandRecorder.ExecuteSavedCommands();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                _commandRecorder.ExecuteRecordedCommands();
            }

            //------------------------------------------------------------------------

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _moveCommand = MoveCommand.Create<ObjectMoveCommand>(this);

                _commandRecorder.Execute(_moveCommand, default);
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                //_commandManager.UndoAllRecords("Endi");

                _commandRecorder.Undo(false, default);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                //_commandManager.RedoAllRecords("Endi");

                _commandRecorder.Redo(false,default);
            }

            //------------------------------------------------------------------------
        }

        private void OnDestroy()
        {
            _commandRecorder?.Dispose();

            _moveCommand?.Dispose();
        }
    }
}