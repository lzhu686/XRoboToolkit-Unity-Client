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

    public IEnumerator OnStartListen(int port)
    {
        Debug.Log("StartListen port:" + port);

        _texture = new Texture2D(_resolutionWidth, _resolutionHeight, TextureFormat.RGB24, false, false);
        RemoteCameraImage.texture = _texture;
        yield return null;

        MediaDecoder.initialize((int)_texture.GetNativeTexturePtr(), _resolutionWidth, _resolutionHeight);
        MediaDecoder.startServer(port, false);
        yield return null;

        // 注意：不再在这里发送 StartReceivePcCamera
        // 由 UICameraCtrl.RequestCameraStreamCoroutine 统一发送 OPEN_CAMERA 命令
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