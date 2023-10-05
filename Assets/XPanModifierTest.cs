using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPanModifierTest : MonoBehaviour
{
    public LightpadBlockXYZReceiver xyzReceiver;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        xyzReceiver.xNormalisedValueChanged += HandleXChanged;
    }

    private void HandleXChanged(float z)
    {
        if (audioSource != null)
        {
            audioSource.panStereo = (2 * z) - 1f;
        }
    }
}
