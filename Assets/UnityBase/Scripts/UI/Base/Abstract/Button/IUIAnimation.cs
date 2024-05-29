using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public interface IUIAnimation
    {
        public UniTask OpenAnimation();
        public UniTask CloseAnimation();
        public UniTask ClickAnimation();
        public UniTask PointerDownAnimation();
        public UniTask PointerUpAnimation();
        public void Dispose();
    }
}