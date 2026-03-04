using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetLERE : MonoBehaviour
{

    public GameObject CanvLE;
    public GameObject CanvRE;
    public RemoteCameraWindow remoteCameraWindow;
    public Material matLE;

    public Material matRE;

    // 16:9 精确适配 (每只眼 1280x720)
    // Canvas 是 Screen Space - Camera 模式，自动填满视口（正方形）
    // 宽高比由 shader 的 heightCompressionFactor 处理
    // 公式: contentRatio = 1 / visibleRatio (无放大条件)
    private float visibleRatio = 1.0f;
    private float contentRatio = 1.0f;
    private float heightCompressionFactor = 1.777778f; // 16:9
    private float stereoOffset = 0.0f; // 双目相机自带视差，默认0

    public void UpdateParameters(float visible, float content, float heightCompression, float offset = 0.0f)
    {
        visibleRatio = visible;
        contentRatio = content;
        heightCompressionFactor = heightCompression;
        stereoOffset = offset;

        Debug.Log($"Updated Ratios - visible: {visibleRatio}, content: {contentRatio}, " +
                  $"heightCompression: {heightCompressionFactor}, stereoOffset: {stereoOffset}");
    }

    public void ResetCanvases()
    {
        CanvLE.SetActive(false);
        CanvRE.SetActive(false);
    }

    void Update()
    {
        if ((!CanvLE.activeSelf) || (!CanvRE.activeSelf))
        {
            CanvLE.SetActive(true);
            CanvRE.SetActive(true);

            matLE.SetTexture("_mainRT", remoteCameraWindow.Texture);
            matRE.SetTexture("_mainRT", remoteCameraWindow.Texture);

            matLE.SetInt("_isLE", 1);
            matRE.SetInt("_isLE", 0);

            matLE.SetFloat("_visibleRatio", visibleRatio);
            matRE.SetFloat("_visibleRatio", visibleRatio);
            matLE.SetFloat("_contentRatio", contentRatio);
            matRE.SetFloat("_contentRatio", contentRatio);
            matLE.SetFloat("_heightCompressionFactor", heightCompressionFactor);
            matRE.SetFloat("_heightCompressionFactor", heightCompressionFactor);
            matLE.SetFloat("_stereoOffset", stereoOffset);
            matRE.SetFloat("_stereoOffset", stereoOffset);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            visibleRatio += 0.005f;
            matLE.SetFloat("_visibleRatio", visibleRatio);
            matRE.SetFloat("_visibleRatio", visibleRatio);
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            visibleRatio -= 0.005f;
            matLE.SetFloat("_visibleRatio", visibleRatio);
            matRE.SetFloat("_visibleRatio", visibleRatio);
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            contentRatio += 0.005f;
            matLE.SetFloat("_contentRatio", contentRatio);
            matRE.SetFloat("_contentRatio", contentRatio);
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            contentRatio -= 0.005f;
            matLE.SetFloat("_contentRatio", contentRatio);
            matRE.SetFloat("_contentRatio", contentRatio);
            Debug.Log($"visibleRatio: {visibleRatio} - contentRatio: {contentRatio}");
        }
    }
}
