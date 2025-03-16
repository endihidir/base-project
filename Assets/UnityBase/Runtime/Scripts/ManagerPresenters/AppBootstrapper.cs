using System;
using System.Collections.Generic;
using UnityBase.BootService;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class AppBootstrapper : IInitializable, IDisposable
    {
        [Inject] 
        private readonly IEnumerable<IAppBootService> _appBootServices;

        public void Initialize()
        {
            foreach (var appBootService in _appBootServices)
            {
                appBootService?.Initialize();
            }
        }

        public void Dispose()
        {
            foreach (var appBootService in _appBootServices)
            {
                appBootService?.Dispose();
            }
        }
    }
}