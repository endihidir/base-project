using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;


public class StarAnimationSequence
{
    private readonly List<IStarUI> _starAnims = new();
    
    public event Action OnSequenceComplete;
    public bool IsInProgress => _starAnims.Count > 0;

    public void Add(IStarUI starUI) => _starAnims.Add(starUI);

    public async void StartMovements(float duration, Ease ease, Action<IStarUI> onStepComplete)
    {
        await PerformMove((starAnim) => starAnim.Move(duration, ease, () => onStepComplete?.Invoke(starAnim)).AsyncWaitForCompletion());
    }

    public async void StartMovements(float duration, float curvePower, CurveSide curveSide, Action<IStarUI> onStepComplete)
    {
        await PerformMove((starAnim) => starAnim.Move(duration, curvePower, curveSide, () => onStepComplete?.Invoke(starAnim)).AsyncWaitForCompletion());
    }
    
    private async Task PerformMove(Func<IStarUI, Task> moveFunction)
    {
        var tasks = new List<Task>();

        for (var i = 0; i < _starAnims.Count; i++)
        {
            var starAnim = _starAnims[i];
            var task = moveFunction(starAnim);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        OnSequenceComplete?.Invoke();
        
        Dispose();
    }

    private void Dispose()
    {
        _starAnims.Clear();
        
        OnSequenceComplete = null;
    }
}


public interface IStarUI
{
    Tween Move(float duration, Ease ease, Action onComplete = null);
    Tween Move(float duration, float curvePower, CurveSide curveSide, Action onComplete = null);
}

public enum CurveSide
{
    
}
