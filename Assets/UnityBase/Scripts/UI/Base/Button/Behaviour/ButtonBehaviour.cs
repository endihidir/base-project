using UnityBase.Extensions;

namespace UnityBase.UI.ButtonCore
{
    public class ButtonBehaviour<TAct, TAnim> : IButtonBehaviour<TAct, TAnim> where TAct : IButtonAction where TAnim : IButtonAnimation
    {
        private bool _isClicked;
        public bool IsClicked => _isClicked;
        public TAct ButtonAction { get; }
        public TAnim ButtonAnimation { get; }

        public ButtonBehaviour(TAct buttonAction, TAnim uiAnimation)
        {
            ButtonAction = buttonAction;
            ButtonAnimation = uiAnimation;
        }
        
        public void ConfigureAction(params object[] parameters) => ButtonAction.ConfigureMethod("Configure", parameters);
        public void ConfigureAnimation(params object[] parameters) => ButtonAnimation.ConfigureMethod("Configure", parameters);

        public virtual async void OnClick()
        {
            if(ButtonAnimation is null) return;
            
            if (_isClicked) return;

            _isClicked = true;

            await ButtonAnimation.Click();

            ButtonAction?.OnClick();

            _isClicked = false;
        }

        public virtual async void OnPointerDown()
        {
            if(ButtonAnimation is null) return;
            
            await ButtonAnimation.PointerDown();
            
            ButtonAction?.OnPointerDown();
        }

        public virtual async void OnPointerUp()
        {
            if(ButtonAnimation is null) return;
            
            await ButtonAnimation.PointerUp();

            ButtonAction?.OnPointerUp();
        }

        public virtual void Dispose()
        {
            ButtonAction?.Dispose();
            
            ButtonAnimation?.Dispose();
        }
    }
}