using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    Transform gridBtnPanel;
    Transform gameModePanel;

    Button runPauseBtn;
    Button stopBtn;
    Button clearLineBtn;
    Button clearAllBtn;

    TextMeshProUGUI gameModeText;
    TextMeshProUGUI runPauseBtnText;
    CustomSystem customSystem;
    private void Awake()
    {
        customSystem = FindObjectOfType<CustomSystem>();
        gridBtnPanel = transform.Find("gridBtnPanel");
        gameModePanel = transform.Find("gameModePanel");

        runPauseBtn = gridBtnPanel.Find("runPauseBtn").GetComponent<Button>();
        runPauseBtnText = runPauseBtn.GetComponentInChildren<TextMeshProUGUI>();

        stopBtn = gridBtnPanel.Find("stopBtn").GetComponent<Button>();
        clearAllBtn = gridBtnPanel.Find("clearAllBtn").GetComponent<Button>();
        clearLineBtn = gridBtnPanel.Find("clearLineBtn").GetComponent<Button>();

        gameModeText = gameModePanel.Find("gameModeText").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        runPauseBtn.onClick.AddListener(() => customSystem.RunPauseBtn());
        stopBtn.onClick.AddListener(() => customSystem.StopBtn());
        clearAllBtn.onClick.AddListener(() => customSystem.ClearAllBtn());
        clearLineBtn.onClick.AddListener(() => customSystem.ClearLineBtn());

        runPauseBtnText.text = "Run";
    }

    public void SwitchGameModeToRun()
    {
        gameModeText.text = "Run Mode";
        runPauseBtnText.text = "Pause";
    }

    public void SwitchGameModeToEdit()
    {
        gameModeText.text = "Edit Mode";
        runPauseBtnText.text = "Run";
    }

    public void SwitchGameModeToPause()
    {
        gameModeText.text = "Pause Mode";
        runPauseBtnText.text = "Run";
    }
}
