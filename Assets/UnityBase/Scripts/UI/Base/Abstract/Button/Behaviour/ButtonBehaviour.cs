namespace UnityBase.UI.ButtonCore
{
    public class ButtonBehaviour<TAct, TAnim> : IButtonBehaviour where TAct : IButtonAction where TAnim : IUIAnimation
    {
        private bool _isClicked;

        public bool IsClicked => _isClicked;
        public IButtonAction ButtonAction { get; }
        public IUIAnimation UIAnimation { get; }

        public ButtonBehaviour(TAct buttonAction, TAnim uiAnimation)
        {
            ButtonAction = buttonAction;
            UIAnimation = uiAnimation;
        }
        
        public void ConfigureAction(params object[] parameters)
        {
            var configureMethod = typeof(TAct).GetMethod("Configure");
            configureMethod?.Invoke(ButtonAction, parameters);
        }
        
        public void ConfigureAnimation(params object[] parameters)
        {
            var configureMethod = typeof(TAnim).GetMethod("Configure");
            configureMethod?.Invoke(ButtonAction, parameters);
        }
        
        public async void OnClick()
        {
            if (_isClicked) return;

            _isClicked = true;

            await UIAnimation.ClickAnimation();

            ButtonAction?.OnClick();

            _isClicked = false;
        }

        public async void OnPointerDown()
        {
            await UIAnimation.PointerDownAnimation();
            
            ButtonAction?.OnPointerDown();
        }

        public async void OnPointerUp()
        {
            await UIAnimation.PointerUpAnimation();

            ButtonAction?.OnPointerUp();
        }

        public void Dispose()
        {
            ButtonAction?.Dispose();
            
            UIAnimation?.Dispose();
        }
    }
}