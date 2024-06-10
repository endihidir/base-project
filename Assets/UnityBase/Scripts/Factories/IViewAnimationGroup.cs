namespace UnityBase.UI.ViewCore
{
    public interface IViewAnimationGroup
    {
        public void AddAnimationBehaviour(IViewBehaviour<IViewAnimation> viewBehaviour);
        public bool TryGetAnimationBehaviour<TViewBehaviour>(out TViewBehaviour viewBehaviour) where TViewBehaviour : class, IViewBehaviour<IViewAnimation>;
    }
}