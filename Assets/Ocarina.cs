using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ocarina : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clipList = new List<AudioClip>();
    [SerializeField] bool isMusicPlaying;
    // Tells the script to stop playing the music
    [SerializeField] bool stopMusic;

    [Header("General UI")]
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private TMP_Text playBtn;
    [SerializeField] private Button clearBtn;
    [SerializeField] private Button deleteNoteBtn;
    [SerializeField] private CanvasGroup ocarinaCG;

    [Header("sheet UI")]
    [SerializeField] private ScrollRect scrollRect; 
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private Transform noteList;
    // Tracks whole note objects
    [SerializeField] private List<GameObject> noteObjQueue = new List<GameObject>();
    // Tracks specific note images
    [SerializeField] private List<Image> noteImageQueue = new List<Image>();
    // Color changes when the note is highlighted
    [SerializeField] private Color colorHighlight;

    [Header("Playing Music")]
    [Tooltip("Tracks the order of clips to play")]
    [SerializeField] private List<int> queueIDList = new List<int>();
    // Delay between clips
    [SerializeField] private float delay = 1;
    // Default length when blank clip is played
    [SerializeField] private float defaultLength = 1;

    // Adds music to queue
    public void AddMusic(int id)
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
        ScrollToNote(noteObjQueue.Count - 1);
    }

    // Clears sheet
    public void ClearMusic()
    {
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
        if (queueIDList.Count == 0) return;

        queueIDList.RemoveAt(queueIDList.Count - 1);
        noteImageQueue.RemoveAt(noteImageQueue.Count - 1);

        Destroy(noteObjQueue[noteObjQueue.Count - 1]);
        noteObjQueue.RemoveAt(noteObjQueue.Count - 1);
    }

    // Plays or pauses music
    public void PlayPauseBtn()
    {   
        // Starts playing the music
        if (!isMusicPlaying)
            StartMusic();
        // Tells the script to stop playing music
        else
            stopMusic = true;
    }

    // Starts music
    public void StopsMusic()
    {
        if (queueIDList.Count == 0) return;

        isMusicPlaying = true;

        // Disables player interactable stuff
        ocarinaCG.interactable = false;
        clearBtn.interactable = false;
        deleteNoteBtn.interactable = false;
        scrollbar.interactable = false;
        StartCoroutine(PlayMusic());
    }

    // Starts music
    public void StartMusic()
    {
        if (queueIDList.Count == 0) return;

        isMusicPlaying = true;

        // Flips the play btn text to stop
        playBtn.text = "Stop Music";

        // Disables player interactable stuff
        ocarinaCG.interactable = false;
        clearBtn.interactable = false;
        deleteNoteBtn.interactable = false;
        scrollbar.interactable = false;

        StartCoroutine(PlayMusic());
    }

    // Plays out queue of clips
    private IEnumerator PlayMusic()
    {
        for (int i = 0; i < queueIDList.Count; i++)
        {
            // Stops playing music
            if (stopMusic) break;

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
            ScrollToNote(i);

            yield return new WaitForSeconds(time + delay);
        }

        yield return null;

        // Goes back to the start of the sheet
        ScrollToNote(0);

        // Reset note colors
        foreach (Image image in noteImageQueue)
            image.color = Color.white;

        // Renables player interactable stuff
        ocarinaCG.interactable = true;
        clearBtn.interactable = true;
        deleteNoteBtn.interactable = true;
        scrollbar.interactable = true;

        isMusicPlaying = false;
        stopMusic = false;
        // Flips back to play music
        playBtn.text = "Play Music";
    }

    /// <summary> Updates the sheet's scroll rect location</summary>
    private void ScrollToNote(int index)
    {
        int count = noteObjQueue.Count;

        // Defaults start position
        if (count <= 1)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        
        float contentWidth  = scrollRect.content.rect.width;
        float viewportWidth = scrollRect.viewport != null ? scrollRect.viewport.rect.width
            : ((RectTransform)scrollRect.transform).rect.width;

        float scrollableWidth = contentWidth - viewportWidth;
        if (scrollableWidth <= 0f)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
            return;
        }

        // Caculates position of note
        float itemWidth = contentWidth / count;
        float itemCenter = itemWidth * (index + 0.5f);
        float targetX = itemCenter - viewportWidth * 0.5f;

        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(targetX / scrollableWidth);
    }
}
