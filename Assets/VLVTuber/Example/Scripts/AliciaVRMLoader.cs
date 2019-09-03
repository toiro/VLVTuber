using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacantLot.VLVTuber;
using VacantLot.VLUtil;
using VRM;

public class AliciaVRMLoader : MonoBehaviour {
    [SerializeField]
    [Layer]
    int AvatarLayer;

    // Use this for initialization
    void Start () {
        var path = Application.streamingAssetsPath + "/" + "AliciaSolid.vrm";
        var context = new VRMImporterContext();

        context.Parse(path);
        context.LoadAsync(() =>
        {
            var avatar = context.Root;
            avatar.transform.parent = this.transform;

            foreach (var t in avatar.transform.Decendants())
            {
                t.gameObject.layer = AvatarLayer;
            }

            // Set Avatar to System
            var blendShapeClipChanger = GetComponentInChildren<BlendShapeClipChanger>();
            blendShapeClipChanger.VRM = avatar.GetComponent<VRMBlendShapeProxy>();

            var vrmLipSync = GetComponentInChildren<OVRLipSyncVRMTarget>();
            vrmLipSync.BlendShapeProxy = avatar.GetComponent<VRMBlendShapeProxy>();

            var vrikProvider = GetComponentInChildren<VRIKProvider>();
            vrikProvider.Avatar = avatar;
            vrikProvider.CallSetUp();

            context.ShowMeshes();
        });
    }
}
