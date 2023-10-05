using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}

public class LightpadBlockTouchHandler : MonoBehaviour
{
    public GameObject touchPrefab;
    public Color activeTouchColor = Color.cyan;
    public Color inActiveTouchColor = Color.gray;
    public GameObject activeTouchGO;
    public RectTransform canvasParent;
    public LightpadBlockTouchElement touchElement;
    public LightpadBlockXYZReceiver xyzReceiver;
    public Vector2 canvasMinXY = new(-380, 380);
    public Vector2 touchZoomScaleMinMax = new(20, 300);

    // Start is called before the first frame update
    void Start()
    {
        if (activeTouchGO == null)
        {
            activeTouchGO = Instantiate(touchPrefab, canvasParent.transform);
            touchElement = activeTouchGO.GetComponent<LightpadBlockTouchElement>();
        }
        xyzReceiver.xRawValueChanged += HandleXUpdated;
        xyzReceiver.yRawValueChanged += HandleYUpdated;
        xyzReceiver.zRawValueChanged += HandleZUpdated;
        xyzReceiver.OnTouchBegan.AddListener(HandleTouchBegan);
        xyzReceiver.OnTouchEnded.AddListener(HandleTouchEnded);
    }

    private void HandleTouchBegan()
    {
        if (touchElement != null)
        {
            touchElement.color = activeTouchColor;
        }
    }

    private void HandleTouchEnded()
    {
        if (touchElement != null)
        {
            touchElement.color = inActiveTouchColor;
        }
    }

    private void HandleXUpdated(int x)
    {
        Vector3 currentPos = touchElement.dotImage.rectTransform.anchoredPosition;
        float normal = Mathf.InverseLerp(0, 127, x);
        float remappedX = Mathf.Lerp(canvasMinXY.x, canvasMinXY.y, normal);
        touchElement.dotImage.rectTransform.anchoredPosition = new Vector3(remappedX, currentPos.y, currentPos.z);
    }

    private void HandleYUpdated(int y)
    {
        Vector3 currentPos = touchElement.dotImage.rectTransform.anchoredPosition;
        float normal = Mathf.InverseLerp(0, 127, y);
        float remappedY = Mathf.Lerp(canvasMinXY.x, canvasMinXY.y, normal);
        touchElement.dotImage.rectTransform.anchoredPosition = new Vector3(currentPos.x, remappedY, currentPos.z);
    }

    private void HandleZUpdated(int z)
    {

        float normal = Mathf.InverseLerp(0, 127, z);
        float remappedZ = Mathf.Lerp(touchZoomScaleMinMax.x, touchZoomScaleMinMax.y, normal);
        touchElement.dotImage.rectTransform.sizeDelta = new Vector2(remappedZ, remappedZ);
    }
}
