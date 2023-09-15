using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public class LightpadBlockTouchElement : MonoBehaviour
{
    public SVGImage dotImage;
    public Color color { set { dotImage.color = value; } }
    private float _size;
    public float Size { set { SetSize(value); } }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void SetSize(float value)
    {
        _size = value;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
