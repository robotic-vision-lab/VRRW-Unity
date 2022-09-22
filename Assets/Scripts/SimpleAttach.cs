using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class SimpleAttach : MonoBehaviour
{
    private Interactable interactable;
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & ( ~Hand.AttachmentFlags.SnapOnAttach ) & ( ~Hand.AttachmentFlags.DetachOthers ) & ( ~Hand.AttachmentFlags.VelocityMovement );

    Vector3 OriginalPosition;
    Quaternion OriginalRotation;


    // Start is called before the first frame update
    void Start()
    {
        interactable = this.GetComponent<Interactable>();
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RestorePose()
    {
        transform.position = OriginalPosition;
        transform.rotation = OriginalRotation;
    }

    private void OnHandHoverBegin(Hand hand)
    {
        hand.ShowGrabHint();
    }

    private void OnHandHoverEnd(Hand hand)
    {
        hand.HideGrabHint();
    }

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
        {
            // Save our position/rotation so that we can restore it when we detach
            // oldPosition = transform.position;
            // oldRotation = transform.rotation;

            // Call this to continue receiving HandHoverUpdate messages,
            // and prevent the hand from hovering over anything else
            hand.HoverLock(interactable);

            // Attach this object to the hand
            hand.AttachObject(gameObject, startingGrabType, attachmentFlags);

            // Hide hints
            hand.HideGrabHint();
        }
        else if (isGrabEnding)
        {
            // Detach this object from the hand
            hand.DetachObject(gameObject);

            // Call this to undo HoverLock
            hand.HoverUnlock(interactable);

            // Restore position/rotation
            // transform.position = oldPosition;
            // transform.rotation = oldRotation;
        }
    }
}
