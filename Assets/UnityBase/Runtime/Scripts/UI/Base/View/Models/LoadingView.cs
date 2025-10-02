using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityBase.Runtime.Factories
{
    public class LoadingView : ILoadingView
    {
        private CanvasGroup _loadingCanvasGroup;
        
        private Image _sliderImage;
        
        private TextMeshProUGUI _sliderTxt;

        public ILoadingView Initialize(CanvasGroup loadingCanvasGroup, Image sliderImage, TextMeshProUGUI sliderTxt)
        {
            _loadingCanvasGroup = loadingCanvasGroup;
            _sliderImage = sliderImage;
            _sliderTxt = sliderTxt;
            return this;
        }

        public async UniTask SetActive(bool value)
        {
            await _loadingCanvasGroup.DOFade(value ? 1f : 0f, 0.1f)
                                     .SetEase(Ease.Linear)
                                     .SetDelay(value ? 0f: 1f)
                                     .AsyncWaitForCompletion();
        }

        public void SetFillAmount(float value) => _sliderImage.fillAmount = value;

        public void SetText(string value) => _sliderTxt.text = value;
        
        
        public void Dispose()
        {
            
        }
    }
    
    public interface ILoadingView : IView
    {
        public ILoadingView Initialize(CanvasGroup loadingCanvasGroup, Image sliderImage, TextMeshProUGUI sliderTxt);
        public UniTask SetActive(bool value);
        public void SetFillAmount(float value);
        public void SetText(string value);
    }
}