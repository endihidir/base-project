using TMPro;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelTxt;
    
    [Inject] 
    private readonly ILevelDataService _levelDataService;
    
    private void Awake()
    {
        _levelTxt.text = "LEVEL " + _levelDataService.LevelText.ToString("0");
    }
}
