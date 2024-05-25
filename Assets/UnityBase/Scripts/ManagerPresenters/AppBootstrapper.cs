using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityBase.Service;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class AppBootstrapper : IInitializable, IDisposable
    {
        [Inject] 
        private readonly IEnumerable<IAppBootService> _appBootServices;

        public void Initialize() => _appBootServices.ForEach(x => x.Initialize());
        public void Dispose() => _appBootServices.ForEach(x => x.Dispose());
    }
}