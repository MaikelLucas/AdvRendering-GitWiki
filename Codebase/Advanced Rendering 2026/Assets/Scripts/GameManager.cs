using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // FPS Logging
    //private List<float> _fpsList = new();
    //private bool _logFPS = false;
    public float logTime = 10f;
    private float _logTimer = 0f;
    //public bool logOnPlay = false;

    [Header("Log Settings")]
    [SerializeField] private string _filename = "test";

    private bool _testRunning = false;
    private List<float> _fpsList = new();
    private List<float> _timeList = new();

    [SerializeField] private int _randomSeed = 0;

    private void Awake()
    {
        UnityEngine.Random.InitState(_randomSeed);
    }

    private void Start()
    {
        _testRunning = true;

        CheckSheet();   // Checks if sheet is open
    }

    private void Update()
    {
        ReloadScene();
        //LogFPS();
        UpdateTimer();
        if (_testRunning)
        {
            LogData();
        }
    }

    private void UpdateTimer()
    {
        _logTimer += Time.deltaTime;
        if (_logTimer >= logTime)
        {
            EndTest();
        }
    }

    private void LogData()
    {
        //Debug.Log(UnityStats.triangles + " triangles");
        _fpsList.Add(1f / Time.deltaTime);
        _timeList.Add(Time.time);
    }

    private void CreateFile()
    {
        string filepath = Application.dataPath + "/Results/" + _filename + ".csv";  // Set filepath
        TextWriter tw = new StreamWriter(filepath, true);                           // Open textwriter

        tw.WriteLine(",FPS");                                             // Write headers
        for (int i = 0; i < _fpsList.Count; i++)
        {
            string line = "";
            line += _timeList[i] + ",";
            line += _fpsList[i] + ",";
            tw.WriteLine(line);                                                     // Write data line
        }

        tw.Close();                                                                 // Close textwriter
        Debug.Log("File Saved");
    }

    private void EndTest()
    {
        // Stop the test (makes sure no more data is logged)
        _testRunning = false;

        CreateFile();

        Debug.Log("Test Ended");

        // Quit application
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void CheckSheet()
    {
        try
        {
            string filepath = Application.dataPath + "/Results/" + _filename + ".csv";  // Set filepath
            TextWriter tw = new StreamWriter(filepath, true);                           // Open textwriter
            tw.Close();                                                                 // Close textwriter
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Close the Excel Sheet");
            throw ex;
        }
    }

    //private void LogFPS()
    //{
    //    // start logging at the start
    //    if (logOnPlay)
    //    {
    //        logOnPlay = false;
    //        _logFPS = true;
    //        Debug.Log("Started Logging");
    //    }

    //    // start logging FPS when F key is pressed
    //    if (Keyboard.current.fKey.wasPressedThisFrame)
    //    {
    //        _logFPS = true;
    //        Debug.Log("Started Logging");
    //    }

    //    if (_logFPS)
    //    {
    //        // increase timer & log current FPS
    //        _logTimer += Time.deltaTime;
    //        _fpsList.Add(1f / Time.deltaTime);

    //        if (_logTimer >= logTime)
    //        {
    //            // calculate average FPS
    //            float averageFPS = 0f;
    //            foreach (float fps in _fpsList)
    //            {
    //                averageFPS += fps;
    //            }
    //            averageFPS /= _fpsList.Count;
    //            Debug.Log("Stopped Logging");
    //            Debug.Log($"Average FPS: {averageFPS:F2}");

    //            // reset logging
    //            _logFPS = false;
    //            _logTimer = 0f;
    //            _fpsList.Clear();
    //        }
    //    }
    //}

    private static void ReloadScene()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}