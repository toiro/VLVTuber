using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using VacantLot.VLUtil.PolarCoordinates;
using VacantLot.VLVTuber;
using System;

public class ViveInputHandler : MonoBehaviour
{
    public HandPoseManager HandPose;
    [Tooltip("パッドの認識分割数。中央を0として、前方から外側周りに番号を振る。")]
    public int PadPartitionCount = 4;
    public float PadCenterSize = 0.6f;

    // Update is called once per frame
    void Update()
    {
        PadTouchToHandPose();
        TriggerToClench();
    }

    void PadTouchToHandPose()
    {
        if (!HandPose) return;
        TouchPadToHandPose(HandPoseManager.LR.L);
        TouchPadToHandPose(HandPoseManager.LR.R);
    }

    void TouchPadToHandPose(HandPoseManager.LR lr)
    {
        var index = PadToIndex(lr, PadPartitionCount, PadCenterSize);

        if (index == -1) HandPose.Clear(lr);
        else HandPose.Apply(lr, index);
    }

    void TriggerToClench()
    {
        if (!HandPose) return;
        HandPose.LeftClench = Mathf.Clamp01(ViveInput.GetTriggerValueEx(HandRole.LeftHand));
        HandPose.RightClench = Mathf.Clamp01(ViveInput.GetTriggerValueEx(HandRole.RightHand));
    }

    static HandRole LRToRole(HandPoseManager.LR lr)
    {
        switch (lr)
        {
            case HandPoseManager.LR.L:
                return HandRole.LeftHand;
            case HandPoseManager.LR.R:
                return HandRole.RightHand;
        }
        throw new Exception("bug");
    }

    static int PadToIndex(HandPoseManager.LR lr, int partitionCount, float padCenterSize)
    {
        if (partitionCount <= 0) return -1;

        var handRole = LRToRole(lr);

        if (!ViveInput.GetPressEx(handRole, ControllerButton.PadTouch)) return -1;

        if (partitionCount == 1) return 0;

        var axis = ViveInput.GetPadAxisEx(handRole);

        // パッド座標を極座標に変換
        float unitAngle = 360f / (partitionCount - 1);
        // 前方から外側周り
        var isClockWise = (lr == HandPoseManager.LR.R);
        var angleOffset = unitAngle + (isClockWise ? 90 : -90);
        var polar = new PolarCircular(axis, new PolarContext2(Vector2.zero, angleOffset, isClockWise)).NormalizeAngle();

        //Debug.Log("PadAxis: " + axis);
        //Debug.LogFormat("PadAngle: {0}, {1}", polar.theta, (int)(Mathf.Floor(polar.NormalizeAngle().theta / unitAngle) + 1));

        if (polar.r < padCenterSize) return 0;
        else return (int)Mathf.Floor(polar.theta / unitAngle) + 1;
    }
}
