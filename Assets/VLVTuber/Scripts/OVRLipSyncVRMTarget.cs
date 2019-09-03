using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace VacantLot.VLVTuber
{
    public class OVRLipSyncVRMTarget : MonoBehaviour
    {
        public VRMBlendShapeProxy BlendShapeProxy;

        // smoothing amount
        [Range(1, 100)]
        [Tooltip("Smoothing of 1 will yield only the current predicted viseme, 100 will yield an extremely smooth viseme response.")]
        public int smoothAmount = 70;

        // Look for a lip-sync Context (should be set at the same level as this component)
        private OVRLipSyncContextBase lipsyncContext = null;

        private readonly static Dictionary<BlendShapePreset, int> lipsyncClips
            = new Dictionary<BlendShapePreset, int>()
            {
                { BlendShapePreset.A, 10 },
                { BlendShapePreset.E, 11 },
                { BlendShapePreset.I, 12 },
                { BlendShapePreset.O, 13 },
                { BlendShapePreset.U, 14 }
            };

        // Use this for initialization
        void Start()
        {
            // make sure there is a phoneme context assigned to this object
            lipsyncContext = GetComponent<OVRLipSyncContextBase>();
            if (lipsyncContext == null)
            {
                Debug.LogError("OVRLipSyncVRMTarget.Start Error: " +
                    "No OVRLipSyncContext component on this object!");
            }
            else
            {
                // Send smoothing amount to context
                lipsyncContext.Smoothing = smoothAmount;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if ((lipsyncContext != null) && (BlendShapeProxy != null))
            {
                // get the current viseme frame
                OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();
                if (frame != null)
                {
                    SetVisemeToVRMTarget(frame);
                }

                // Update smoothing value
                if (smoothAmount != lipsyncContext.Smoothing)
                {
                    lipsyncContext.Smoothing = smoothAmount;
                }
            }
        }

        /// <summary>
        /// Sets the viseme to VRM target.
        /// </summary>
        void SetVisemeToVRMTarget(OVRLipSync.Frame frame)
        {
            var values = lipsyncClips.Select(_ => new KeyValuePair<BlendShapeKey, float>(new BlendShapeKey(_.Key), frame.Visemes[_.Value]));
            BlendShapeProxy.SetValues(values);
        }
    }
}
