using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacantLot.VLVTuber;
using VacantLot.VLUtil;
using VRM;

public class AliciaLoader : MonoBehaviour {
    [SerializeField]
    GameObject AliciaAvatar;
    [SerializeField]
    BlendShapeAvatar AliciaBlendShapeAvatar;
    [SerializeField]
    [Layer]
    int AvatarLayer;


	// Use this for initialization
	IEnumerator Start () {
        // Instantiate Avatar and Modify
        var alicia = GameObject.Instantiate(AliciaAvatar);
        alicia.transform.parent = this.transform;

        foreach (var t in alicia.transform.Decendants())
        {
            t.gameObject.layer = AvatarLayer;
        }

        var moof = (MonoBehaviour) alicia.GetComponent("Alicia_moof");
        if (moof) moof.enabled = false;

        var camAngles = (MonoBehaviour)alicia.GetComponent("camAngles");
        if (camAngles) camAngles.enabled = false;

        var blendShapeProxy = alicia.AddComponent<VRMBlendShapeProxy>();
        blendShapeProxy.BlendShapeAvatar = AliciaBlendShapeAvatar;

        yield return null;

        // Set Avatar to System
        var blendShapeClipChanger = GetComponentInChildren<BlendShapeClipChanger>();
        blendShapeClipChanger.VRM = blendShapeProxy;

        var vrikProvider = GetComponentInChildren<VRIKProvider>();
        vrikProvider.Avatar = alicia;
        yield return vrikProvider.SetUp();

        var handPoseManager = GetComponentInChildren<HandPoseManager>();
        handPoseManager.LeftDefinition.RootBone = vrikProvider.References.leftHand;
        handPoseManager.RightDefinition.RootBone = vrikProvider.References.rightHand;

        var vrmLipSync = GetComponentInChildren<OVRLipSyncVRMTarget>();
        vrmLipSync.BlendShapeProxy = blendShapeProxy;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
