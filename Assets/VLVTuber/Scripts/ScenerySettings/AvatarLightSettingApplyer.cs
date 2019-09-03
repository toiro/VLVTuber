using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacantLot.VLUtil;

namespace VacantLot.VLVTuber
{
    public class AvatarLightSettingApplyer : MonoBehaviour
    {
        public bool SetDisableTargetLight = false;
        Light Target => GetComponent<Light>();


        ScenerySettingReference Reference => ReferableMonoBehaviour.GetFromActiveScene<ScenerySettingReference>();

        // Use this for initialization
        void Start()
        {
            if (Reference)
            {
                Reference.AvatarLight.ApplyLightColor(Target.color);
                if(SetDisableTargetLight)
                {
                    Target.enabled = false;
                }
            }
        }

    }
}
