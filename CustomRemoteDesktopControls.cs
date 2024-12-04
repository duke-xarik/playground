using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class CustomRemoteDesktopControls : MonoBehaviour
{
    public UnityEvent<int> OnInputGetKeyEvent = new UnityEvent<int>();
    public UnityEvent OnInputClickEvent = new UnityEvent();
    [SerializeField] private int eventInt = 0;
    private float inputDownTime = 0f;
    private float currentTime = 0f;
    private float onClickedThreshold = 0.25f;
    private bool onClicked = false;

    private LineRenderer _lineRenderer;

    // we need to track collisionEnter and collisionStay separately since the can happen on the same frame.  Stay can overwrite Enter.
    private bool _collisionEnter = false;
    private bool _collisionStay = false;
    private bool _collisionExit = false;

    [SerializeField] private Material _defaultLine;
    [SerializeField] private Material _triggerEnterLine;
    [SerializeField] private Material _triggerStayLine;
    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        currentTime = Time.realtimeSinceStartup;

        int _eventInt = 0;
        if (_collisionEnter)
        {
            _eventInt = 1;
            inputDownTime = Time.realtimeSinceStartup;

            // something is broken with click-drag.  The initial position is getting the position from the previous click-drag for some reason.
            //var _control = _manager.GetFMRemoteDesktopControl();
            //_control.GetLastClickedDesktopViewer.RaycastPointLastClicked = _control.GetLastClickedDesktopViewer.RaycastPoint;
            //_control.XarikSetRemoteMouseLastClick(_control.GetLastClickedDesktopViewer.RaycastPointLastClicked);
        }
        else if (_collisionExit)
        {
            _eventInt = 2;
            // if you touch and release quickly enough, it counts as a 'click'
            if (currentTime - inputDownTime <= onClickedThreshold) onClicked = true;
        }
        else if (_collisionStay)
        {
            //_eventInt = 3;
            //don't invoke drag signal within the threshold of a "click" - this helps to fire an eventInt = 1 & eventInt = 0 before firing eventInt = 3
            if (currentTime - inputDownTime > onClickedThreshold) _eventInt = 3;
        }

        //down(1) and up(2) only trigger once, while drag(3) can be triggered every frame
        if (eventInt != _eventInt || _eventInt == 3)
        {
            eventInt = _eventInt;
            OnInputGetKeyEvent.Invoke(eventInt);
        }
        
        if (onClicked)
        {
            onClicked = false;
            OnInputClickEvent.Invoke();
        }
    }

    private void OnDisable()
    {
        eventInt = 0;
        OnInputGetKeyEvent.Invoke(eventInt);
    }


    private void UpdateLineRendererMaterial(Material newMaterial)
    {
        // Get the current materials array
        Material[] materials = _lineRenderer.materials;

        // Check if there is at least one material assigned
        if (materials.Length > 0)
        {
            // Change the material at Element 0
            materials[0] = newMaterial;

            // Apply the updated materials array back to the LineRenderer
            _lineRenderer.materials = materials;
        }
        else
        {
            Debug.LogWarning("No materials found on the Line Renderer!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "TouchCollider")
        {
            Debug.Log("CollisionEnter with FMRemoteDesktopViewer component.");
            _collisionEnter = true;
            UpdateLineRendererMaterial(_triggerEnterLine);
            StartCoroutine(ResetCollisionEnter());
        } 
    }

    private IEnumerator ResetCollisionEnter()
    {
        yield return new WaitForEndOfFrame();
        _collisionEnter = false;
        _collisionStay = true;
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.name == "TouchCollider")
    //    {
    //        Debug.Log("CollisionStay with FMRemoteDesktopViewer component.");
    //        _collisionStay = true;
    //        UpdateLineRendererMaterial(_triggerStayLine);
    //    }
    //}

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "TouchCollider")
        {
            Debug.Log("CollisionExit with FMRemoteDesktopViewer component.");
            _collisionExit = true;
            //_collisionEnter = false;
            _collisionStay = false;
            UpdateLineRendererMaterial(_defaultLine);
            StartCoroutine(ResetCollisionExit());
        }
    }

    private IEnumerator ResetCollisionExit()
    {
        yield return new WaitForEndOfFrame();
        _collisionExit = false;
    }
}
