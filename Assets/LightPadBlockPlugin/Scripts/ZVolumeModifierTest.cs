using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZVolumeModifierTest : MonoBehaviour
{
    public LightpadBlockXYZReceiver xyzReceiver;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        xyzReceiver.zNormalisedValueChanged += HandleZChanged;
    }

    private void HandleZChanged(float z)
    {
        if (audioSource != null)
        {
            audioSource.volume = z;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
