# APK 版本管理

包名: `com.xrobotoolkit.client`

## 最新版本 v1.4

### 下载

从 [GitHub Releases](https://github.com/lzhu686/XRoboToolkit-Unity-Client/releases) 下载:

```bash
wget https://github.com/lzhu686/XRoboToolkit-Unity-Client/releases/download/v1.4/XRoboToolkit-v1.4.apk
adb install -r -g XRoboToolkit-v1.4.apk
```

> APK 文件较大（~35MB），不纳入 git 仓库，请从 Releases 页面下载。

### v1.4 更新内容

- MediaDecoder ACK 握手协议（防止 Broken Pipe）
- stereoOffset 可配置（双目相机设为 0.0，消除 8% 内容裁剪）
- contentRatio 修正为 2.276（= 1/visibleRatio，100% 内容可见）

## 常用命令

```bash
# 安装
adb install -r -g XRoboToolkit-v1.4.apk

# 卸载
adb uninstall com.xrobotoolkit.client

# 清除应用数据（不卸载）
adb shell pm clear com.xrobotoolkit.client
```

---

> Generated with [Claude Code](https://claude.ai/code)
