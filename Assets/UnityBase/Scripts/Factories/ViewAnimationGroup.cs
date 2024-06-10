using System;
using System.Collections.Generic;

namespace UnityBase.UI.ViewCore
{
    public class ViewAnimationGroup : IViewAnimationGroup
    {
        private readonly IDictionary<Type, object> _viewAnimBehaviours = new Dictionary<Type, object>();

        public void AddAnimationBehaviour(IViewBehaviour<IViewAnimation> viewBehaviour)
        {
            _viewAnimBehaviours[viewBehaviour.GetType()] = viewBehaviour;
        }
            
        public bool TryGetAnimationBehaviour<TViewBehaviour>(out TViewBehaviour viewBehaviour) where TViewBehaviour : class, IViewBehaviour<IViewAnimation>
        {
            var key = typeof(TViewBehaviour);

            if (_viewAnimBehaviours.TryGetValue(key, out var behaviour))
            {
                viewBehaviour = behaviour as TViewBehaviour;
                return true;
            }

            viewBehaviour = null;
            return false;
        }
    }
}