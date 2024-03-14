using DG.Tweening;
using Sirenix.OdinInspector;
using UnityBase.Extensions;
using UnityBase.Service;
using UnityBase.TutorialCore;
using UnityBase.TutorialCore.TutorialAction;
using UnityEngine;
using VContainer;

public class TutorialTest : MonoBehaviour
{
    [Inject]
    private readonly ITutorialDataService _tutorialDataService;
    
    [Inject]
    private readonly ITutorialMaskDataService _tutorialMaskDataService;
    
    [Button]
    public void HandDrawTest()
    {
        var tut = _tutorialDataService.GetTutorial<HandTutorial>(PositionSpace.ScreenSpace);

        var pos1 = new Vector3(Screen.width * 0.2f, Screen.height * 0.8f);
        var pos2 = new Vector3(Screen.width * 0.75f, Screen.height * 0.5f);
        var pos3 = new Vector3(Screen.width * 0.75f, Screen.height * 0.25f);
        var pos4 = new Vector3(Screen.width * 0.2f, Screen.height * 0.25f);
        var poses = new[] { pos1, pos2, pos3, pos4 };
        
        var maskData = new MaskUIData(PositionSpace.ScreenSpace, Vector2.one * 150f);
        
        _tutorialMaskDataService.GetMasks(poses, maskData);
        
        var act = tut.GetAction<DrawAction>(0.85f);
        
        act.Draw(poses, 1.25f, PathType.CatmullRom, true);
    }
    
    [Button]
    public void HandMoveTest()
    {
        var tut = _tutorialDataService.GetTutorial<HandTutorial>(PositionSpace.ScreenSpace);
        
        var pos1 = new Vector3(Screen.width * 0.6f, Screen.height * 0.5f);
        var pos2 = new Vector3(Screen.width * 0.4f, Screen.height * 0.75f);
        var maskData = new MaskUIData(PositionSpace.ScreenSpace, Vector2.one * 150f);
        
        _tutorialMaskDataService.GetMasks(new []{pos1, pos2}, maskData);

        var act = tut.GetAction<MoveAction>(0.85f);
        
        act.Move(pos1, pos2, 1f, 0.1f).AlignDirection();
    }
    
    [Button]
    public void HandSlideTest()
    {
        var tut = _tutorialDataService.GetTutorial<HandTutorial>(PositionSpace.ScreenSpace);
        
        var pos1 = new Vector3(Screen.width * 0.2f, Screen.height * 0.5f);
        var pos2 = new Vector3(Screen.width * 0.75f, Screen.height * 0.75f);
        var maskData = new MaskUIData(PositionSpace.ScreenSpace, Vector2.one * 150f);
        
        _tutorialMaskDataService.GetMasks(new []{pos1, pos2}, maskData);

        var act = tut.GetAction<SlideAction>(0.85f);
        act.Slide(pos1, pos2, 1f).AlignDirection();
    }
    
    [Button]
    public void HandClickTest()
    {
        var tut = _tutorialDataService.GetTutorial<HandTutorial>(PositionSpace.ScreenSpace);
        
        var pos1 = new Vector3(Screen.width * 0.5f, Screen.height * 0.8f);
        var maskData = new MaskUIData(PositionSpace.ScreenSpace, Vector2.one * 150f);
        
        _tutorialMaskDataService.GetMask(pos1, maskData);
        
        var act = tut.GetAction<ClickAction>(0.85f);
        
        act.Click(pos1, 0.1f);
    }
    
    [Button]
    public void HandKill()
    {
        _tutorialDataService.HideAllTutorialOfType<HandTutorial>(0.5f);
        _tutorialMaskDataService.HideAllMasks(0.5f);
    }
    
    [Button]
    public void TextTypeWriteTest()
    {
        string text = "sdfsdfsdjkfsdfsdjkfsdjfjkdfjkskjfjsfjsfjsjkfjksfjksjkfjksfjsfjkkjsfjksfjksjkfjkskfjsjk";
        var tut = _tutorialDataService.GetTutorial<TextTutorial>(PositionSpace.ScreenSpace);
        var act = tut.Action<TypeWriterAction>(new Vector2(300, 300));
        var pos2 = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        act.TypeWriter(text, pos2, 0.05f);
    }
    
    [Button]
    public void TextMoveInOutTest()
    {
        string text = "sdfsdfsdjkfsdfsdjkfsdjfjkdfjkskjfjsfjsfjsjkfjksfjksjkfjksfjsfjkkjsfjksfjksjkfjkskfjsjk";
        var tut = _tutorialDataService.GetTutorial<TextTutorial>(PositionSpace.ScreenSpace);
        var act = tut.Action<MoveInOutAction>(new Vector2(300, 300));
        
        var pos1 = new Vector3(-Screen.width * 0.5f, Screen.height * 0.5f);
        var pos2 = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        var pos3 = new Vector3(Screen.width * 2f, Screen.height * 0.5f);
        
        act.MoveInOut(pos1,pos2, pos3, 1f, 3f).SetText(text);
    }
    
    [Button]
    public void TextScaleUpTest()
    {
        string text = "sdfsdfsdjkfsdfsdjkfsdjfjkdfjkskjfjsfjsfjsjkfjksfjksjkfjksfjsfjkkjsfjksfjksjkfjkskfjsjk";
        var tut = _tutorialDataService.GetTutorial<TextTutorial>(PositionSpace.ScreenSpace);
        var act = tut.Action<ScaleAction>(new Vector2(300, 300));
        var pos2 = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        act.ScaleUp(pos2, 1f, 0f).SetText(text);
    }
    
    [Button]
    public void TextKill()
    {
        _tutorialDataService.HideAllTutorialOfType<TextTutorial>(0.5f);
    }
}