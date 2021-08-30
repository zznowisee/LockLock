using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Transform btnPanel;

    Button startBtn;
    Button stopBtn;
    Button restartBtn;
    Button clearBtn;

    CustomSystem inputSystem;
    private void Awake()
    {
        inputSystem = FindObjectOfType<CustomSystem>();
        btnPanel = transform.Find("btnPanel");

        startBtn = btnPanel.Find("startBtn").GetComponent<Button>();
        stopBtn = btnPanel.Find("stopBtn").GetComponent<Button>();
        clearBtn = btnPanel.Find("clearBtn").GetComponent<Button>();
        restartBtn = btnPanel.Find("restartBtn").GetComponent<Button>();
    }

    private void Start()
    {
        startBtn.onClick.AddListener(() => inputSystem.Run());
        stopBtn.onClick.AddListener(() => inputSystem.Stop());
        clearBtn.onClick.AddListener(() => inputSystem.Clear());
        restartBtn.onClick.AddListener(() => inputSystem.Restart());
    }
}
