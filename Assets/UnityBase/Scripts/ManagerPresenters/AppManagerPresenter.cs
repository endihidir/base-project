using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityBase.Service;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class AppManagerPresenter : IInitializable, IStartable, IDisposable
    {
        [Inject] 
        private readonly IEnumerable<IAppPresenterDataService> _appPresenterDataServices;

        public void Initialize() => _appPresenterDataServices.ForEach(x => x.Initialize());
        public void Start() => _appPresenterDataServices.ForEach(x => x.Start());
        public void Dispose() => _appPresenterDataServices.ForEach(x => x.Dispose());
    }
}