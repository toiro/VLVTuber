using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VacantLot.VLUtil
{
    public class KeyInputHandler : MonoBehaviour
    {
        public KeyAssignGroup[] Definition;

        // Update is called once per frame
        void Update()
        {
            foreach (var g in Definition)
            {
                if (!g.IsActive) continue;

                foreach (var a in g.Assigns)
                {
                    if (Input.GetKeyDown(a.Key))
                    {
                        Debug.Log(g.Name + " - " + a.Name);
                        a.OnKeyDown.Invoke();
                    }
                }
            }
        }

        public void DebugLogTrigger(string msg)
        {
            Debug.Log("Trigger: " + msg);
        }

        [System.Serializable]
        public class KeyAssignGroup
        {
            public string Name;
            public KeyAssign[] Assigns;
            public bool IsActive;
        }

        [System.Serializable]
        public class KeyAssign
        {
            public string Name;
            public KeyCode Key;
            public UnityEvent OnKeyDown;
        }
    }
}
