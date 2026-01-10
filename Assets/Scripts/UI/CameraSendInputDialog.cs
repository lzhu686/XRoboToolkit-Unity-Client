using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraSendInputDialog : MonoBehaviour
{
    public TMP_InputField TmpInput;
    //   public TMP_Dropdown renderModeDropdown;

    public Action<string> OnConfirmCall;

    public Button CloseBtn;
    public Button ConfirmBtn;

    private void Awake()
    {
        CloseBtn.onClick.AddListener(OnCloseBtn);
        ConfirmBtn.onClick.AddListener(OnConfirmBtn);
    }

    private void OnDestroy()
    {
        CloseBtn.onClick.RemoveListener(OnCloseBtn);
        ConfirmBtn.onClick.RemoveListener(OnConfirmBtn);
    }

    public void Show(Action<string> onConfirmCall)
    {
        // 优先使用历史记录，如果没有则使用配置文件中的默认 IP
        string savedIP = PlayerPrefs.GetString("CameraSendInputDialog", "");
        if (string.IsNullOrEmpty(savedIP))
        {
            savedIP = VideoSourceConfigManager.Instance?.DefaultIP ?? "127.0.0.1";
        }
        TmpInput.text = savedIP;
        OnConfirmCall = onConfirmCall;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 切换视频源时更新默认 IP
    /// </summary>
    public void UpdateDefaultIP()
    {
        string defaultIP = VideoSourceConfigManager.Instance?.DefaultIP ?? "127.0.0.1";
        TmpInput.text = defaultIP;
        PlayerPrefs.SetString("CameraSendInputDialog", defaultIP);
    }

    private void OnConfirmBtn()
    {
        PlayerPrefs.SetString("CameraSendInputDialog", TmpInput.text);
        if (OnConfirmCall != null)
        {
            OnConfirmCall(TmpInput.text);
        }

        gameObject.SetActive(false);
    }

    private void OnCloseBtn()
    {
        gameObject.SetActive(false);
    }
}