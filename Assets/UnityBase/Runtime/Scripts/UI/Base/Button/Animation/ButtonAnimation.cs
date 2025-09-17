using Cysharp.Threading.Tasks;
using UnityBase.Runtime.Behaviours;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ButtonAnimation : IButtonAnimation
    {
        public abstract UniTask Click();
        public abstract UniTask PointerDown();
        public abstract UniTask PointerUp();
        public abstract void Dispose();
    }
    
    public interface IButtonAnimation : IAnimation
    {
        public UniTask Click();
        public UniTask PointerDown();
        public UniTask PointerUp();
    }
}