using UnityBase.UI.ButtonCore;

public class TermsOfUseButton : ButtonBehaviour
{
    protected override void OnClick()
    {
        OnClickTermsOfUse();
    }

    private void OnClickTermsOfUse()
    {
        //Application.OpenURL("https://google.com");
    }
}