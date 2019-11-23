/* Tal Rastopchin
 * July 31, 2019
 * 
 * A script to read lines from a file into textblocks and cyclicly display them
 * to text and an ourline TextMeshProGUI objects.
 */

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using TMPro;

/* TextViewer is attatched to the controller that it takes input from and sets the
 * text component of displayText and displayOutline TextMeshProGUI objects. displayText
 * is supposed to render white text and the displayOutline object is a copy of the text
 * object but set to black, a z scale of 0 in the Rect Transform, and a black Underlay
 * of Dilate 1 in order to create a black backround for the text to make it more accessible.
 */
public class TextViewer : MonoBehaviour
{
    // relative path of text blocks .txt file
    public string textFile;

    // array to store our resulting textBlocks
    private string[] textBlocks;
    int numTextBlocks;
    int currentTextBlock = 0;

    // CameraRig controller
    private SteamVR_TrackedObject trackedController;
    private SteamVR_Controller.Device device;

    // buttonmask for ButtonB and ButtonA
    private const ulong ButtonB = SteamVR_Controller.ButtonMask.ApplicationMenu;
    private const ulong ButtonA = 128ul;

    // text and outline TextMeshProUGUI objects to set text and outline
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI displayOutline;

    // boolean variable for storing viewing state
    private bool viewing = true;

    /* Start
     * 
     * Reads and error checks the input from the specified input textFile. If an
     * error occurs while reading, logs an error via Debug.LogError and sets the
     * current text block to that error. If no error occurs, the lines read are
     * stored in the textBlocks array.
     * 
     * Called before the first frame update
     */
    void Start()
    {
        // list to store all of the read lines
        List<string> lines = new List<string>();

        try
        {
            // Create an instance of StreamReader to read from a file
            // The using statement also closes the StreamReader
            using (StreamReader streamReader = new StreamReader(textFile))
            {
                string line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                        lines.Add(line);
                }
            }

            textBlocks = lines.ToArray();
            numTextBlocks = textBlocks.Length;
        }
        catch (Exception exception)
        {
            Debug.LogError("Error reading \"" + textFile + "\":\n" + exception.Message);
            lines.Clear();
            numTextBlocks = 1;
            textBlocks = new string[numTextBlocks];
            textBlocks[0] = exception.Message;
        }
       
        trackedController = GetComponent<SteamVR_TrackedObject>();

        SetCurrentTextBlock();
    }

    /* Update
     * 
     * If the joystick button is pressed down, we toggle the viewing boolean and
     * call SetCurrentTextBlock to reflect this change. If we ar currently viewing,
     * pressing the 'B' button will cycle forwards through the textBlocks array and
     * pressing the 'A' button will cycle backwards through the textBlocks
     * array.
     * Called once per frame
     */
    void Update()
    {
        GetControllerDevice();

        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            device.TriggerHapticPulse(500);

            viewing = !viewing;
            SetCurrentTextBlock();
        }

        if (viewing)
        {
            // cycle forwards
            if (device.GetPressDown(ButtonB))
            {
                device.TriggerHapticPulse(500);

                if (currentTextBlock < numTextBlocks - 1)
                    currentTextBlock++;

                SetCurrentTextBlock();
            }

            // cycle backwards
            else if (device.GetPressDown(ButtonA))
            {
                device.TriggerHapticPulse(500);

                if (currentTextBlock > 0)
                    currentTextBlock--;

                SetCurrentTextBlock();
            }
        }

    }

    /* SetCurrentTextBlock
     * 
     * If viewing is true, we set the text fields of the text and outline
     * 
     */
    private void SetCurrentTextBlock()
    {
        if (viewing)
        {
            displayText.text = textBlocks[currentTextBlock];
            displayOutline.text = textBlocks[currentTextBlock];
        }
        else
        {
            displayText.text = "";
            displayOutline.text = "";
        }
    }

    /* GetControllerDevice
     * 
     * Assigns the SteamVR_Controller.Device objects to the corresponding controller
     * device
     */
    private void GetControllerDevice()
    {
        device = SteamVR_Controller.Input((int)trackedController.index);
    }
}

/* Sources:
 * https://support.unity3d.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-
 * https://forum.unity.com/threads/changing-text-through-c-script-nullreferenceexception.530214/
 * https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=netframework-4.8
 */
