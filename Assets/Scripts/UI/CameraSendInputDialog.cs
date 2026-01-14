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

    /// <summary>
    /// 当前视频源名称（ADB、WIFI 等），用于生成独立的 PlayerPrefs key
    /// </summary>
    private string _currentVideoSourceName;

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

    /// <summary>
    /// 生成当前视频源对应的 PlayerPrefs key
    /// </summary>
    private string GetPrefsKey()
    {
        return $"CameraSendInputDialog_{_currentVideoSourceName}";
    }

    /// <summary>
    /// 生成视频源对应的配置默认值 key
    /// </summary>
    private string GetDefaultIPConfigKey()
    {
        return $"{_currentVideoSourceName}.DefaultIP";
    }

    public void Show(Action<string> onConfirmCall)
    {
        // 获取当前视频源名称
        _currentVideoSourceName = VideoSourceConfigManager.Instance?.CurrentVideoSource?.name ?? "ADB";

        // 优先使用历史记录，如果没有则使用配置文件中的默认 IP
        string savedIP = PlayerPrefs.GetString(GetPrefsKey(), "");
        if (string.IsNullOrEmpty(savedIP))
        {
            // 从配置文件获取该视频源的默认 IP
            savedIP = VideoSourceConfigManager.Instance?.GetStringProperty(GetDefaultIPConfigKey()) ?? "127.0.0.1";
        }
        TmpInput.text = savedIP;
        OnConfirmCall = onConfirmCall;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 切换视频源时更新默认 IP（仅当用户没有自定义 IP 时才更新）
    /// </summary>
    public void UpdateDefaultIP()
    {
        // 获取当前视频源名称
        _currentVideoSourceName = VideoSourceConfigManager.Instance?.CurrentVideoSource?.name ?? "ADB";

        // 检查用户是否有自定义保存的 IP
        string savedIP = PlayerPrefs.GetString(GetPrefsKey(), "");

        // 如果用户已有保存的 IP，保留用户的选择，不覆盖
        if (!string.IsNullOrEmpty(savedIP))
        {
            TmpInput.text = savedIP;
            return;
        }

        // 用户没有自定义 IP，使用配置文件默认值
        string defaultIP = VideoSourceConfigManager.Instance?.GetStringProperty(GetDefaultIPConfigKey()) ?? "127.0.0.1";
        TmpInput.text = defaultIP;
    }

    /// <summary>
    /// 强制重置为配置文件的默认 IP（清除当前视频源的用户历史）
    /// </summary>
    public void ResetToDefaultIP()
    {
        _currentVideoSourceName = VideoSourceConfigManager.Instance?.CurrentVideoSource?.name ?? "ADB";
        string defaultIP = VideoSourceConfigManager.Instance?.GetStringProperty(GetDefaultIPConfigKey()) ?? "127.0.0.1";
        TmpInput.text = defaultIP;
        PlayerPrefs.SetString(GetPrefsKey(), defaultIP);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 清除当前视频源的用户保存 IP 历史
    /// </summary>
    public void ClearSavedIP()
    {
        _currentVideoSourceName = VideoSourceConfigManager.Instance?.CurrentVideoSource?.name ?? "ADB";
        PlayerPrefs.DeleteKey(GetPrefsKey());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 清除所有视频源的保存 IP 历史
    /// </summary>
    public void ClearAllSavedIPs()
    {
        PlayerPrefs.DeleteKey("CameraSendInputDialog_ADB");
        PlayerPrefs.DeleteKey("CameraSendInputDialog_WIFI");
        // 兼容旧版本的 key
        PlayerPrefs.DeleteKey("CameraSendInputDialog");
        PlayerPrefs.Save();
    }

    private void OnConfirmBtn()
    {
        // 使用当前视频源对应的 key 保存用户输入的 IP
        PlayerPrefs.SetString(GetPrefsKey(), TmpInput.text);
        PlayerPrefs.Save();  // 确保立即写入磁盘

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
