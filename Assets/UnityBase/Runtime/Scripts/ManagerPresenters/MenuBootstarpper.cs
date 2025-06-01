using System;
using System.Collections.Generic;
using UnityBase.BootService;
using UnityBase.Manager;
using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenter
{
    public class MenuBootstarpper : IInitializable, IDisposable
    {
        [Inject]
        private readonly IEnumerable<IMenuBootService> _menuBootServices;

        public void Initialize()
        {
            foreach (var gameplayBootService in _menuBootServices)
            {
                gameplayBootService?.Initialize();
            }
        }

        public void Dispose()
        {
            foreach (var gameplayBootService in _menuBootServices)
            {
                gameplayBootService?.Dispose();
            }
        }
    }
}