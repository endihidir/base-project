using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public interface IViewAnimation
    {
        public UniTask Show();
        public UniTask Hide();
        public void Dispose();
    }
}