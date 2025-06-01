using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBehaviour : MonoBehaviour
{
    [Header("Scaling Properties")]

    [Tooltip("The minimum size (in unity units) that the player should be")]
    public float minScale = 0.5f;

    [Tooltip("The maximum size (in unity units) that the player should be")]
    public float maxScale = 3.0f;

    //current scale of the player
    private float currentScale = 1;

    public enum MobileHorizMovement
    {
        Accelerometer,
        ScreenTouch
    }

    [Tooltip("What horizontal movement should be used")]
    public MobileHorizMovement horizMovement = MobileHorizMovement.Accelerometer;

    [Header("Swipe Properties")]
    [Tooltip("How far will the player move upon swiping")]
    public float swipeMove = 2f;

    [Tooltip("How far must the player swipe before we will execute the action (in inches)")]
    public float minSwipeDistance = 0.25f;

    ///<summary>
    /// used to hold the value that converts
    /// minSwipeDistance to pixels
    /// </summary>
    private float minSwipeDistancePixels;

    // stores starting position of mobile touch events 
    private Vector2 touchStart; 


    /// <summary>
    /// A reference to the Rigidbody component
    /// </summary>
    private Rigidbody rb; // A reference to the Rigidbody component
    [Tooltip("How fast the ball moves left/right")]
    public float dodgeSpeed = 5; // How fast the ball moves left/right
    [Tooltip("How fast the ball moves forward automatically")]
    [Range(0, 10)]
    public float rollSpeed = 5; // how fast the ball moves forward automatically

    // Start is called before the first frame update
    void Start()
    {
        // Get access to our Rigidbody component
        rb = GetComponent<Rigidbody>();

        minSwipeDistancePixels = minSwipeDistance * Screen.dpi;
    }

    /// <summary>
    /// Fixedupdate is a prime place to put physics
    /// calculations happening over a period of time.
    /// </summary>
    void FixedUpdate()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            print("HIT");
            // Get the pause menu
            var pauseBehaviour = GameObject.FindObjectOfType<PauseScreenBehaviour>();

            // Toggle value
            pauseBehaviour.SetPauseMenu(!PauseScreenBehaviour.paused);
        }
        // if the game is paused, don't do anything
        if (PauseScreenBehaviour.paused)
        {
            return;
        }
        // if the game is paused, don't do anything
        if (PauseScreenBehaviour.paused)
        {
            return;
        }

        // Check if we're moving to the side
        var horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

        // Check if we are running either in the Unity editor or in a *standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR


        /* if the mouse is held down (pr the screen is pressed * on Mobile
         */
        if (Input.GetMouseButton(0))
        {
            /*Get a reference to the camera for converting * between spaces
             */

            // Converts mouse position to a 0 to 1 range
            var screenPos = Input.mousePosition;
            horizontalSpeed = CalculateMovement(screenPos);
            TouchObjects(screenPos); 

        }

        if (Input.touchCount > 0)
            {
                // store the first touch detected
                var firstTouch = Input.touches[0];
                var screenPos = firstTouch.position;
                horizontalSpeed = CalculateMovement(screenPos);
            }
#elif UNITY_IOS || UNITY_ANDROID

    switch (horizMovement)
    {
        case MobileHorizMovement.Accelerometer:
        // move player based on accelerometer direction
        horizontalSpeed = Input.acceleration.x * dodgeSpeed;
        break;

        case MobileHorizMovement.ScreenTouch:
        // check if input registered more than zero touches
        if (Input.touchCount > 0)
        {
        // store the first touch detected
        var firstTouch = Input.touches[0];
        var screenPos = firstTouch.position;
        horizontalSpeed = CalculateMovement(screenPos);
        TouchObjects(firstTouch.position);
        
        }
        break;
    }

