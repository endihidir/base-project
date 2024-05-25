namespace UnityBase.Service
{
    public interface ITaskManagementService
    { 
        public void CreateTask(string key);
        public bool IsTaskCompleted(string key);
        public void CompleteTask(string key);
    }
}