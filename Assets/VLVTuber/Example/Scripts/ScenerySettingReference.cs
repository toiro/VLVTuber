using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacantLot.VLUtil;
using VacantLot.VLVTuber;

/// <summary>
/// 背景シーンから VR シーンのオブジェクトを参照するための中継点
/// </summary>
public class ScenerySettingReference : ReferableMonoBehaviour
{
    [SerializeField]
    public CameraSettingManager Cameras;

    [SerializeField]
    public AvatarLightSettingManager AvatarLight;
}

