using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;
using System.Text.RegularExpressions;

public class SolarReorder : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public Material[] Colors;

    public KMSelectable[] Lights;
    public GameObject[] LightColors;
    public KMSelectable[] Buttons;
    public TextMesh[] ButtonNumbers;
    public TextMesh[] SubmissionNumbers;
    public GameObject[] Texts;
    public GameObject[] ColoredButtons;

    string[] colornames = { "black", "blue", "cyan", "green", "magenta", "red", "white", "yellow" };
    string[] edgeworkcheck = { "K", "F", "R", "A", "D", "Y" };

    bool Initialized;
    bool Activated;
    int ViewCount;

    int[] SolarColors = new int[8];
    int[] OrderedNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    List<int> RefNums = new List<int>();
    int[] StartNumbers = new int[16];
    string[] StartToText = new string[16];
    int step = 0;
    int[] EndNumbers = new int[16];

    int[] ShuffledButtons = new int[16];
    int offset = 0;
    float lightshutoff = 0;
    int lightsoff = 0;
    bool buttonsplaying;
    float timespent;

    int ButtonsPressed;
    int[] ButtonSeq = new int[16];
    int[] ActualInput = new int[16];
    bool[] correctinputs = new bool[16];
    bool valid;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;


    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (byte i = 0; i < Buttons.Length; i++)
        {
            KMSelectable Ordering = Buttons[i];
            Ordering.OnInteract += delegate
            {
                Reordering(Ordering);
                return false;
            };
        }
        for (byte i = 0; i < Lights.Length; i++)
        {
            KMSelectable Begin = Lights[i];
            Begin.OnInteract += delegate
            {
                Viewing(Begin);
                return false;
            };
        }
        for (int i = 0; i < ColoredButtons.Length; i++) ColoredButtons[i].SetActive(false);
    }
    void Start ()
    {
        for (int i = 0; i < 16; i++) { ButtonNumbers[i].text = ""; SubmissionNumbers[i].text = ""; }
        Debug.LogFormat("[Solar Reorder #{0}] Module initialized. Submissions not available yet.", moduleId);
        if (edgeworkcheck.Any(bomb.GetSerialNumber().Contains)) Debug.LogFormat("[Solar Reorder #{0}] KFRADY rule applies!", moduleId);
        else Debug.LogFormat("[Solar Reorder #{0}] KFRADY rule does NOT apply.", moduleId);
    }

    void Reordering(KMSelectable Ordering)
    {
        Ordering.AddInteractionPunch(0.9f);
        if (Initialized && !Activated && !ModuleSolved)
        {
            int btn = Array.IndexOf(Buttons, Ordering);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[btn].transform);
            bool valid = true;
            for (int i=0; i<ButtonsPressed; i++)
            {
                if (btn == ActualInput[i])
                {
                    i = 16;
                    valid = false;
                }
            }
            if (valid)
            {
                if (edgeworkcheck.Any(bomb.GetSerialNumber().Contains))
                    ButtonSeq[ButtonsPressed] = btn+1;
                else ButtonSeq[btn] = ButtonsPressed + 1;
                ActualInput[ButtonsPressed] = btn;
                ButtonsPressed++;
                ButtonNumbers[btn].text = ButtonsPressed.ToString();
                if (ButtonsPressed < 10) ButtonNumbers[btn].text = "0" + ButtonNumbers[btn].text;
                if (ButtonsPressed == 16)
                {
                    Activated = true;
                    Debug.LogFormat("[Solar Reorder #{0}] Your submission was {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}...", moduleId, ButtonSeq[0], ButtonSeq[1], ButtonSeq[2], ButtonSeq[3], ButtonSeq[4], ButtonSeq[5], ButtonSeq[6], ButtonSeq[7], ButtonSeq[8], ButtonSeq[9], ButtonSeq[10], ButtonSeq[11], ButtonSeq[12], ButtonSeq[13], ButtonSeq[14], ButtonSeq[15]);
                    StartCoroutine(SubmissionAnim());
                }
            }
        }
    }

    void Viewing(KMSelectable Begin)
    {
        Begin.AddInteractionPunch(0.5f);
        if (!Activated && !ModuleSolved)
        {
            Initialized = true;
            ButtonsPressed = 0;
            audio.PlaySoundAtTransform("begin", transform);
            Activated = true;
            StartCoroutine(Animation());
        }
    }

    IEnumerator SubmissionAnim() //Self-explanatory, this took up way too many lines but it was worth it
    {
        for (int i = 0; i < 16; i++)
        {
            SubmissionNumbers[i].text = ButtonNumbers[i].text;
            SubmissionNumbers[i].color = new Color(0f, 0f, 0f, 0f);
        }
        valid = true;
        for (int i = 0; i < 16; i++) ButtonNumbers[i].text = "";
        yield return new WaitForSeconds(0.5f);
        audio.PlaySoundAtTransform("submit", transform);
        buttonsplaying = true;
        yield return new WaitWhile(() => timespent < 0.1f);
        timespent -= 0.1f;
        for (int i=0; i<8; i++)
        {
            ColoredButtons[i].SetActive(true);
            yield return new WaitWhile(() => timespent < 0.4818f);
            timespent -= 0.4818f;
        }
        for (int i = 0; i < 8; i++)
        {
            ColoredButtons[i+8].SetActive(true);
            yield return new WaitWhile(() => timespent < 0.4366f);
            timespent -= 0.4366f;
        }
        for (int i = 0; i < 8; i++)
        {
            ColoredButtons[i].SetActive(false);
            yield return new WaitWhile(() => timespent < 0.4065f);
            timespent -= 0.4065f;
        }
        for (int i = 0; i < 8; i++)
        {
            ColoredButtons[i+8].SetActive(false);
            yield return new WaitWhile(() => timespent < 0.3764f);
            timespent -= 0.3764f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i]].SetActive(true);
            yield return new WaitWhile(() => timespent < 0.3162f);
            timespent -= 0.3162f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i + 4]].SetActive(true);
            yield return new WaitWhile(() => timespent < 0.25596f);
            timespent -= 0.25596f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i + 8]].SetActive(true);
            yield return new WaitWhile(() => timespent < 0.2108f);
            timespent -= 0.2108f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i + 12]].SetActive(true);
            yield return new WaitWhile(() => timespent < 0.1807f);
            timespent -= 0.1807f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i]].SetActive(false);
            yield return new WaitWhile(() => timespent < 0.1506f);
            timespent -= 0.1506f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i + 4]].SetActive(false);
            yield return new WaitWhile(() => timespent < 0.1054f);
            timespent -= 0.1054f;
        }
        for (int i = 0; i < 4; i++)
        {
            ColoredButtons[ActualInput[i + 8]].SetActive(false);
            yield return new WaitWhile(() => timespent < 0.0753f);
            timespent -= 0.0753f;
        }
        for (int i = 0; i < 3; i++)
        {
            ColoredButtons[ActualInput[i + 12]].SetActive(false);
            yield return new WaitWhile(() => timespent < 0.04517f);
            timespent -= 0.04517f;
        }
        for (int i = 0; i < 2; i++)
        {
            if (i == 0) ColoredButtons[ActualInput[15]].SetActive(false);
            else
            {
                for (int j = 0; j < 16; j++) ColoredButtons[j].SetActive(true);
            }
            yield return new WaitWhile(() => timespent < 0.03011f);
            timespent -= 0.03011f;
        }
        for (int i = 0; i < 16; i++) ColoredButtons[i].SetActive(false);
        yield return new WaitWhile(() => timespent < 1.18859f);
        timespent -= 1.18859f;
        for (int i=0; i<16; i++)
        {
            if (EndNumbers[i] == ButtonSeq[i]) correctinputs[i] = true;
            else { correctinputs[i] = false; valid = false; }
        }
        if (valid) StartCoroutine(SolveAnim()); else StartCoroutine(StrikeAnim());
    }

    IEnumerator SolveAnim()
    {
        ModuleSolved = true;
        Debug.LogFormat("[Solar Reorder #{0}] ...and that's correct! Module solved!", moduleId);
        audio.PlaySoundAtTransform("correct", transform);
        timespent = 0f;
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i += 2) ColoredButtons[i].SetActive(true);
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i += 2) { ColoredButtons[i].SetActive(false); ColoredButtons[i+1].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i += 2) { ColoredButtons[i+1].SetActive(false); ColoredButtons[i].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i += 2) { ColoredButtons[i].SetActive(false); ColoredButtons[i+1].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.1805f);
        timespent -= 0.1805f;
        for (int i = 0; i < 16; i += 2) { ColoredButtons[i+1].SetActive(false); ColoredButtons[i].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i += 2) { ColoredButtons[i+1].SetActive(true); }
        module.HandlePass();
        yield return new WaitWhile(() => timespent < 0.85736f);
        timespent -= 0.85736f;
        for (int i = 4; i < 16; i++) { ColoredButtons[i].SetActive(false); }
        yield return new WaitWhile(() => timespent < 0.1354f);
        timespent -= 0.1354f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.1354f);
        timespent -= 0.1354f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.1354f);
        timespent -= 0.1354f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.1354f);
        timespent -= 0.1354f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.1354f);
        timespent -= 0.1354f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.1354f);
        timespent -= 0.1354f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.4964f);
        timespent -= 0.4964f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(false); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 4].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 8].SetActive(false); }
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(true); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 4; i++) { ColoredButtons[i + 12].SetActive(false); }
        yield return new WaitWhile(() => timespent < 0.8122f);
        timespent -= 0.8122f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(true); }
        LightColors[0].GetComponent<Renderer>().material = Colors[3];
        LightColors[7].GetComponent<Renderer>().material = Colors[3];
        yield return new WaitWhile(() => timespent < 0.1805f);
        timespent -= 0.1805f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(false); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(true); }
        LightColors[1].GetComponent<Renderer>().material = Colors[3];
        LightColors[6].GetComponent<Renderer>().material = Colors[3];
        yield return new WaitWhile(() => timespent < 0.1805f);
        timespent -= 0.1805f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(false); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(true); }
        LightColors[2].GetComponent<Renderer>().material = Colors[3];
        LightColors[5].GetComponent<Renderer>().material = Colors[3];
        yield return new WaitWhile(() => timespent < 0.1805f);
        timespent -= 0.1805f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(false); }
        yield return new WaitWhile(() => timespent < 0.0902f);
        timespent -= 0.0902f;
        for (int i = 0; i < 16; i++) { ColoredButtons[i].SetActive(true); }
        LightColors[3].GetComponent<Renderer>().material = Colors[3];
        LightColors[4].GetComponent<Renderer>().material = Colors[3];
    }

    IEnumerator StrikeAnim()
    {
        audio.PlaySoundAtTransform("wrong", transform);
        Debug.LogFormat("[Solar Reorder #{0}] ...which is sadly incorrect. No worries, you can try again.", moduleId);
        timespent = 0f;
        yield return new WaitWhile(() => timespent < 0.3011f);
        timespent -= 0.3011f;
        module.HandleStrike();
        for (int i = 0; i < 16; i++) ColoredButtons[i].SetActive(true);
        yield return new WaitWhile(() => timespent < 0.4517f);
        timespent -= 0.4517f;
        for (int i=0; i<100; i++)
        {
            for (int j=0; j<16; j++)
            {
                SubmissionNumbers[j].color = new Color(0f, 0f, 0f, 0.01f+(Convert.ToSingle(i)/100f));
            }
            yield return new WaitWhile(() => timespent < 0.02439f);
            timespent -= 0.02439f;
        }
        for (int i=0; i<16; i++)
        {
            if (correctinputs[i] == false)
            {
                if (edgeworkcheck.Any(bomb.GetSerialNumber().Contains))
                {
                    ColoredButtons[ActualInput[i] + 16].SetActive(true);
                    ColoredButtons[ActualInput[i]].SetActive(false);
                }
                else
                {
                    ColoredButtons[ButtonSeq[ActualInput[i]] + 15].SetActive(true);
                    ColoredButtons[ButtonSeq[ActualInput[i]] - 1].SetActive(false);
                }
            }
        }
        for (int i = 100; i > 0; i--)
        {
            for (int j = 0; j < 16; j++)
            {
                SubmissionNumbers[j].color = new Color(0f, 0f, 0f, 0f + (Convert.ToSingle(i) / 100f));
            }
            yield return new WaitWhile(() => timespent < 0.115635f);
            timespent -= 0.115635f;
        }
        for (int j = 0; j < 16; j++)
        {
            SubmissionNumbers[j].color = new Color(0f, 0f, 0f, 0f);
            ActualInput[j] = 0;
            ButtonSeq[j] = 0;
            SubmissionNumbers[j].text = "";
            correctinputs[j] = true;
        }
        Activated = false;
        ButtonsPressed = 0;
        buttonsplaying = false;
        for (int i = 0; i < ColoredButtons.Length; i++) ColoredButtons[i].SetActive(false);
    }

    IEnumerator Animation() //Sequence flashing animation and calculations
    {
        step = 0;
        if (ViewCount == 0)
        {
            for (int i=0; i<SolarColors.Length; i++)
            {
                SolarColors[i] = rnd.Range(0, 8);
            }
            Debug.LogFormat("[Solar Reorder #{0}] New color sequence generated! In order, they are: {1}, {2}, {3}, {4}, {5}, {6}, {7}, and {8}.", moduleId, colornames[SolarColors[0]], colornames[SolarColors[1]], colornames[SolarColors[2]], colornames[SolarColors[3]], colornames[SolarColors[4]], colornames[SolarColors[5]], colornames[SolarColors[6]], colornames[SolarColors[7]]);
        }
        ViewCount = (ViewCount + 1)%3;
        ButtonsPressed = 0;
        for (int i = 0; i < 16; i++) RefNums.Add(OrderedNumbers[i]); //Resetting the list
        for (int i=15; i>=0; i--)
        {
            int temp = rnd.Range(0, i+1);
            StartNumbers[i] = RefNums[temp]; //Technically, this writes the array backwards, but it still works pretty much the same as forwards
            EndNumbers[i] = StartNumbers[i];
            RefNums.RemoveAt(temp);
            if (StartNumbers[i] < 10) StartToText[i] = "0" + StartNumbers[i].ToString();
            else StartToText[i] = StartNumbers[i].ToString();
            ButtonSeq[i] = 0;
            ActualInput[i] = 0;
        }
        for (int i = 0; i < 16; i++) RefNums.Add(OrderedNumbers[i]);
        for (int i = 15; i >= 0; i--)
        {
            int temp = rnd.Range(0, i+1);
            ShuffledButtons[i] = RefNums[temp]; //We're just doing the same thing I did above but with this array, which makes the flashes less predictable
            RefNums.RemoveAt(temp);
        }
        for (int i = 0; i < 16; i++) { ButtonNumbers[i].text = StartToText[i]; Texts[i].SetActive(false); }
        Debug.LogFormat("[Solar Reorder #{0}] The number sequence this time around is {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, StartNumbers[0], StartNumbers[1], StartNumbers[2], StartNumbers[3], StartNumbers[4], StartNumbers[5], StartNumbers[6], StartNumbers[7], StartNumbers[8], StartNumbers[9], StartNumbers[10], StartNumbers[11], StartNumbers[12], StartNumbers[13], StartNumbers[14], StartNumbers[15]);
        LightColors[0].GetComponent<Renderer>().material = Colors[SolarColors[0]]; //Color activation
        buttonsplaying = true;
        yield return new WaitWhile(() => timespent < 0.271f);
        timespent -= 0.271f;
        LightColors[1].GetComponent<Renderer>().material = Colors[SolarColors[1]];
        yield return new WaitWhile(() => timespent < 0.271f);
        timespent -= 0.271f;
        LightColors[2].GetComponent<Renderer>().material = Colors[SolarColors[2]];
        yield return new WaitWhile(() => timespent < 0.271f);
        timespent -= 0.271f;
        LightColors[3].GetComponent<Renderer>().material = Colors[SolarColors[3]];
        yield return new WaitWhile(() => timespent < 0.1805f);
        timespent -= 0.1805f;
        LightColors[4].GetComponent<Renderer>().material = Colors[SolarColors[4]];
        yield return new WaitWhile(() => timespent < 0.03f);
        timespent -= 0.03f;
        LightColors[5].GetComponent<Renderer>().material = Colors[SolarColors[5]];
        yield return new WaitWhile(() => timespent < 0.03f);
        timespent -= 0.03f;
        LightColors[6].GetComponent<Renderer>().material = Colors[SolarColors[6]];
        yield return new WaitWhile(() => timespent < 0.03f);
        timespent -= 0.03f;
        LightColors[7].GetComponent<Renderer>().material = Colors[SolarColors[7]];
        buttonsplaying = false;
        yield return new WaitForSeconds(0.56f);
        while (step < 8) ColorHandler(SolarColors[step]);
        Debug.LogFormat("[Solar Reorder #{0}] The final sequence is {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15]);
        audio.PlaySoundAtTransform("sequence", transform); //OH BOY TIME FOR THE PART WHERE THE BUTTONS SHOW THEIR NUMBERS AND STUFF I GUESS IDK
        yield return new WaitForSeconds(0.02f);
        offset = 0;
        lightshutoff = 0;
        lightsoff = 0;
        buttonsplaying = true;
        for (int i = 0; i < 115; i++)
        {
            switch (i)
            {
                case 0: case 2: case 4: case 5: case 7: case 9: case 11: case 12: case 14: case 16: case 18: case 20: case 21: case 23: case 25: case 27: case 28: case 30: case 32: case 34: case 36: case 37: case 39: case 41: case 44: case 46: case 48: case 49: case 51: case 54: case 56: case 83: case 85: case 86: case 88: case 90: case 91: case 93: case 95: case 96: case 98:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0902f); timespent -= 0.0902f;; break; //Two drums together
                case 1: case 3: case 6: case 8: case 10: case 13: case 15: case 17: case 19: case 22: case 24: case 26: case 29: case 31: case 33: case 35: case 38: case 40: case 43: case 45: case 47: case 50: case 55: case 58: case 61: case 63: case 66: case 69: case 70: case 72: case 84: case 87: case 89: case 92: case 94: case 97: case 99: case 101: case 103: case 105: case 107: case 108: case 109: case 110: case 111: case 112: case 113: case 114:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0902f); timespent -= 0.0902f;; break; //Single drum
                case 42: case 52: case 62: case 75: case 77:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //Drum followed by another
                case 53: case 57: case 64: case 65: case 71: case 76:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //Two drums followed by one drum
                case 59: case 79:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //Two after two
                case 67: case 73: case 78: case 80:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //One followed by two drums
                case 68: case 74:
                    yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //Delayed one
                case 81:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //Kick+snare followed by double kick, snare, and hihat
                case 82:
                    Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); offset++; Texts[ShuffledButtons[(i + offset) % 16] - 1].SetActive(true); yield return new WaitWhile(() => timespent < 0.0451f); timespent -= 0.0451f;; break; //Kick+snare, then double kick+snare
                default:
                    yield return new WaitWhile(() => timespent < 0.0902f); timespent -= 0.0902f;; break; //Silence
            }
            for (int j = 0; j < 16; j++) Texts[j].SetActive(false);
            lightshutoff++;
            if (lightshutoff >= 14.375)
            {
                LightColors[lightsoff].GetComponent<Renderer>().material = Colors[0];
                lightsoff++;
                lightshutoff -= 14.375f;
            }
        }
        for (int i=0; i < 16; i++) { ButtonNumbers[i].text = ""; Texts[i].SetActive(true); }
        buttonsplaying = false;
        Activated = false;
    }

    void ColorHandler(int arg)
    {
        switch (arg){
            case 0: Blackflash(); break;
            case 1: Blueflash(); break;
            case 2: Cyanflash(); break;
            case 3: Greenflash(); break;
            case 4: Magentaflash(); break;
            case 5: Redflash(); break;
            case 6: Whiteflash(); break;
            case 7: Yellowflash(); break;
        }
        step++;
    }

    void Blackflash()
    {
        Debug.LogFormat("[Solar Reorder #{0}] Step {1}: Sequence is not being modified!", moduleId, step);
    }

    void Redflash()
    {
        for (int i=0; i<16; i += 2)
        {
            int temp = EndNumbers[i];
            EndNumbers[i] = EndNumbers[i + 1];
            EndNumbers[i + 1] = temp;
        }
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After swapping each pair, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Greenflash()
    {
        for (int i=0; i<16; i++)
        {
            EndNumbers[i]++;
            if (EndNumbers[i] == 17) EndNumbers[i] = 1;
        }
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After incrementing by one, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Blueflash()
    {
        int temp = EndNumbers[0];
        for (int i=0; i<15; i++)
            EndNumbers[i] = EndNumbers[i + 1];
        EndNumbers[15] = temp;
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After shifting to the left, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Yellowflash()
    {
        int temp = 0;
        for (int i=0; i<16; i += 8)
        {
            temp = EndNumbers[i];
            EndNumbers[i] = EndNumbers[i + 7];
            EndNumbers[i + 7] = temp;
            temp = EndNumbers[i + 1];
            EndNumbers[i + 1] = EndNumbers[i + 6];
            EndNumbers[i + 6] = temp;
            temp = EndNumbers[i + 2];
            EndNumbers[i + 2] = EndNumbers[i + 5];
            EndNumbers[i + 5] = temp;
            temp = EndNumbers[i + 3];
            EndNumbers[i + 3] = EndNumbers[i + 4];
            EndNumbers[i + 4] = temp;
        }
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After reversing both halves, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Cyanflash()
    {
        for (int i = 0; i < 16; i++)
        {
            EndNumbers[i] += 8;
            EndNumbers[i] = EndNumbers[i] % 16;
            if (EndNumbers[i] == 0) EndNumbers[i] = 16;
        }
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After incrementing by eight, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Magentaflash()
    {
        int temp = 0;
        for (int i=0; i<8; i++)
        {
            temp = EndNumbers[i];
            EndNumbers[i] = EndNumbers[i + 8];
            EndNumbers[i + 8] = temp;
        }
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After swapping both halves, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Whiteflash()
    {
        int temp = 0;
        for (int i=0; i<8; i++)
        {
            temp = EndNumbers[i];
            EndNumbers[i] = EndNumbers[15 - i];
            EndNumbers[15 - i] = temp;
        }
        Debug.LogFormat("[Solar Reorder #{0}] Step {17}: After reversing the full sequence, the sequence is now {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16}.", moduleId, EndNumbers[0], EndNumbers[1], EndNumbers[2], EndNumbers[3], EndNumbers[4], EndNumbers[5], EndNumbers[6], EndNumbers[7], EndNumbers[8], EndNumbers[9], EndNumbers[10], EndNumbers[11], EndNumbers[12], EndNumbers[13], EndNumbers[14], EndNumbers[15], step);
    }

    void Update ()
    {
        if (buttonsplaying) timespent += Time.deltaTime; //Helps to ensure button flashes stay in sync with the music even if framerates are stupid
        else timespent = 0f;
	}

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!Initialized || !ButtonSeq.Take(ButtonsPressed).SequenceEqual(EndNumbers.Take(ButtonsPressed)))
        {
            Lights.PickRandom().OnInteract();
            while (Activated)
                yield return true;
        }
        var orderDeterminedToPress = edgeworkcheck.Any(bomb.GetSerialNumber().Contains) ? EndNumbers : OrderedNumbers.OrderBy(a => EndNumbers[a - 1]).ToArray();
        for (var x = ButtonsPressed; x < 16; x++)
        {
            Buttons[orderDeterminedToPress[x] - 1].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        while (Activated)
            yield return true;
    }
#pragma warning disable IDE0051 // Remove unused private members
    readonly string TwitchHelpMessage = "\"!{0} go/activate/start\" [Plays the sequence.] | \"!{0} press 1 5 10 16 2\" or \"!{0} submit 1 5 10 16 2\" [Presses the 1st, 5th, 10th, 16th, and 2nd buttons in reading order.]";
#pragma warning restore IDE0051 // Remove unused private members

    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var rgxStart = Regex.Match(cmd, @"^(go|activate|start)$");
        var rgxPress = Regex.Match(cmd, @"^(press|submit)(\s\d{1,2})+$");
        if (rgxStart.Success)
        {
            if (Activated)
            {
                yield return "sendtochaterror Can't interact right now! Wait until the sequence has finished playing.";
                yield break;
            }
            yield return null;
            Lights.PickRandom().OnInteract();
        }
        else if (rgxPress.Success)
        {
            if (Activated)
            {
                yield return "sendtochaterror Can't interact right now! Wait until the sequence has finished playing.";
                yield break;
            }
            if (!Initialized)
            {
                yield return "sendtochaterror Can't press any buttons right now! Did you forget to play a sequence?";
                yield break;
            }
            var splitCoordsAll = rgxPress.Value.Trim().Split().Skip(1);
            var btnsToPress = new List<KMSelectable>();
            foreach (var coord in splitCoordsAll)
            {
                int idx;
                if (!int.TryParse(coord, out idx) || idx < 1 || idx > 16)
                    yield break;
                btnsToPress.Add(Buttons[idx - 1]);
            }
            yield return null;
            for (int i = 0; i < btnsToPress.Count; i++)
            {
                KMSelectable btn = btnsToPress[i];
                btn.OnInteract();
                yield return new WaitForSeconds(0.1f);
                if (Activated)
                {
                    yield return "solve";
                    yield return "strike";
                    if (i + 1 < btnsToPress.Count)
                        yield return "sendtochat Button presses have been interuptted after " + (i + 1).ToString() + " press(es)!";
                    yield break;
                }
            }
        }
        yield break;
    }

}
