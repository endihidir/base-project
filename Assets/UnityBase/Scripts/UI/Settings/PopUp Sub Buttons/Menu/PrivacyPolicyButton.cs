using UnityBase.UI.ButtonCore;

public class PrivacyPolicyButton : ButtonBehaviour
{
    protected override void OnClick()
    {
        OnClickPrivacyPolicy();
    }

    private void OnClickPrivacyPolicy()
    {
       // Application.OpenURL("https://google.com");
    }
}