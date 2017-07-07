using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum State
{
    WAITING,
    CALIBRATE_STRAIGHT,
    CALIBRATE_BEND,
    PLAYING
}

public class Arduino : MonoBehaviour
{
    SerialPort stream;
    public Text calibrateText;
    public int deadzone = 3;
    public float CALIBRATE_STRAIGHT_TIME = 3;
    public float CALIBRATE_BEND_TIME = 5;
    private float timer = 0;
    private State state = State.WAITING;
    private int minStraight = -1;
    private int maxStraight = -1;
    private int minBend = -1;
    private int maxBend = -1;
    private bool inverted = false; // set to true when bend int is lower than straight
    private int totalMin; // totalmin and max are used to calculate var from 0-100 from
    private int totalMax;
    private float _value;
    private float lastRealValue = 0;
    public float Value
    {
        get {
            return _value;
        }
        set
        {
            float v = value;
            if (v < totalMin) v = totalMin;
            else if (v > totalMax) v = totalMax;
            v = (v - totalMin) / (totalMax - totalMin) * 100f;
            if (inverted) v = 100 - v;
            _value = v;
        }
    }
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void StartComp(string compName)
    {
        stream = new SerialPort(compName, 9600);
        stream.Open();
        state = State.CALIBRATE_STRAIGHT;
        timer = CALIBRATE_STRAIGHT_TIME;
        calibrateText.gameObject.SetActive(true);
        calibrateText.text = "Please don't touch the sensor for " + CALIBRATE_STRAIGHT_TIME + " seconds";
    }

    void MoveToBendTest()
    {
        state = State.CALIBRATE_BEND;
        timer = CALIBRATE_BEND_TIME;
        calibrateText.text = "Please bend as far as you can for " + CALIBRATE_BEND_TIME + " seconds";
    }

    void Update()
    {
        if (stream == null) { return; }
        int value;
        if (!int.TryParse(stream.ReadLine(), out value)) return;
        if (value == 0) { return; }
        switch (state)
        {
            case State.CALIBRATE_STRAIGHT:
                timer -= Time.deltaTime;
                CalibrateToVars(ref minStraight, ref maxStraight, value);
                calibrateText.text = "Please don't touch the sensor for " + Mathf.RoundToInt(timer) + " seconds, min: " + minStraight + "max: " + maxStraight;
                if (timer < 0) MoveToBendTest();
                break;
            case State.CALIBRATE_BEND:
                timer -= Time.deltaTime;
                CalibrateToVars(ref minBend, ref maxBend, value);
                calibrateText.text = "Please bend as far as you can for " + Mathf.RoundToInt(timer) + " seconds, min: " + minBend + "max: " + maxBend;
                if (timer < 0) FinishCalibration();
                break;
            case State.PLAYING:
                if (Mathf.Abs(value - lastRealValue) < deadzone) return; // no slight changes
                Value = value;
                calibrateText.text = "Val: " + Value;
                break;
        }
        lastRealValue = value;
    }

    void FinishCalibration()
    {
        float b = minBend + maxBend / 2;
        float s = minStraight + maxStraight / 2;
        if (b < s) inverted = true;
        totalMin = minStraight < minBend ? minStraight : minBend;
        totalMax = maxStraight > maxBend ? maxStraight : maxBend;
        state = State.PLAYING;
        SceneManager.LoadScene("flower");
    }

    void CalibrateToVars(ref int min, ref int max, int value)
    {
        if (min == -1)
        {
            min = value;
            max = value;
            return;
        }
        if (value < min) min = value;
        else if (value > max) max = value;
    }
}
