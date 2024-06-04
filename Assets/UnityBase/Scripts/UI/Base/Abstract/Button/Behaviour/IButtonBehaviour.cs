namespace UnityBase.UI.ButtonCore
{
    public interface IButtonBehaviour
    {
        public bool IsClicked { get; }
        public void OnClick();
        public void OnPointerDown();
        public void OnPointerUp();
        public void Dispose();
    }
    public interface IButtonBehaviour<out TAct, out TAnim> : IButtonBehaviour where TAct : IButtonAction where TAnim : IButtonAnimation
    {
        public TAct ButtonAction { get; }
        public TAnim ButtonAnimation { get; }
        public void ConfigureAction(params object[] parameters);
        public void ConfigureAnimation(params object[] parameters);
    }
}