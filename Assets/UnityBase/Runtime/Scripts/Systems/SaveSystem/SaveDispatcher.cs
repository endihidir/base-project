using UnityEngine;

namespace UnityBase.SaveSystem
{
    public class SaveDispatcher : MonoBehaviour
    {
        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                QuitInvokeUtil.InvokeAll();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                QuitInvokeUtil.InvokeAll();
            }
        }

        void OnApplicationQuit()
        {
            QuitInvokeUtil.InvokeAll();
        }
    }
}