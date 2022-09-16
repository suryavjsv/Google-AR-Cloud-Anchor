using DilmerGames.Core.Singletons;
using Google.XR.ARCoreExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

public class AnchorCreatedEvent : UnityEvent<Transform>{} //have to  send info to placement manager that a AR object is placed

public class ARCloudAnchorManager : Singleton<ARCloudAnchorManager>
{
    [SerializeField]
    private Camera arCamera = null;

    [SerializeField]
    private float resolveAnchorPassedTimeout = 10.0f; ///Manually set a time to get response from google

    private ARAnchorManager arAnchorManager = null; //component to create cloud anchors

    private ARAnchor pendingHostAnchor = null; //component to queuing the anchors

    private ARCloudAnchor cloudAnchor = null; //ref. that used to place anchors by calling google api

    private string anchorToResolve; //anchor id that i wanted to resolve

    //bools used in update method
    private bool anchorHostInProgress = false; //try to update the anchor

    private bool anchorResolveInProgress = false; //try to resolve the anchor
    
    private float safeToResolvePassed = 0; //checking the @resolveAnchorPassedTimeout value to keep track 
    //--
    private AnchorCreatedEvent cloudanchorCreatedEvent = null; //inherting from UnityResolver Class and Event we created

    private void Awake() 
    {
        cloudanchorCreatedEvent = new AnchorCreatedEvent();   
        cloudanchorCreatedEvent.AddListener((t) => ARPlacementManager.Instance.ReCreatePlacement(t));
    }

    //ARCore is creating feature map when we place cloud anchor.
    //this will ensure the map is high quality or lowquality. if low, the anchors isn't able to resolve
    private Pose GetCameraPose()
    {
        return new Pose(arCamera.transform.position,
            arCamera.transform.rotation);
    }

//Anchor Components
#region Anchor Cycle 

    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchor = arAnchor; //setting the reference for placing AR object
    }

    public void HostAnchor()
    {
        ARDebugManager.Instance.LogInfo($"HostAnchor executing");

        //recommended up to 30 seconds of scanning before calling host anchor
        //Quality of featurepoints from camera
        FeatureMapQuality quality =
            arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose()); 

        ARDebugManager.Instance.LogInfo($"Feature Map Quality is: {quality} ");

        //Hosting the anchor, passing the pending anchor
        cloudAnchor = arAnchorManager.HostCloudAnchor(pendingHostAnchor, 1); //second parameter is denoting how many days your anchor is going to be in that place
    
        //checking cloud anchor is working or not
        if(cloudAnchor == null)
        {
            ARDebugManager.Instance.LogError("Unable to host cloud anchor");
        }
        else
        {
            anchorHostInProgress = true;
        }
    }
    
    public void Resolve()
    {
        ARDebugManager.Instance.LogInfo("Resolve executing");

        //Resolving cloud anchor, passing the anchor(Active) to be resolved
        cloudAnchor = arAnchorManager.ResolveCloudAnchorId(anchorToResolve);

        //checking whether the anchor is resolve or not
        if(cloudAnchor == null)
        {
            ARDebugManager.Instance.LogError($"Failed to resolve cloud achor id {cloudAnchor.cloudAnchorId}");
        }
        else
        {
            anchorResolveInProgress = true;
        }
    }

    private void CheckHostingProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState; //Whether the cloud anchor is completed or not
        
        //checking the state of anchor that we're going to host
        if(cloudAnchorState == CloudAnchorState.Success)
        {
            ARDebugManager.Instance.LogError("Anchor successfully hosted");
            
            anchorHostInProgress = false;

            // keep track of cloud anchors added
            anchorToResolve = cloudAnchor.cloudAnchorId;
        }
        else if(cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            ARDebugManager.Instance.LogError($"Fail to host anchor with state: {cloudAnchorState}");
            anchorHostInProgress = false;
        }
    }

    private void CheckResolveProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        
        ARDebugManager.Instance.LogInfo($"ResolveCloudAnchor state {cloudAnchorState}");
        
        //checking the state of anchor that we're gnoing to resolve
        if (cloudAnchorState == CloudAnchorState.Success)
        {
            ARDebugManager.Instance.LogInfo($"CloudAnchorId: {cloudAnchor.cloudAnchorId} resolved");

            //Unity event for next placement if prev is resolved
            cloudanchorCreatedEvent.Invoke(cloudAnchor.transform);

            anchorResolveInProgress = false;
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            ARDebugManager.Instance.LogError($"Fail to resolve Cloud Anchor with state: {cloudAnchorState}");

            anchorResolveInProgress = false;
        }
    }

#endregion

    void Update()
    {
        // check progress of new anchors created
        if(anchorHostInProgress)
        {
            CheckHostingProgress();
            return;
        }

        if(anchorResolveInProgress && safeToResolvePassed <= 0)
        {
            // check evey (resolveAnchorPassedTimeout)
            safeToResolvePassed = resolveAnchorPassedTimeout;

            if(!string.IsNullOrEmpty(anchorToResolve))
            {
                ARDebugManager.Instance.LogInfo($"Resolving AnchorId: {anchorToResolve}");
                CheckResolveProgress();
            }
        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }
    }
}
