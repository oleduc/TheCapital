using TheCapital.Dialogs;

namespace TheCapital
{
    public class Hooks
    {
        private static Hooks _instance;

        public static Hooks Instance => _instance ?? (_instance = new Hooks());

        public void OnGUIHook()
        {
            DevToolOpener.Instance.AddDebugButton();
        }
    }
}