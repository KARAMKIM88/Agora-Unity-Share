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
    
    string Token = "0061999b7231aa94b50b6a3905974300f05IAAbbCgTiC79R/7nMJAGBkOxMOUkWVcAcD9pt8bgPXCYsYamEDYAAAAAEAAb78m5BkkvYgEAAQCWxy9i";
                       

    Texture2D mTexture;
    Rect mRect;
    int cnt = 100;
    bool isShare = false;


    RawImage myImage;
    RawImage remoteImage;

    IRtcEngine mRtcEngine;
    Metadata metaData;
    MetadataObserver mtObserver;
    Text AppIDText;
    Text ChannelText;
    Text DebugMsg;
    byte[] string_screenshare = Encoding.UTF8.GetBytes("screen sharing");
    int stream_id;
    DataStreamConfig msgConfig;

    
    void Awake()
    {
        SetupUI();
    }
    void SetupUI()
    {
        myImage = GameObject.Find("MyView").GetComponent<RawImage>();
        myImage.gameObject.AddComponent<VideoSurface>();

        


        GameObject go = GameObject.Find("LeaveButton");
        go?.GetComponent<Button>()?.onClick.AddListener(Leave);
        go = GameObject.Find("JoinButton");
        go?.GetComponent<Button>()?.onClick.AddListener(Join);
        DebugMsg = GameObject.Find("DebugMsg").GetComponent<Text>();
        go = GameObject.Find("ShareButton");
        go?.GetComponent<Button>()?.onClick.AddListener(Share);

        AppIDText = GameObject.Find("AppID").GetComponent<Text>();
        ChannelText = GameObject.Find("Channel").GetComponent<Text>();


    }

    void SetupScreenShare()
    {
        mRect = new Rect(0, 0, Screen.width, Screen.height);
        // Creates a texture of the rectangle you create.
        mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGBA32, false);

        
    }



    void Start()
    {
        SetupAgora();
        
    }

    private void Update()
    {
       // if(isShare)
       //     StartCoroutine(shareScreen());
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

    void Join()
    {
        AppIDText.text = "My AppID : " + AppID;
        ChannelText.text = "My Channel ID : " + ChannelName;
        
        DebugMsg.text = "Join Pressed. Channel Name is " + ChannelName;

        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();

        myImage.GetComponent<VideoSurface>().SetEnable(true);
        mRtcEngine.EnableDualStreamMode(true);
        //myView.SetEnable(true);

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
        DebugMsg.text = "Joined channel" + channelName + "my uid = " + uid;
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

            //myImage.GetComponent<RectTransform>().position = new Vector2(0, 0);

    }

    void OnStreamMessageHandler(uint userId, int streamId, byte[] data, int length)
    {
        DebugMsg.text = "[KKR]OnStreamMessageHandler";
        myImage = GameObject.Find("MyView").GetComponent<RawImage>();
        remoteImage = GameObject.Find("RemoteView").GetComponent<RawImage>();
        remoteImage.GetComponent<RectTransform>().sizeDelta = new Vector2(640, 480);


    }


}
