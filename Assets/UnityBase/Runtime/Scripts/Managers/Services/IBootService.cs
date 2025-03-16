namespace UnityBase.BootService
{
    public interface IBootService
    {
         public void Initialize();
         public void Dispose();
    }
    
    public interface IAppBootService : IBootService
    {
       
    }

    public interface IMenuBootService : IBootService
    {
        
    }
    
    public interface IGameplayBootService : IBootService
    {
       
    }

}