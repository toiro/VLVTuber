using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VacantLot.VLVTuber
{
    public class CameraSettingManager : MonoBehaviour
    {
        [SerializeField] Camera MainCamera;
        [SerializeField] Camera MirrorCamera;
        [SerializeField] Camera Sub1Camera;
        [SerializeField] Camera Sub2Camera;

        public void ApplyMainCamera(Camera from) => ApplySetting(MainCamera, from);
        public void ApplyMirrorCamera(Camera from) => ApplySetting(MirrorCamera, from);
        public void ApplySub1Camera(Camera from) => ApplySetting(Sub1Camera, from);
        public void ApplySub2Camera(Camera from) => ApplySetting(Sub2Camera, from);

        void ApplySetting(Camera target, Camera from)
        {
            if (!target || !from) return;
            // targetDisplay だけは維持する
            var targetDisplay = target.targetDisplay;
            target.CopyFrom(from);
            target.targetDisplay = targetDisplay;
            from.enabled = false;
        }
    }
}
