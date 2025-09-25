
namespace UnityBase.Runtime.Factories
{
    public interface IModel
    {
        public void Dispose();
    }

    public interface IView
    {
        public void Dispose();
    }
    
    public interface IAnimation
    {
        public void Dispose();
    }
    
    public interface IAction
    {
        public void Dispose();
    }
    
    public interface IPresenter
    {
        public void Dispose();
    }

    public interface ISaveData
    {
        public void Save();
    }
}