using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ViewAnimation : IViewAnimation
    {
        public abstract UniTask Show();
        public abstract UniTask Hide();
        public abstract void Dispose();
    }
}