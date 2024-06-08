using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ViewCore
{
    public interface IViewAnimation
    {
        public UniTask Show();
        public UniTask Hide();
        public void Dispose();
    }
}