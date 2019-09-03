using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using VacantLot.VLUtil;

namespace VacantLot.VLVTuber
{
    /// <summary>
    /// ViveInputUtility で表示されるコントローラーにレイヤーを設定する
    /// </summary>
    public class DeviceModelLayerController : MonoBehaviour
    {
        [Layer]
        public int Layer = 0;

        private RenderModelHook[] targets;

        // Use this for initialization
        void Start()
        {
            targets = FindObjectsOfType<RenderModelHook>();
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var t in targets)
            {
                t.transform.Decendants().Where(_ => _.gameObject.layer != Layer).ToList().ForEach(_ => _.gameObject.layer = Layer);
            }
        }
    }
}
