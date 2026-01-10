using System.Collections;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LitJson;
using Network;
using Robot;
using UnityEngine.UI;


/// <summary>
/// Display window of PC camera
/// Responsible for receiving, decoding, and displaying data
///
/// 生命周期管理:
/// - OnStartListen: 初始化 MediaDecoder，发送 OPEN_CAMERA 命令
/// - OnCloseBtn: 发送 CLOSE_CAMERA 命令，然后释放资源
/// - OnDisable: 仅释放 MediaDecoder 资源（避免重复发送关闭命令）
/// </summary>
public class RemoteCameraWindow : MonoBehaviour
{
    public RawImage RemoteCameraImage;
    private TcpListener _tcpListener;
    private TcpClient _client;
    private NetworkStream _stream;
    private Texture2D _texture;
    public Texture2D Texture => _texture;
    private byte[] _imageBuffer;
    private CancellationTokenSource _receiveImageTs = null;
    private Task _imageReceiveTask;

    private int _resolutionWidth = 2160;
    private int _resolutionHeight = 2160 / 2 * 4 / 3;
    private int _videoFps = 60;
    private int _bitrate = 40 * 1024 * 1024;

    public CustomButton listenBtn;

    // 标记是否已发送关闭命令，避免重复发送
    private bool _closeCameraSent = false;

    private void Awake()
    {
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;
    }

    private void OnEnable()
    {
        // 重置状态
        _closeCameraSent = false;
    }

    public void StartListen(int width, int height, int fps, int bitrate, int port)
    {
        _resolutionWidth = width;
        _resolutionHeight = height;
        _videoFps = fps;
        _bitrate = bitrate;

        StartCoroutine(OnStartListen(port));
    }

    private void OnDisable()
    {
        Debug.Log("RemoteCameraWindow OnDisable");

        // 仅释放 MediaDecoder 资源
        MediaDecoder.release();

        // 注意：不在这里发送关闭命令
        // 关闭命令统一由 OnCloseBtn 发送，避免重复
    }

    public void OnCloseBtn()
    {
        // Reset listen button
        listenBtn.SetOn(false);

        // 发送关闭命令（仅发送一次）
        if (!_closeCameraSent)
        {
            _closeCameraSent = true;
            NetworkCommander.Instance.CloseCamera();
        }

        gameObject.SetActive(false);
    }

    // 保存监听端口，供外部调用 NotifyMediaDecoderReady 使用
    private int _listeningPort = 12345;

    public IEnumerator OnStartListen(int port)
    {
        Debug.Log("StartListen port:" + port);
        _listeningPort = port;

        _texture = new Texture2D(_resolutionWidth, _resolutionHeight, TextureFormat.RGB24, false, false);
        RemoteCameraImage.texture = _texture;
        yield return null;

        MediaDecoder.initialize((int)_texture.GetNativeTexturePtr(), _resolutionWidth, _resolutionHeight);
        MediaDecoder.startServer(port, false);

        // 等待 MediaDecoder 完成初始化
        // startServer 是异步的，需要给底层一些时间完成 TCP 监听的建立
        yield return new WaitForSeconds(0.5f);

        // 注意：MEDIA_DECODER_READY 不在这里发送！
        // 必须等 TcpClient 连接后，先发送 OPEN_CAMERA，再发送 MEDIA_DECODER_READY
        // 由 UICameraCtrl.RequestCameraStreamCoroutine 在正确时机调用 NotifyMediaDecoderReady()
        Debug.Log($"MediaDecoder initialized on port {port}, waiting for NotifyMediaDecoderReady call");
    }

    /// <summary>
    /// 通知服务器 MediaDecoder 已就绪
    /// 必须在 OPEN_CAMERA 发送之后调用
    /// </summary>
    public void NotifyMediaDecoderReady()
    {
        if (NetworkCommander.Instance != null)
        {
            NetworkCommander.Instance.MediaDecoderReady(_listeningPort);
            Debug.Log($"MediaDecoder ready signal sent for port {_listeningPort}");
        }
    }

    private void LateUpdate()
    {
        //Keep the window facing the camera at all times
        if (Camera.main != null)
        {
            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    private void Update()
    {
        if (_texture != null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (MediaDecoder.isUpdateFrame())
                {
                    MediaDecoder.updateTexture();
                    GL.InvalidateState();
                }
            }
        }
    }
}