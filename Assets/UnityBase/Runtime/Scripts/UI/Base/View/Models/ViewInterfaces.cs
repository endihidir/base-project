
using System;

namespace UnityBase.Runtime.Factories
{
    public interface IModel : IDisposable
    {
       
    }

    public interface IView : IDisposable
    {
      
    }
    
    public interface IAnimation : IDisposable
    {
      
    }
    
    public interface IAction : IDisposable
    {
       
    }
    
    public interface IPresenter : IDisposable
    {
       
    }

    public interface ISaveData
    {
        public void Save();
    }

    public interface IUpdater
    {
        public void Update();
    }
}