using System;   // Require for 'Console'

namespace BennyBroseph
{
    static class ContextualDebug
    {
        static public void DebugMessage(object a_Message)
        {
#if (!UNITY_EDITOR && DEBUG)
            Console.WriteLine(a_Message);
#elif UNITY_EDITOR
            Debug.Log(a_Message);
#endif
        }
        static public void DebugWarning(object a_Message)
        {
#if (!UNITY_EDITOR && DEBUG)
            Console.WriteLine(a_Message + "...");
#elif UNITY_EDITOR
            Debug.LogWarning(a_Message + "...");
#endif
        }
        static public void DebugError(object a_Message)
        {
#if (!UNITY_EDITOR && DEBUG)
            Console.WriteLine("ERROR: " + a_Message + "!");
#elif UNITY_EDITOR
            Debug.LogError(a_Message + "!");
#endif
        }
    }
}
