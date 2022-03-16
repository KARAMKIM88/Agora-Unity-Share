using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using agora_gaming_rtc;
using agora_utilities;
using System.Globalization;
using System.Runtime.InteropServices;
using System;
using System.Text;

public class NaiveComm : MonoBehaviour
{

    [SerializeField]
    //1999b7231aa94b50b6a3905974300f05
    string AppID = "1999b7231aa94b50b6a3905974300f05";
    
    [SerializeField]
    string ChannelName = "hello";
    
    string Token = "";
                       

    Texture2D mTexture;
    Rect mRect;
    int cnt = 100;
    bool isMyShare = false;
    bool isOtherShare = false;


    RawImage myImage;
    RawImage remoteImage;

    IRtcEngine mRtcEngine;
    Text DebugMsg;
    byte[] string_screenshare = Encoding.UTF8.GetBytes("screen sharing");
    byte[] string_camon = Encoding.UTF8.GetBytes("cam on");
    int stream_id;
    DataStreamConfig msgConfig;
    VideoEncoderConfiguration config;

    [SerializeField]

    public Vector3 my_share_pt;
    public Vector3 other_share_pt;



    void Awake()
    {
        SetupUI();
    }
    void SetupUI()
    {
        Screen.SetResolution(1280, 720, false);
        
        myImage = GameObject.Find("MyView").GetComponent<RawImage>();
        myImage.gameObject.AddComponent<VideoSurface>();

        GameObject go = GameObject.Find("LeaveButton");
        go?.GetComponent<Button>()?.onClick.AddListener(Leave);
        DebugMsg = GameObject.Find("DebugMsg").GetComponent<Text>();

        go = GameObject.Find("ShareButton");
        go?.GetComponent<Button>()?.onClick.AddListener(Share);
        go = GameObject.Find("QuitButton");
        go.GetComponent<Button>().onClick.AddListener(Quit);
        go = GameObject.Find("CamButton");
        go.GetComponent<Button>().onClick.AddListener(CameraOn);
      


    }

    void CameraOn()
    {
        if(isOtherShare)
        {
            Debug.Log("isOtherShare");
        }

        if(isMyShare)
        {
            mRtcEngine.StopScreenCapture();
            mRtcEngine.SendStreamMessage(stream_id, string_camon);
            myImage = GameObject.Find("MyView").GetComponent<RawImage>();

            myImage.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 320);
            myImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, -30, 0);

