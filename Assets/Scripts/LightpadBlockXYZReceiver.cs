using UnityEngine;
using System.Collections.Generic;
using RtMidi.LowLevel;
using UnityEngine.Events;

[System.Serializable]
public class CCXDirectionUpdated : UnityEvent<int>
{
}
[System.Serializable]
public class CCYDirectionUpdated : UnityEvent<int>
{
}
[System.Serializable]
public class CCZDirectionUpdated : UnityEvent<int>
{
}


public class LightpadBlockXYZReceiver : MonoBehaviour
{
    #region Private members

    MidiProbe _probe;
    List<MidiInPort> _ports = new List<MidiInPort>();

    [Header("CC")]
    public int xChannel = 113;
    public int yChannel = 114;
    public int zChannel = 115;

    public CCXDirectionUpdated onXUpdated;
    public CCYDirectionUpdated onYUpdated;
    public CCZDirectionUpdated onZUpdated;

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
            );;
        }
    }

    private void HandleControlChangeMessage(string name, byte channel, byte number, byte value)
    {
        if (number == xChannel)
        {
            //Debug.Log(string.Format("X: {0} ({1})", number, value));
            onXUpdated.Invoke(value);
        }
        if (number == yChannel)
        {
            //Debug.Log(string.Format("Y: {0} ({1})", number, value));
            onYUpdated.Invoke(value);
        }
        if (number == zChannel)
        {
            //Debug.Log(string.Format("Z: {0} ({1})", number, value));
            onZUpdated.Invoke(value);
        }

        //Debug.Log(string.Format("{0} [{1}] CC {2} ({3})", name, channel, number, value));
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
        foreach (var p in _ports) p?.ProcessMessages();
    }

    void OnDestroy()
    {
        _probe?.Dispose();
        DisposePorts();
    }

    #endregion
}
