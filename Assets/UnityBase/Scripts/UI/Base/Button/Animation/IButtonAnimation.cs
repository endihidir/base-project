using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public interface IButtonAnimation
    {
        public UniTask Click();
        public UniTask PointerDown();
        public UniTask PointerUp();
        public void Dispose();
    }
}