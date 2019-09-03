using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kayac;

namespace VacantLot.VLUtil
{
    public class KeyInputHandlerUI : MonoBehaviour
    {
        [SerializeField]
        OverlayUIManager UIManager;

        KeyInputHandler Handler => GetComponent<KeyInputHandler>();

        DebugUiMenu Menu { get; set; }

        void Start()
        {
            Menu = CreateMenu(Handler);
            UIManager.UI.Add(Menu, 5, 5);
        }

        private static DebugUiMenu CreateMenu(KeyInputHandler handler)
        {
            var menu = new DebugUiMenu(100, 80, direction: DebugUi.Direction.Down);
            foreach (var g in handler.Definition)
            {
                var sub = new DebugUiSubMenu(g.Name, 100, 80);
                foreach (var a in g.Assigns)
                {
                    sub.AddItem(a.Name + "\n[" + a.Key.ToString() + "]", () => { if (g.IsActive) a.OnKeyDown.Invoke(); });
                }
                menu.AddSubMenu(sub, DebugUi.Direction.Right);
            }
            return menu;
        }
    }
}
