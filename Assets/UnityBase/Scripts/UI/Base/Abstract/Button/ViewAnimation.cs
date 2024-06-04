using Cysharp.Threading.Tasks;

namespace UnityBase.UI.ButtonCore
{
    public abstract class ViewAnimation : IViewAnimation
    {
        public abstract UniTask Show();
        public abstract UniTask Hide();
        public abstract void Dispose();
    }

    public class InOutView : ViewAnimation
    {
        public override UniTask Show()
        {
            return default;
        }

        public override UniTask Hide()
        {
            return default;
        }

        public override void Dispose()
        {
            
        }
    }
}