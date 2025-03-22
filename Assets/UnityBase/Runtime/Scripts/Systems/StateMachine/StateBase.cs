using UnityBase.BlackboardCore;
using UnityEngine;

namespace UnityBase.StateMachineCore
{
    public interface IState
    {
        public string StateID { get; }
        public bool HasInit { get; }
        public bool IsActive { get; }
        public IState Init();
        public IState InitWith(IBlackboard blackboard);
        public void Enter();
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
        public void Exit();
    }
    
    public abstract class StateBase : IState
    {
        public virtual string StateID => GetType().ToString();
        public bool HasInit { get; private set; }
        public bool IsActive { get; private set; }
        protected IBlackboard Blackboard { get; private set; }

        public IState Init()
        {
            if (HasInit) return this;
            
            HasInit = true;
            
            OnInit();
            
            return this;
        }

        public IState InitWith(IBlackboard blackboard)
        {
            if (HasInit) return this;
            
            Blackboard = blackboard;
            
            HasInit = true;
            
            OnInit();
            
            return this;
        }
        
        public void Enter()
        {
            if (!HasInit)
            {
                Debug.LogError($"The {StateID} state has not init yet! You need to init the state before enter!");
                return;
            }

            if(IsActive) return;
            
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
        
        public void Exit()
        {
            if(!IsActive) return;
            
            IsActive = false;
            
            OnExit();
        }
        
        protected abstract void OnInit();
        protected abstract void OnEnter();
        protected abstract void OnUpdate(float deltaTime);
        protected abstract void OnFixedUpdate(float deltaTime);
        protected abstract void OnLateUpdate(float deltaTime);
        protected abstract void OnExit();
    }
}