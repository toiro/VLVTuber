using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VacantLot.VLVTuber
{
    public class AvatarLightSettingManager : MonoBehaviour
    {
        Light Target => GetComponent<Light>();

        public void ApplyLightColor(Color color) => Target.color = color;
    }
}