#endif

        rb.AddForce(horizontalSpeed, 0, rollSpeed);

    }

    private void update()
    {
        // using keyboard controller to toggle pause menu
        if (Input.GetButtonDown("Cancel"))
        {
            print("HIT");
            // Get the pause menu
            var pauseBehaviour = GameObject.FindObjectOfType<PauseScreenBehaviour>();

            // Toggle value
            pauseBehaviour.SetPauseMenu(!PauseScreenBehaviour.paused);
        }

        //if the game is paused, dont do anything
        if (PauseScreenBehaviour.paused)
        {
            return;
        }

        // check if  we are running either in the Unity editor or in a standalone build
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITYEDITOR

        // if the mouse is tapped
        if (Input.GetMouseButtonDown(0))
        {
          Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
          TouchObjects(screenPos);
          print("Pressed Here");
        }
        // check iif we are running on a mobile device
#elif UNITY_IOS || UNITY_ANDROID
        // check if Input has registered more than zero touches 
        if(Input.touchCount > 0)
     {
      Touch touch = Input.touches[0];

      TouchObjects(touch.position);
      SwipeTeleport(touch);
      ScalePlayer();
     }
#endif
    }

    private float CalculateMovement(Vector3 screenPos)
    {
        // Get a reference to the camera for converting between spaces
        var cam = Camera.main;

        // converets mouse position to a 0 to 1 range
        var viewPos = cam.ScreenToViewportPoint(screenPos);

        float xMove = 0;

        // if we press the right side of the screen
        if (viewPos.x < 0.5f)
        {
            xMove = -1;
        }
        else
        {
            // otherwise we're on the left
            xMove = 1;
        }
        return xMove * dodgeSpeed;
    }

    // will teleport the player if swiped to the left or right 
    private void SwipeTeleport(Touch touch)
    {
        // check if the touch just started
        if (touch.phase == TouchPhase.Began)
        {
            // is so ,set touchStart
            touchStart = touch.position;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            Vector2 touchEnd = touch.position;

            // calculate the difference between the beginiing and end of the touch on x
            float x = touchEnd.x - touchStart.x;

            if (Mathf.Abs (x) < minSwipeDistancePixels)
            {
                return;
            }

            Vector3 moveDirection;

            //If moved negatively in the x axis, move left
            if(x < 0)
            {
                moveDirection = Vector3.left;
            }
            else
            {
                moveDirection = Vector3.right; // otherwise players is on the right
            }

            RaycastHit hit;

            // only move if player wouldnt hit something
            if(!rb.SweepTest(moveDirection, out hit, swipeMove))
            {
                //move player
                var movement = moveDirection * swipeMove;
                var newPos = rb.position + movement;
            }
        }
    }

    //will change the players scale via piniching and stretching two touch events
    private void ScalePlayer()
    {
        // we must have two touches to check if we are scaling properly
        if (Input.touchCount != 2)
        {
            return;
        }
        else
        {
            // store the touches detected
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            Vector2 t0Pos = touch0.position;
            Vector2 t0Delta = touch0.deltaPosition;

            Vector2 t1Pos = touch1.position;
            Vector2 t1Delta = touch1.deltaPosition;

            //find previous frame position of each touch
            Vector2 t0Prev = t0Pos - t0Delta;
            Vector2 t1Prev = t1Pos - t1Delta;

            // find the distance (or magnitude) between the * touches in each frame
            float prevTDeltaMag = (t0Prev - t1Prev).magnitude;

            float tDeltaMag = (t0Pos - t1Pos).magnitude;

            // find the difference in the distances between each frame
            float deltaMagDiff = prevTDeltaMag - tDeltaMag;

            //keep the change consisten no matter what
            float newScale = currentScale;
            newScale -= (deltaMagDiff * Time.deltaTime);

            // ensure that the new value is valid
            newScale = Mathf.Clamp(newScale, minScale, maxScale);

            // Update the players scale
            transform.localScale = Vector3.one * newScale;

            // set current scale for the next frame
            currentScale = newScale; 
        }
    }

    // Will determine if we are touching a game object and if so call events for it
    // parm name="screenPos" the position of the touch 
    private static void TouchObjects(Vector2 screenPos)
    {
        // Conver the position into a ray
        Ray touchRay = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        // create a LayerMask that will collide with all possible channels
        int layerMask = ~0;

        // are we touching an object with a collider?
        if (Physics.Raycast(touchRay, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            // call the playertouch function if it exists on a component attached to this object
            hit.transform.SendMessage("PlayerTouch", SendMessageOptions.DontRequireReceiver);
        }
    }

    // will determine if we are touching a game object and if so call for it
    // <parm name = "touch">
    private static void TouchObjects(Touch touch)
    {
        // Convert the position into a ray
        Ray touchRay = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        print("Pressed");
        // create a layermask that will collide with all possible channels
        int layerMask = ~0;

        // Are we touching an object with a collider?
        if (Physics.Raycast(touchRay, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            // call the pllayerTouch function if it exists on a component attached to this object
            hit.transform.SendMessage("PlayerTouch", SendMessageOptions.DontRequireReceiver);
        }
    }
}
