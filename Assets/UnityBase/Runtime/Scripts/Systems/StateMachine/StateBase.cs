using UnityBase.BlackboardCore;

namespace UnityBase.StateMachineCore
{
    public interface IState
    {
        public string StateID { get; }
        public bool HasInit { get; }
        public bool IsActive { get; }
        public bool NeedsExitTime { get; }
        public bool IsExitReady { get; }
        public void RequestExit();
        public IState Init(IBlackboard blackboard = null, bool showLogs = true);
        public void Enter();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
        public bool Exit();
        public void ClearAll();
    }

    public abstract class StateBase : IState
    {
        public string StateID { get; protected set; }
        public bool HasInit { get; private set; }
        public bool IsActive { get; private set; }
        protected IBlackboard Blackboard { get; private set; }

        protected bool ShowLogs;

        public virtual bool NeedsExitTime => false;
        public bool IsExitReady { get; private set; }

        protected StateBase()
        {
            StateID = GetType().ToString();
        }

        public IState Init(IBlackboard blackboard = null, bool showLogs = true)
        {
            Blackboard ??= blackboard;

            ShowLogs = showLogs;

            if (HasInit) return this;

            HasInit = true;

            OnInit();

            return this;
        }

        public void Enter()
        {
            if (!HasInit)
            {
                if(ShowLogs)
                    DebugLogger.LogError($"The {StateID} state has not init yet! You need to init the state before enter!");

                return;
            }

            if(IsActive) return;

            var canActivate = OnBeforeEnter();

            if (!canActivate) return;

            IsExitReady = !NeedsExitTime;
            
            IsActive = true;

            OnEnter();
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            OnUpdate(deltaTime);
        }

        public void FixedUpdate(float deltaTime)
        {
            if (!IsActive) return;

            OnFixedUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            if(!IsActive) return;

            OnLateUpdate(deltaTime);
        }

        public virtual bool Exit()
        {
            if (!IsActive) return false;

            PerformExit();

            return true;
        }

        protected void PerformExit()
        {
            if(!IsActive) return;

            IsActive = false;

            IsExitReady = false;

            OnExit();
        }

        public virtual void RequestExit() => IsExitReady = true;

        protected abstract void OnInit();
        protected abstract bool OnBeforeEnter();
        protected abstract void OnEnter();
        protected abstract void OnUpdate(float deltaTime);
        protected abstract void OnFixedUpdate(float deltaTime);
        protected abstract void OnLateUpdate(float deltaTime);
        protected abstract void OnExit();
        
        public virtual void ClearAll()
        {
            StateID = string.Empty;
            HasInit = false;
            IsActive = false;
            Blackboard = null;
            ShowLogs = false;
            IsExitReady = false;
        }
    }
}
