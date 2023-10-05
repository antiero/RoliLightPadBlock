using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YPitchModifierTest : MonoBehaviour
{
    public LightpadBlockXYZReceiver xyzReceiver;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        xyzReceiver.yNormalisedValueChanged += HandleYChanged;
    }

    private void HandleYChanged(float z)
    {
        if (audioSource != null)
        {
            audioSource.pitch = ((2 * z) - 1f);
        }
    }
}