            remoteImage = GameObject.Find("RemoteView").GetComponent<RawImage>();
            remoteImage.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 320);
            remoteImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(300, -30, 0);
        }

    }

    private void Quit()
    {
        OnApplicationQuit();
        Application.Quit();

    }

    void SetupScreenShare()
    {
        mRect = new Rect(0, 0, Screen.width, Screen.height);
        // Creates a texture of the rectangle you create.
        mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGBA32, false);

        
    }



    void Start()
    {
        SetupVideoConfig();
        SetupAgora();
        Join();



    }

    private void Update()
    {
      
    }

    void SetupAgora()
    {
        msgConfig = new DataStreamConfig();
        msgConfig.syncWithAudio = false;
        msgConfig.ordered = false;

        DebugMsg.text = "[Naive] Setup Agora";
        mRtcEngine = IRtcEngine.GetEngine(AppID);

        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;
        mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;
        mRtcEngine.OnStreamMessage = OnStreamMessageHandler;
        

        
    }

     void SetupVideoConfig()
    {
        config = new VideoEncoderConfiguration();
        config.dimensions.width = 1280;
        config.dimensions.height = 720;
        // Sets the video frame rate.
        config.frameRate = FRAME_RATE.FRAME_RATE_FPS_30;
        // Sets the video encoding bitrate (Kbps).
        config.bitrate = 800;
        // Sets the adaptive orientation mode. See the description in API Reference.
        config.orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE;
        // Sets the video encoding degradation preference under limited bandwidth. MIANTAIN_QUALITY means to degrade the frame rate to maintain the video quality.
        config.degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_QUALITY;
        // Sets the video encoder configuration.
        

    }



    void Join()
    {
              
        DebugMsg.text = "Join Pressed. Channel Name is " + ChannelName;
        
       

        mRtcEngine.SetVideoEncoderConfiguration(config);
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();
        myImage = GameObject.Find("MyView").GetComponent<RawImage>();
        myImage.gameObject.GetComponent<VideoSurface>().SetEnable(true);

        mRtcEngine.JoinChannelByKey(Token, ChannelName, "", 0);
    }

    void Share()
    {
        //mRtcEngine.DisableVideoObserver();
        //mRtcEngine.DisableVideo();
        //myView.SetEnable(false);


        // myView.SetEnable(false);
        //remoteView.SetEnable(false);

        //mRtcEngine.SetExternalVideoSource(true, false);
        //isShare = true;
        //DebugMsg.text = "Start to Screen Share";
        isMyShare = true;
        isOtherShare = false;



        TestRectCrop(1);
    }

    void Leave()
    {
        DebugMsg.text = "Pressed Leave()";
        
        mRtcEngine.DisableVideo();
        mRtcEngine.DisableVideoObserver();
        mRtcEngine.LeaveChannel();
        OnApplicationQuit();
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        DebugMsg.text = "Joined channel : " + channelName + " my uid : " + uid;
        //Debug.LogFormat("Joined channel {0} successful, my uid = {1}", channelName, uid);
        //Debug.Log("Join channel : " + channelName + " my uid : " + uid);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        myImage.GetComponent<VideoSurface>().SetEnable(false);
        
        if (remoteImage.GetComponent<VideoSurface>() != null)
        {
            remoteImage.GetComponent<VideoSurface>().SetEnable(false);
        }
    }

    void OnUserJoined(uint uid, int elapsed)
    {

        

        remoteImage = GameObject.Find("RemoteView").GetComponent<RawImage>();
        remoteImage.gameObject.AddComponent<VideoSurface>();


        VideoSurface remoteView = remoteImage.GetComponent<VideoSurface>();

        if (remoteView == null)
        {
            DebugMsg.text = "remoteView is null";
            remoteImage.gameObject.AddComponent<VideoSurface>();

        }
        remoteView = remoteImage.GetComponent<VideoSurface>();


        remoteView.SetForUser(uid);
        remoteView.SetEnable(true);
        remoteView.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        remoteView.SetGameFps(30);

        DebugMsg.text = "channel joined - uid : " + uid;


    }

    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        remoteImage.GetComponent<VideoSurface>().SetEnable(false);
    }

    void OnApplicationQuit()
    {
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }


    void TestRectCrop(int order)
    {
        // Assuming you have two display monitors, each of 1920x1080, position left to right:
        Rectangle screenRect = new Rectangle() { x = 0, y = 0, width = 1920 , height = 1080 };
        Rectangle regionRect = new Rectangle() { x = 0, y = 0, width = 1920, height = 1080 };

        int rc = mRtcEngine.StartScreenCaptureByScreenRect(screenRect,
            regionRect,
            default(ScreenCaptureParameters)
            );
        if (rc != 0) Debug.LogWarning("rc = " + rc);

        stream_id = mRtcEngine.CreateDataStream(msgConfig);


        myImage = GameObject.Find("MyView").GetComponent<RawImage>();
        remoteImage = GameObject.Find("RemoteView").GetComponent<RawImage>();
        

        mRtcEngine.SendStreamMessage(stream_id, string_screenshare);
        myImage.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
        
        my_share_pt = new Vector3(-300, 0, 0);
        myImage.GetComponent<RectTransform>().anchoredPosition = my_share_pt;
        remoteImage.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 160);
        remoteImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(230, 0, 0);

    }

    void OnStreamMessageHandler(uint userId, int streamId, byte[] data, int length)
    {
        byte[] byteMsg = new byte[length];
        Buffer.BlockCopy(data, 0, byteMsg, 0, length);
        string stringMsg = Encoding.Default.GetString(byteMsg);
        DebugMsg.text = stringMsg;

        if (stringMsg == Encoding.Default.GetString(string_camon)) //change code
        {
            DebugMsg.text = "[KKR]Camera On";
            myImage = GameObject.Find("MyView").GetComponent<RawImage>();
            remoteImage = GameObject.Find("RemoteView").GetComponent<RawImage>();
            remoteImage.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 320);
            remoteImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(300, -30, 151);
            myImage.GetComponent<RectTransform>().sizeDelta = new Vector2(480, 320);
            myImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, 0, 0);

        }
        else
        {
            DebugMsg.text = "[KKR]OnStreamMessageHandler";
            myImage = GameObject.Find("MyView").GetComponent<RawImage>();
            remoteImage = GameObject.Find("RemoteView").GetComponent<RawImage>();
            remoteImage.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);
            remoteImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(300, -30, 151);
            myImage.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 160);
            myImage.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, 0, 0);

        }







        if (isMyShare == true)
        {
            mRtcEngine.StopScreenCapture();
            isMyShare = false;
        }

        isOtherShare = true;

    }


}
