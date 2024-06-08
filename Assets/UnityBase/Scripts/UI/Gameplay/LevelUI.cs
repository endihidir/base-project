using TMPro;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelTxt;
    
    [Inject] 
    private readonly ILevelManager _levelManager;
    
    private void Awake()
    {
        _levelTxt.text = "LEVEL " + _levelManager.LevelText.ToString("0");
    }
}
