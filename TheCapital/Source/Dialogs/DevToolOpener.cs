using System;
using UnityEngine;
using Verse;

namespace TheCapital.Dialogs
{
    [StaticConstructorOnStartup]
    public class DevToolOpener {
        public static readonly Texture2D Info = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);

        private static DevToolOpener _instance;
        public static DevToolOpener Instance => _instance ?? (_instance = new DevToolOpener());
        
        public void AddDebugButton()
        {
            if (!Prefs.DevMode)
                return;
            Vector2 vector2 = new Vector2(UI.screenWidth * 0.5f - 28f, 3f);
            int num = 1;
            float height = 25f;
            Find.WindowStack.ImmediateWindow(
                1593759362,
                new Rect(vector2.x, vector2.y, (float) (num * 28.0 - 4.0 + 1.0), height).Rounded(),
                WindowLayer.GameUI,
                () => DrawButtons(), false, false, 0.0f);
        }
        
        private void DrawButtons()
        {
            WidgetRow widgetRow = new WidgetRow();
            if (widgetRow.ButtonIcon(Info, "Open the debug log."))
                ToggleLogWindow();
        }
        
        private void ToggleLogWindow()
        {
            if (Find.WindowStack.TryRemove(typeof (Dialog_DebugTool)))
                return;
            Find.WindowStack.Add(new Dialog_DebugTool());
        }
    }
}