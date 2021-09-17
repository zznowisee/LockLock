using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Transform gridBtnPanel;
    Transform realTimeBtnPanel;

    Button startBtn;
    Button stopBtn;
    Button restartBtn;
    Button clearBtn;

    Button pauseBtn;
    Button runBtn;

    CustomSystem customSystem;
    private void Awake()
    {
        customSystem = FindObjectOfType<CustomSystem>();
        gridBtnPanel = transform.Find("gridBtnPanel");
        realTimeBtnPanel = transform.Find("realTimeBtnPanel");

        startBtn = gridBtnPanel.Find("startBtn").GetComponent<Button>();
        stopBtn = gridBtnPanel.Find("stopBtn").GetComponent<Button>();
        clearBtn = gridBtnPanel.Find("clearBtn").GetComponent<Button>();
        restartBtn = gridBtnPanel.Find("restartBtn").GetComponent<Button>();

        pauseBtn = realTimeBtnPanel.Find("pauseBtn").GetComponent<Button>();
        runBtn = realTimeBtnPanel.Find("runBtn").GetComponent<Button>();
    }

    private void Start()
    {
        startBtn.onClick.AddListener(() => customSystem.StartGame());
        stopBtn.onClick.AddListener(() => customSystem.StopGame());
        clearBtn.onClick.AddListener(() => customSystem.Clear());
        restartBtn.onClick.AddListener(() => customSystem.Restart());

        pauseBtn.onClick.AddListener(() => customSystem.PauseGame());
        runBtn.onClick.AddListener(() => customSystem.Run());
    }
}
