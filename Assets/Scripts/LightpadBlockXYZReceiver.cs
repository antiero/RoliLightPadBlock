using UnityEngine;
using System.Collections.Generic;
using RtMidi.LowLevel;
using UnityEngine.Events;

public class LightpadBlockXYZReceiver : MonoBehaviour
{
    #region Private members

    MidiProbe _probe;
    List<MidiInPort> _ports = new List<MidiInPort>();


    [Header("CC")]
    public int xChannel = 113;
    public int yChannel = 114;
    public int zChannel = 115;

    private float _lastTouchTime = 0f;

    [SerializeField]
    private bool _touching;
    public bool Touching
    {
        get { return _touching; }
        set { _touching = value; }
    }

    [Header("RAW CC Values")]
    [SerializeField]
    private int _x;
    public int RawXValue
    {
        get { return _x; }
        set { _x = value; }
    }
    [SerializeField]
    private int _y;
    public int RawYValue
    {
        get { return _y; }
        set { _y = value; }
    }
    [SerializeField]
    private int _z;
    public int RawZValue
    {
        get { return _z; }
        set { _z = value; }
    }

    [Header("Normalised Float Values")]
    [SerializeField]
    private float _xFloat;
    public float XFloat
    {
        get { return _xFloat; }
        set { _xFloat = value; }
    }
    [SerializeField]
    private float _yFloat;
    public float YFloat
    {
        get { return _yFloat; }
        set { _yFloat = value; }
    }
    [SerializeField]
    private float _zFloat;
    public float ZFloat
    {
        get { return _zFloat; }
        set { _zFloat = value; }
    }

    [Header("Current Curve Values")]
    [SerializeField]
    private float _xCurve;
    public float XCurve
    {
        get { return _xCurve; }
        set { _xCurve = GetXCurve(); }
    }
    [SerializeField]
    private float _yCurve;
    public float YCurve
    {
        get { return _yCurve; }
        set { _yCurve = value; }
    }
    [SerializeField]
    private float _zCurve;
    public float ZCurve
    {
        get { return _zCurve; }
        set { _zCurve = GetZCurve(); }
    }

    [Header("Events / Actions")]
    public UnityEvent OnTouchBegan;
    public UnityEvent OnTouchEnded;

    // We can send raw CC values of 0-127..
    public UnityAction<int> xRawValueChanged;
    public UnityAction<int> yRawValueChanged;
    public UnityAction<int> zRawValueChanged;

    // Or normalise the value of 0-127 to the range of 0-1...
    public UnityAction<float> xNormalisedValueChanged;
    public UnityAction<float> yNormalisedValueChanged;
    public UnityAction<float> zNormalisedValueChanged;

    // Or use a Curve to map values of 0-127 to a response curve
    public UnityAction<float> xCurveValueChanged;
    public UnityAction<float> yCurveValueChanged;
    public UnityAction<float> zCurveValueChanged;

    [Header("Scaling Curves")]
    public AnimationCurve xCurve;
    public AnimationCurve yCurve;
    public AnimationCurve zCurve;



    public float GetXCurve()
    {
        float value = xCurve.Evaluate(XFloat);
        return value;
    }

    public float GetYCurve()
    {
        float value = yCurve.Evaluate(YFloat);
        return value;
    }

    public float GetZCurve()
    {
        float value = zCurve.Evaluate(ZFloat);
        return value;
    }

    // Does the port seem real or not?
    // This is mainly used on Linux (ALSA) to filter automatically generated
    // virtual ports.
    bool IsRealPort(string name)
    {
        return !name.Contains("Through") && !name.Contains("RtMidi");
    }

    // Scan and open all the available output ports.
    void ScanPorts()
    {
        for (var i = 0; i < _probe.PortCount; i++)
        {
            var name = _probe.GetPortName(i);
            Debug.Log("MIDI-in port found: " + name);

            _ports.Add(IsRealPort(name) ? new MidiInPort(i)
            {
                //OnNoteOn = (byte channel, byte note, byte velocity) =>
                //    Debug.Log(string.Format("{0} [{1}] On {2} ({3})", name, channel, note, velocity)),

                //OnNoteOff = (byte channel, byte note) =>
                //    Debug.Log(string.Format("{0} [{1}] Off {2}", name, channel, note)),

                OnControlChange = (byte channel, byte number, byte value) =>
                    HandleControlChangeMessage(name, channel, number, value)

            } : null
            ); ;
        }
    }

    // Sets mode to touching. Fires event if it wasn't previously touching.
    private void HandleXYZIsTouching()
    {
        if (!Touching)
        {
            OnTouchBegan.Invoke();
        }
        Touching = true;
    }

    private void HandleControlChangeMessage(string name, byte channel, byte number, byte value)
    {
        _lastTouchTime = Time.fixedTime;
        if (number == xChannel)
        {
            //Debug.Log(string.Format("X: {0} ({1})", number, value));
            if (RawXValue != value)
            {
                xRawValueChanged(value);
                XFloat = value / 127f;
                XCurve = GetXCurve();
                xNormalisedValueChanged(XFloat);
            }
            RawXValue = value;
            if (!Touching)
            {
                OnTouchBegan.Invoke();
            }
            HandleXYZIsTouching();
            return;
        }
        if (number == yChannel)
        {
            if (RawYValue != value)
            {
                YFloat = value / 127f;
                yRawValueChanged(value);
                YCurve = GetYCurve();
                yNormalisedValueChanged(YFloat);
            }
            RawYValue = value;
            HandleXYZIsTouching();
            return;
        }
        if (number == zChannel)
        {
            if (RawZValue != value)
            {
                ZFloat = value / 127f;
                zRawValueChanged(value);
                ZCurve = GetZCurve();
                zNormalisedValueChanged(ZFloat);
            }
            RawZValue = value;
            HandleXYZIsTouching();
            return;
        }
    }

    // Close and release all the opened ports.
    void DisposePorts()
    {
        foreach (var p in _ports) p?.Dispose();
        _ports.Clear();
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _probe = new MidiProbe(MidiProbe.Mode.In);
    }

    void Update()
    {
        // Rescan when the number of ports changed.
        if (_ports.Count != _probe.PortCount)
        {
            DisposePorts();
            ScanPorts();
        }

        // Process queued messages in the opened ports.
        foreach (var p in _ports)
        {
            p?.ProcessMessages();
        }

        if (_lastTouchTime < Time.fixedTime - 0.05f)
        {
            if (Touching)
            {
                OnTouchEnded.Invoke();
            }
            Touching = false;
        }
    }

    void OnDestroy()
    {
        _probe?.Dispose();
        DisposePorts();
    }

    #endregion
}
