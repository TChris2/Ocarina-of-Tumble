using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ocarina : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [Tooltip("List of ocarina RT SFXs")]
    [SerializeField] private List<AudioClip> clipList = new List<AudioClip>();
    [SerializeField] bool isMusicPlaying;
    // Tracks music coroutine
    private Coroutine musicCoroutine;

    [Header("General UI")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private TMP_Text playBtn;
    [SerializeField] private Button clearBtn;
    [SerializeField] private Button deleteNoteBtn;
    [SerializeField] private CanvasGroup ocarinaCG;

    [Header("Sheet UI")]
    [SerializeField] private ScrollRect scrollRect; 
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Transform noteList;
    [Tooltip("Color swapped to when note is highlighted")]
    [SerializeField] private Color colorHighlight;

    [Header("Audio")]
    [Tooltip("Delay between clips")]
    [SerializeField] private float delay = 1;
    [Tooltip("Default length when blank clip is played")]
    [SerializeField] private float defaultLength = 1;

    [Header("List tracking")]
    [Tooltip("Tracks the order of clips to play")]
    [SerializeField] private List<int> queueIDList = new List<int>();
    [Tooltip("Tracks whole note objects created")]
    [SerializeField] private List<GameObject> noteObjQueue = new List<GameObject>();
    [Tooltip("Tracks specific note images used in inside the created prefabs")]
    [SerializeField] private List<Image> noteImageQueue = new List<Image>();

    #region Note logic

    // Adds music to queue
    public void AddNote(int id)
    {
        queueIDList.Add(id);
        
        // Spawns note in list
        GameObject noteObj = Instantiate(notePrefab, noteList);
        // Gets specific note and enables
        Image note = noteObj.transform.GetChild(id).GetComponent<Image>();
        note.gameObject.SetActive(true);

        // Adds it to lists
        noteImageQueue.Add(note);
        noteObjQueue.Add(noteObj);

        // Goes to new note
        _ScrollToNote(noteObjQueue.Count - 1);
    }

    // Clears sheet
    public void ClearMusic()
    {
        // Doesn't do anything if queue is empty
        if (queueIDList.Count == 0) return;

        queueIDList.Clear();
        noteImageQueue.Clear();

        // Deletes objs from note sheet
        if (noteObjQueue.Count != 0)
        {
            foreach (GameObject obj in noteObjQueue)
                Destroy(obj);
        }

        noteObjQueue.Clear();
    }

    // Deletes a single note from the sheet
    public void DeleteNote()
    {
        // Doesn't do anything if queue is empty
        if (queueIDList.Count == 0) return;

        queueIDList.RemoveAt(queueIDList.Count - 1);
        noteImageQueue.RemoveAt(noteImageQueue.Count - 1);

        Destroy(noteObjQueue[noteObjQueue.Count - 1]);
        noteObjQueue.RemoveAt(noteObjQueue.Count - 1);
    }

    #endregion
    #region Music logic

    // Plays or stops music
    public void PlayStopBtn()
    {   
        // Starts playing the music
        if (!isMusicPlaying)
            StartMusic();
        // Stops playing the music
        else
        {
            // Ends the music coroutine
            StopCoroutine(musicCoroutine);
            
            // Renables player interactable stuff
            _TogglePlay(true);

            _ResetSheet();
        }
    }

    // Starts music
    public void StartMusic()
    {
        if (queueIDList.Count == 0) return;

        // Disables player interactable stuff
        _TogglePlay(false);

        musicCoroutine = StartCoroutine(PlayMusic());
    }

    // Plays out queue of clips
    private IEnumerator PlayMusic()
    {
        for (int i = 0; i < queueIDList.Count; i++)
        {
            float time = defaultLength;

            // Does not play clip when on blank
            if (queueIDList[i] != 5)
            {
                AudioClip clip = clipList[queueIDList[i]];
                time = clip.length;
                audioSource.clip = clip;
                audioSource.Play();
            }
            
            // Highlights note
            noteImageQueue[i].color = colorHighlight;

            // Goes to note
            _ScrollToNote(i);

            yield return new WaitForSeconds(time + delay);
        }

        yield return null;

        _ResetSheet();

        // Renables player interactable stuff
        _TogglePlay(true);
    }

    #endregion
    #region UI logic

    /// <summary> Updates the sheet's scroll rect position</summary>
    private void _ScrollToNote(int index)
    {
        // Defaults start position
        if (queueIDList.Count <= 1)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        
        // Caculates whether scrolling is needed
        float contentWidth  = scrollRect.content.rect.width;
        float viewportWidth = scrollRect.viewport.rect.width;
        float scrollableWidth = contentWidth - viewportWidth;
        // If all the notes can fit onscreen without a scrollbar
        if (scrollableWidth <= 0f)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }

        // Caculates position of note
        float itemWidth = contentWidth / queueIDList.Count;
        float itemCenter = itemWidth * (index + 0.5f);
        float targetX = itemCenter - viewportWidth * 0.5f;

        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(targetX / scrollableWidth);
    }

    /// <summary> Toggles interactive stuff the player use atm when music is played or stopped</summary>
    private void _TogglePlay(bool isInteractive)
    {
        ocarinaCG.interactable = isInteractive;
        clearBtn.interactable = isInteractive;
        deleteNoteBtn.interactable = isInteractive;
        scrollbar.interactable = isInteractive;

        isMusicPlaying = !isInteractive;
        // Flips button text
        playBtn.text = isInteractive ? "Play Music" : "Stop Music";
    }

    /// <summary> Resets sheet back to start</summary>
    private void _ResetSheet()
    {
        // Goes back to the start of the sheet
        _ScrollToNote(0);

        // Reset note colors
        foreach (Image image in noteImageQueue)
            image.color = Color.white;
    }

    #endregion
}
