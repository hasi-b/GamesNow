using System.Collections;
using UnityEngine;
using TMPro;

public class TextChange : MonoBehaviour
{
    public TMP_Text textMeshProObject; // Assign the TextMeshPro object in the Inspector
    public string[] messages; // Array of messages to display (6 messages required)
    private int currentIndex = 0; // To keep track of the current message index

    void Start()
    {
        if (messages.Length < 6)
        {
            Debug.LogError("Please ensure the 'messages' array has at least 6 messages.");
            return;
        }

        //StartCoroutine(ChangeTextRoutine());
    }

   public IEnumerator ChangeTextRoutine()
    {
        while (currentIndex < 6) // Repeat 6 times (30 seconds total)
        {
            textMeshProObject.text = messages[currentIndex]; // Set the text
            currentIndex++; // Move to the next message
            yield return new WaitForSeconds(5f); // Wait for 5 seconds
        }
    }
}