using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using agora_gaming_rtc;
using agora_utilities;

public class ScreenShare : MonoBehaviour
{
    [SerializeField]
    //1999b7231aa94b50b6a3905974300f05
    string AppID = "1999b7231aa94b50b6a3905974300f05";
    string ChannelName = "hello";
    string Token = "0061999b7231aa94b50b6a3905974300f05IACrmdLdSQaM5ZG4mOVolCm4DTXPO9NUewUajAHisYfedIamEDYAAAAAEACYVs+Kiy4oYgEAAQCKLihi";
    //0061999b7231aa94b50b6a3905974300f05IADE5Pqmqfg9NmJA/ZJZr5G335EzX1kf3MTSeFhuhcmFl+JyGcsAAAAAEAAD1/lOXqQmYgEAAQBdpCZi                       

    VideoSurface myView;
    VideoSurface remoteView;
    VideoSurface screenView;
    IRtcEngine mRtcEngine;
    Text AppIDText, ChannelText;
    Text DebugMsg;
    public GameObject obj;
    InputField AppIDInput, ChannelNameInput, TokenInput;

    Texture2D mTexture;
    Rect mRect;
    bool running = false;
    MonoBehaviour monoProxy;



    //screen share ¶¼¾î³»±â

    void getUserInput()
    {
        AppIDInput = GameObject.Find("AppIDInput").GetComponent<InputField>();
        ChannelNameInput = GameObject.Find("ChannelNameInput").GetComponent<InputField>();
        TokenInput = GameObject.Find("TokenInput").GetComponent<InputField>();
        /*
        AppID = AppIDInput.text;
        ChannelName = ChannelNameInput.text;
        Token = TokenInput.text;
        */

    }

    void Awake()
    {
        SetupUI();
    }


    // Start is called before the first frame update
    void Start()
    {
        SetupAgora();

        DebugMsg = GameObject.Find("DebugMsg").GetComponent<Text>();
        AppIDText = GameObject.Find("AppID").GetComponent<Text>();
        ChannelText = GameObject.Find("Channel").GetComponent<Text>();


    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(ShareScreen());

    }
    void SetupUI()
    {
        GameObject go = GameObject.Find("MyView");
        myView = go.AddComponent<VideoSurface>();
        go = GameObject.Find("LeaveButton");
        go.GetComponent<Button>().onClick.AddListener(Leave);
        go = GameObject.Find("JoinButton");
        go.GetComponent<Button>().onClick.AddListener(Join);
        go = GameObject.Find("ShareButton");
        go.GetComponent<Button>().onClick.AddListener(StartSharing);
        go = GameObject.Find("ShareView");
        screenView = go.AddComponent<VideoSurface>();
        monoProxy = GameObject.Find("Canvas").GetComponent<MonoBehaviour>();

    }

    public void Join()
    {
        getUserInput();
        AppIDText.text = "My AppID : " + AppID;
        ChannelText.text = "My Channel ID : " + ChannelName;
        DebugMsg.text = "Start";
        Debug.Log("channel name " + ChannelName);
        DebugMsg.text = "Join Pressed. Channel Name " + ChannelName;
        Debug.Log("Join Pressed. Channel Name " + ChannelName);
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();


        PrepareScreenShare();
        myView.GetComponent<VideoSurface>().SetEnable(true);
        //mRtcEngine.JoinChannel(ChannelName, "", 0);
        mRtcEngine.JoinChannelByKey(Token, ChannelName, "", 0);

    }

    void PrepareScreenShare()
    {
        mRtcEngine.SetExternalVideoSource(true, false);
        // Live Broadcasting mode to allow many view only audience 

        mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        /*
        // This block of code push frame at full resolution, proving the support of 720p
        mRtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration()
        {
            bitrate = 1130,
            frameRate = FRAME_RATE.FRAME_RATE_FPS_15,
            dimensions = new VideoDimensions() { width = Screen.width, height = Screen.height },

            // Note if your remote user video surface to set to flip Horizontal, then we should flip it before sending
            mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED
        });
        */

        DebugMsg.text = "Sharing Screen with width " + Screen.width + " , height " + Screen.height;
    }

