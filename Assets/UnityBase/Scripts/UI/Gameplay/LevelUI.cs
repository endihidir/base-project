using TMPro;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _levelTxt;
    
    [Inject] 
    private readonly ILevelManagementService _levelManagementService;
    
    private void Awake()
    {
        _levelTxt.text = "LEVEL " + _levelManagementService.LevelText.ToString("0");
    }
}
