using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRadius = 2f; // Initial interaction radius
    public float hoverDistance = 1.5f; // Distance of the first circle from the player
    public float hoverSpeed = 2f; // Speed at which objects move to the circle position
    public int baseObjectsPerCircle = 7; // Number of objects in the first circle
    public float interactRadiusIncrement = 5f; // Increase in interaction radius per circle
    public float revolutionSpeed = 30f; // Speed of revolution around the player (degrees/second)
    public float selfRotationSpeed = 100f; // Speed of objects' self-rotation around their Z-axis
    public float interactionDelay = 5f; // Delay before object joins the circle
    public Sprite activeBackgroundSprite; // Temporary background sprite during interaction
    public LayerMask interactableLayer; // LayerMask for interactable objects
    public GameObject[] backgroundObjects; // The background objects in the scene
    public Sprite originalBackgroundSprite; // The original sprite of the background objects
    public TMP_Text IWantSpaceText; // TextMeshPro object for "I WANT SPACE"
    public int totalObjectsToInteract = 15; // Total number of objects to interact with

    [SerializeField] private List<GameObject> attachedObjects = new List<GameObject>(); // List of objects orbiting the player
    private PlayerMovement playerMovement;
    private float originalMoveSpeed;
    public float pushObjectSpeed;
    bool isFinalStage;
    [SerializeField]
    GameObject explosion;
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            originalMoveSpeed = playerMovement.moveSpeed; // Store the original speed
        }

        if (IWantSpaceText != null)
        {
            IWantSpaceText.gameObject.SetActive(false); // Ensure the text is initially inactive
        }

        // Ensure the array is initialized
        if (backgroundObjects != null)
        {
            foreach (GameObject backgroundObject in backgroundObjects)
            {
                if (backgroundObject != null)
                {
                    SpriteRenderer bgRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                    if (bgRenderer != null && originalBackgroundSprite == null)
                    {
                        // Store the sprite of the first valid background object as the original sprite
                        originalBackgroundSprite = bgRenderer.sprite;
                    }
                }
            }
        }
    }

    void Update()
    {

        if (isFinalStage && Input.GetKeyDown(KeyCode.Space))
        {
            BackgroundAudio.instance.StartAngerMusic();
            PushObjectsOutward();
        }
        // Check for interaction input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryInteract();
        }

        // Update positions and rotations of hovering objects
        UpdateHoverPositions();

        // Check if all objects are attached
        if ( !isFinalStage && attachedObjects.Count == totalObjectsToInteract)
        {
            TriggerFinalStage();
        }
    }

    void TryInteract()
    {
        // Find all objects in the interact radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                continue;
            }
            GameObject obj = collider.gameObject;

            // If the object isn't already attached, attach it
            if (!obj.GetComponent<ObjectInteraction>().IsInlist)
            {
                obj.GetComponent<ObjectInteraction>().IsInlist = true;
                StartCoroutine(HandleInteraction(obj));
                break; // Attach only one object per key press
            }
        }
    }

    IEnumerator HandleInteraction(GameObject obj)
    {
        // Find the Canvas and TextMeshPro under the object
        Canvas canvas = obj.GetComponentInChildren<Canvas>(true);
        TextMeshProUGUI textMeshPro = canvas?.GetComponentInChildren<TextMeshProUGUI>(true);

        // Activate the Canvas and TextMeshPro
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true); // Activate the Canvas
        }

        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(true); // Activate the TextMeshPro
        }

        // Change all background objects' sprites temporarily
        if (backgroundObjects != null && activeBackgroundSprite != null)
        {
            foreach (GameObject backgroundObject in backgroundObjects)
            {
                if (backgroundObject != null)
                {
                    SpriteRenderer bgRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                    if (bgRenderer != null)
                    {
                        bgRenderer.sprite = activeBackgroundSprite;
                    }
                }
            }
        }

        // Wait for the interaction delay
        yield return new WaitForSeconds(interactionDelay);

        // Restore all background objects' original sprites
        if (backgroundObjects != null && originalBackgroundSprite != null)
        {
            foreach (GameObject backgroundObject in backgroundObjects)
            {
                if (backgroundObject != null)
                {
                    SpriteRenderer bgRenderer = backgroundObject.GetComponent<SpriteRenderer>();
                    if (bgRenderer != null)
                    {
                        bgRenderer.sprite = originalBackgroundSprite;
                    }
                }
            }
        }

        // Continue to attach the object
        AttachObject(obj);
    }

    void AttachObject(GameObject obj)
    {
        // Add the object to the list
        attachedObjects.Add(obj);

        // Disable the object's collider to prevent further interaction
        obj.GetComponent<Collider2D>().enabled = false;

        // Modify Rigidbody2D properties
        if (obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            //rb.bodyType = RigidbodyType2D.Kinematic; // Set the body type to Kinematic
            rb.velocity = Vector2.zero; // Stop the object's motion
        }

        // Parent the object to the player
        obj.transform.parent = transform;
    }

    void UpdateHoverPositions()
    {
        int circleIndex = 0; // Which circle layer the object belongs to
        int objectsInCurrentCircle = 0; // Number of objects already in the current circle
        int maxObjectsInCircle = baseObjectsPerCircle; // Start with the base number of objects in the first circle

        for (int i = 0; i < attachedObjects.Count; i++)
        {
            GameObject obj = attachedObjects[i];

            // Check if the current circle is full
            if (objectsInCurrentCircle >= maxObjectsInCircle)
            {
                circleIndex++; // Move to the next circle
                objectsInCurrentCircle = 0; // Reset counter for the new circle
                maxObjectsInCircle *= 2; // Double the capacity for the next circle
            }

            // Calculate the angle for the object in the current circle
            float angle = (objectsInCurrentCircle / (float)maxObjectsInCircle) * 360f;
            float radius = hoverDistance + (circleIndex * hoverDistance);

            // Adjust revolution direction (alternating between clockwise and counterclockwise)
            float revolutionDirection = (circleIndex % 2 == 0) ? 1 : -1;
            float revolutionAngle = Time.time * revolutionSpeed * revolutionDirection;

            // Add revolution angle to object's base angle
            angle += revolutionAngle;

            // Convert angle to radians and calculate position
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                0
            );

            // Smoothly move the object to the target position
            obj.transform.localPosition = Vector3.Lerp(
                obj.transform.localPosition,
                offset,
                Time.deltaTime * hoverSpeed
            );

            // Rotate the object around its own Z-axis
            obj.transform.Rotate(Vector3.forward * selfRotationSpeed * Time.deltaTime);

            // Update objects in the current circle
            objectsInCurrentCircle++;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the interaction radius in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    void TriggerFinalStage()
    {
        // Stop player movement
        playerMovement.moveSpeed = 0;
       
        // Show "I WANT SPACE" text
        if (IWantSpaceText != null)
        {
            IWantSpaceText.gameObject.SetActive(true);
        }


        isFinalStage = true;
        // Initialize the pushing coroutine
        
    }

    
    IEnumerator StopExplosion()
    {
        yield return new WaitForSeconds(5f);
        explosion.SetActive(false);
       
        foreach (GameObject gm in attachedObjects)
        {
            Vector3 direction = (gm.transform.position - this.transform.position).normalized;
            this.GetComponent<SpriteRenderer>().color = Color.yellow;
            gm.GetComponent<SpriteRenderer>().sprite = gm.GetComponent<ObjectInteraction>().Sprite;
            gm.GetComponent<Rigidbody2D>().AddForce(direction * 200f);
            //Destroy(gm, 5f);

            // Move the object outward in the opposite direction

        }
        attachedObjects.Clear();

        yield return new WaitForSeconds(10f);
        SceneManager.LoadSceneAsync(1);
    }

    void PushObjectsOutward()
    {
       
        
            // Wait for the player to press Space
           

            //// Pop the last object from the stack
            //GameObject objToPush = attachedObjects[count - 1];
            //// attachedObjects.RemoveAt(attachedObjects.Count - 1);
            //count--;
            //PushObject(objToPush);

            explosion.SetActive(true);

            StartCoroutine(StopExplosion());

            
           

        
        
        Debug.Log("Outside the loop");
        // Restore player movement and UI
        playerMovement.moveSpeed = originalMoveSpeed;
        Debug.Log("Player should move now");
            IWantSpaceText.gameObject.SetActive(false);
          
    }

    void PushObject(GameObject obj)
    {
        obj.transform.parent = null; // Detach object
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = true; // Re-enable collider

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Ensure Rigidbody2D is dynamic and gravity is disabled
            rb.bodyType = RigidbodyType2D.Dynamic; // Set to dynamic for physics interaction
            rb.gravityScale = 0; // Disable gravity effect

            // Calculate the direction from the player to the object and normalize it
            Vector3 pushDirection = (obj.transform.position - transform.position).normalized;

            // Apply force outward, pushing the object in the correct direction
            rb.AddForce(pushDirection * pushObjectSpeed, ForceMode2D.Impulse); // Use Impulse to apply an immediate force
        }
        else
        {
            // If there's no Rigidbody2D, manually move the object (if desired)
            obj.transform.position += (obj.transform.position - transform.position).normalized * pushObjectSpeed * Time.deltaTime;
        }
    }
}