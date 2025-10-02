using UnityBase.Runtime.Factories;
using UnityEngine;
using VContainer;

namespace UnityBase.UI
{
    public abstract class MonoView : MonoBehaviour, IMonoView
    {
        private IOwnerContextFactory _ownerContextFactory;
        public IOwnerContext OwnerContext { get; private set; }

        [Inject]
        private void Construct(IOwnerContextFactory ownerContextFactory)
        {
            _ownerContextFactory = ownerContextFactory;
            
            OwnerContext = _ownerContextFactory.GetContext(this);
            
            Initialize();
        }
        
        protected abstract void Initialize();
        protected virtual void OnDestroy() => _ownerContextFactory?.Release(this);
    }
    
    public interface IMonoView
    {
        public IOwnerContext OwnerContext { get; }
    }
}