    public void StartSharing()
    {
        if (running == false)
        {
            DebugMsg.text = "running - true / start sharing";
            // Create a rectangle width and height of the screen
            mRect = new Rect(0, 0, Screen.width, Screen.height);
            // Create a texture the size of the rectangle you just created
            mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGBA32, false);
            // get the rtc engine instance, assume it has been created before this script starts
            running = true;

            monoProxy.StartCoroutine(ShareScreen());

        }
    }

    public void Leave()
    {
        StopSharing();
        Debug.Log("Leave pressed");
        mRtcEngine.LeaveChannel();
        mRtcEngine.DisableVideo();
        mRtcEngine.DisableVideoObserver();
        DebugMsg.text = "Leave pressed";
    }

    void SetupAgora()
    {
        mRtcEngine = IRtcEngine.GetEngine(AppID);
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);

        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccessHandler;

        mRtcEngine.OnLeaveChannel = OnLeaveChannelHandler;

        //mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        //DebugMsg.text = "Joined channel" + channelName + " successful, my uid = " + uid;
        // can add other logics here, for now just print to the log
        Debug.LogFormat("Joined channel {0} successful, my uid = {1}", channelName, uid);
        Debug.Log("Join channel : " + channelName + " my uid : " + uid);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        myView.GetComponent<VideoSurface>().SetEnable(false);
        if (remoteView != null)
        {
            remoteView.GetComponent<VideoSurface>().SetEnable(false);
        }
    }

    void OnUserJoined(uint uid, int elapsed)
    {
        DebugMsg.text = "another user is joined";
        Debug.Log("another user is joined");
        GameObject go = GameObject.Find("RemoteView");

        if (remoteView == null)
        {
            remoteView.gameObject.AddComponent<VideoSurface>();
        }

        Debug.Log("uid is : " + uid);

        remoteView.GetComponent<VideoSurface>().SetForUser(uid);
        remoteView.GetComponent<VideoSurface>().SetEnable(true);
        remoteView.GetComponent<VideoSurface>().SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
        remoteView.GetComponent<VideoSurface>().SetGameFps(30);
    }

    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        remoteView.SetEnable(false);
    }

    void OnApplicationQuit()
    {
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }

    IEnumerator ShareScreen()
    {
        //running = false;
        while (running)
        {
            yield return new WaitForEndOfFrame();
            //Read the Pixels inside the Rectangle
            mTexture.ReadPixels(mRect, 0, 0);
            //Apply the Pixels read from the rectangle to the texture
            mTexture.Apply();
            // Get the Raw Texture data from the the from the texture and apply it to an array of bytes
            byte[] bytes = mTexture.GetRawTextureData();
            // int size = Marshal.SizeOf(bytes[0]) * bytes.Length;
            // Check to see if there is an engine instance already created
            //if the engine is present
            if (mRtcEngine != null)
            {
                //Create a new external video frame
                ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
                //Set the buffer type of the video frame
                externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
                // Set the video pixel format
                //externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;  // V.2.9.x
                externalVideoFrame.format = ExternalVideoFrame.VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA;  // V.3.x.x
                //apply raw data you are pulling from the rectangle you created earlier to the video frame
                externalVideoFrame.buffer = bytes;
                //Set the width of the video frame (in pixels)
                externalVideoFrame.stride = (int)mRect.width;
                //Set the height of the video frame
                externalVideoFrame.height = (int)mRect.height;
                //Remove pixels from the sides of the frame
                externalVideoFrame.cropLeft = 10;
                externalVideoFrame.cropTop = 10;
                externalVideoFrame.cropRight = 10;
                externalVideoFrame.cropBottom = 10;
                //Rotate the video frame (0, 90, 180, or 270)
                externalVideoFrame.rotation = 180;
                //Push the external video frame with the frame we just created
                mRtcEngine.PushVideoFrame(externalVideoFrame);

                DebugMsg.text = "Running sharescreen()";
            }
        }
    }
    void StopSharing()
    {
        // set the boolean false will cause the shareScreen coRoutine to exit
        running = false;
    }
}
