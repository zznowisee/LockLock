using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    Transform gridBtnPanel;
    Transform gameModePanel;
    Transform receiverNamePanel;
    Transform receiverCodePanel;

    Button runPauseBtn;
    Button stopBtn;
    Button clearLineBtn;
    Button clearAllBtn;

    TextMeshProUGUI gameModeText;
    TextMeshProUGUI runPauseBtnText;
    CustomSystem customSystem;

    [SerializeField] TextMeshProUGUI pfReceiverNameText;
    [SerializeField] TextMeshProUGUI pfReceiverCodeText;

    Dictionary<int, TextMeshProUGUI> receiverNameTextDictionary;
    Dictionary<int, TextMeshProUGUI> receiverCodeTextDictionary;

    private void Awake()
    {
        receiverNameTextDictionary = new Dictionary<int, TextMeshProUGUI>();
        receiverCodeTextDictionary = new Dictionary<int, TextMeshProUGUI>();

        customSystem = FindObjectOfType<CustomSystem>();
        gridBtnPanel = transform.Find("gridBtnPanel");
        gameModePanel = transform.Find("gameModePanel");
        receiverNamePanel = transform.Find("receiverNamePanel");
        receiverCodePanel = transform.Find("receiverCodePanel");

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

        foreach(TextMeshProUGUI text in receiverCodeTextDictionary.Values)
        {
            text.text = "Empty";
        }
    }

    public void SwitchGameModeToPause()
    {
        gameModeText.text = "Pause Mode";
        runPauseBtnText.text = "Run";
    }

    public void AddNewReceiver(int receiverIndex)
    {
        TextMeshProUGUI receiverNameText = Instantiate(pfReceiverNameText, receiverNamePanel);
        receiverNameText.text = $"R0{receiverIndex}";
        receiverNameText.transform.SetSiblingIndex(receiverIndex);

        TextMeshProUGUI receiverCodeText = Instantiate(pfReceiverCodeText, receiverCodePanel);
        receiverCodeText.text = "Empty";
        receiverCodeText.transform.SetSiblingIndex(receiverIndex);

        receiverNameTextDictionary[receiverIndex] = receiverNameText;
        receiverCodeTextDictionary[receiverIndex] = receiverCodeText;
    }

    public void RemoveReceiver(int receiverIndex)
    {
        Destroy(receiverNameTextDictionary[receiverIndex].gameObject);
        Destroy(receiverCodeTextDictionary[receiverIndex].gameObject);
    }

    public void UpdateReceiverCode(string code, int receiverIndex)
    {
        receiverCodeTextDictionary[receiverIndex].text = code;
    }

    public void ClearAllReceiverCodes()
    {
        foreach (TextMeshProUGUI text in receiverCodeTextDictionary.Values)
        {
            Destroy(text.gameObject);
        }
        receiverCodeTextDictionary.Clear();
        foreach (TextMeshProUGUI text in receiverNameTextDictionary.Values)
        {
            Destroy(text.gameObject);
        }
        receiverNameTextDictionary.Clear();
    }
}
