using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Transform gridBtnPanel;

    Button startBtn;
    Button stopBtn;
    Button resetBtn;
    Button clearBtn;

    CustomSystem customSystem;
    private void Awake()
    {
        customSystem = FindObjectOfType<CustomSystem>();
        gridBtnPanel = transform.Find("gridBtnPanel");

        startBtn = gridBtnPanel.Find("startBtn").GetComponent<Button>();
        stopBtn = gridBtnPanel.Find("stopBtn").GetComponent<Button>();
        clearBtn = gridBtnPanel.Find("clearBtn").GetComponent<Button>();
        resetBtn = gridBtnPanel.Find("resetBtn").GetComponent<Button>();
    }

    private void Start()
    {
        startBtn.onClick.AddListener(() => customSystem.StartBtn());
        stopBtn.onClick.AddListener(() => customSystem.StopBtn());
        clearBtn.onClick.AddListener(() => customSystem.ClearBtn());
        resetBtn.onClick.AddListener(() => customSystem.ResetBtn());
    }
}
