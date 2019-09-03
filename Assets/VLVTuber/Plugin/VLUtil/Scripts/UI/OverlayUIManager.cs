using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kayac;

namespace VacantLot.VLUtil
{

    public class OverlayUIManager : MonoBehaviour
    {
        //private const int UILayer = 10;

        [SerializeField]
        Camera TargetCamera;
        [SerializeField]
        [Tooltip("Set Miura/DebugPrimitiveText")]
        Shader TextShader;
        [SerializeField]
        [Tooltip("Set Miura/DebugPrimitiveTextured")]
        Shader TextureShader;
        [SerializeField][Layer]
        int UILayer;

        KeyInputHandler Handler => GetComponent<KeyInputHandler>();

        DebugUiManager _UI;
        public DebugUiManager UI {
            get {
                if (_UI == null) _UI = CreateUI(TargetCamera, TextShader, TextureShader);
                return _UI;
            }
        }

        // Update is called once per frame
        void Update()
        {
             UI.ManualUpdate(Time.deltaTime);
        }

        private DebugUiManager CreateUI(Camera camera, Shader textShader, Shader textureShader)
        {
            //camera.clearFlags = CameraClearFlags.Depth;
            //camera.depth = 100;
            //camera.targetDisplay = 0;
            //camera.stereoTargetEye = StereoTargetEyeMask.None;

            var ui = DebugUiManager.Create(
                camera,
                textShader,
                textureShader,
                Resources.GetBuiltinResource<Font>("Arial.ttf"),
                1920, 1080,
                100f, 8192
                );

            //Set up Layer
            camera.cullingMask = 1 << UILayer;
            foreach (var t in camera.gameObject.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = UILayer;
            }

            return ui;
        }
    }
}
