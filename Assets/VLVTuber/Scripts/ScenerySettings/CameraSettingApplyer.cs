using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VacantLot.VLUtil;

namespace VacantLot.VLVTuber
{
    public class CameraSettingApplyer : MonoBehaviour
    {
        [SerializeField] Camera MainCamera;
        [SerializeField] Camera MirrorCamera;
        [SerializeField] Camera Sub1Camera;
        [SerializeField] Camera Sub2Camera;

        ScenerySettingReference Reference => ReferableMonoBehaviour.GetFromActiveScene<ScenerySettingReference>();
        // Use this for initialization
        void Start()
        {
            if (Reference)
            {
                if (MainCamera)
                {
                    Reference.Cameras.ApplyMainCamera(MainCamera);
                }

                if (MirrorCamera)
                {
                    Reference.Cameras.ApplyMirrorCamera(MirrorCamera);
                }

                if (Sub1Camera)
                {
                    Reference.Cameras.ApplySub1Camera(Sub1Camera);
                }

                if (Sub2Camera)
                {
                    Reference.Cameras.ApplySub2Camera(Sub2Camera);
                }

                transform.GetComponentsInChildren<Camera>().ToList().ForEach(_ => _.enabled = false);
            }
        }
    }
}
