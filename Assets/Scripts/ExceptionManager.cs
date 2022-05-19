using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExceptionManager : MonoBehaviour {

    public TextMeshProUGUI Text;
    void Awake() {
        Application.logMessageReceived += HandleException;
        DontDestroyOnLoad(gameObject);
    }

    void HandleException(string logString, string stackTrace, LogType type) {
        if (type == LogType.Exception && !stackTrace.Contains("InteractorGroup")) {
            Text.text = logString + "\n" + stackTrace + Text.text;
        }
    }
}