using System;
using UnityEngine;

namespace UnityBase.StateMachineCore
{
    public interface ITransition
    {
        public IState From { get; }
        public IState To { get; }
        public bool CheckTrigger();
    }

    public class Transition : ITransition
    {
        public IState From { get; }
        public IState To { get; }
        private event Func<bool> Condition;

        public Transition(IState from, IState to, Func<bool> onSuccess)
        {
            From = from;
            To = to;
            Condition = onSuccess;
            
            if (from is ITreeState fromTree && to is ITreeState toTree && fromTree.GetRootState() != toTree.GetRootState())
            {
                Debug.LogError($"Invalid transition: '{fromTree.StateID}' and '{toTree.StateID}' are not in the same hierarchy.");
            }
        }
        
        public bool CheckTrigger()
        {
            if (Condition?.Invoke() == false) return false;
            
            if (!From.IsActive) return false;

            if (From is ITreeState fromTree && To is ITreeState toTree)
            {
                if (CanTransition(fromTree, toTree))
                {
                    fromTree.Exit();

                    toTree.Enter();

                    return true;
                }

                Debug.LogError($"Cannot force transition from '{fromTree.StateID}' to '{toTree.StateID}': different root states.");

                return false;
            }

            From.Exit();

            To.Enter();

            return true;
        }

        private bool CanTransition(ITreeState from, ITreeState to) => (from.GetRootState() == to.GetRootState()) || from.IsRootState;
    }
}