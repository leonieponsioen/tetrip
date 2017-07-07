using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;

[RequireComponent(typeof(Dropdown))]
public class COMPortDropdown : MonoBehaviour {
    public Button button;
    public Arduino arduino;
    private Dropdown dropdown;
    private string[] values;
	// Use this for initialization
	void Start ()
    {
        dropdown = GetComponent<Dropdown>();
        dropdown.ClearOptions();
        values = SerialPort.GetPortNames();
        foreach (string s in values)
        {
            dropdown.options.Add(new Dropdown.OptionData(s));
        }
        button.onClick.AddListener(Click);
    }

    void Click()
    {
        arduino.StartComp(values[dropdown.value]);
        Destroy(button.gameObject);
        Destroy(gameObject);
    }
}
