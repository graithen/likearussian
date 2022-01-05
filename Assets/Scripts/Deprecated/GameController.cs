using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public GameObject[] Cards = new GameObject[6];
    public Sprite[] Sprites = new Sprite[6];
    public Sprite CardBack;
    public List<int> cardsHand = new List<int> { 0, 1, 2, 3, 4, 5 };
    public List<int> cardsShuffled;
    public int DrawCount = 0;

    [Header("Scoring")]
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI PotText;
    public int Pot = 12;
    public int Score = 0;

    [Header("Audio")]
    public AudioSource Audio;
    public AudioClip Gunshot, Hammer, Prepare;

    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        PotText.text = Pot.ToString();
        ScoreText.text = Score.ToString();
    }

    void PlayAudio(AudioClip audio)
    {
        Audio.PlayOneShot(audio);
    }

    public void DrawCard()
    {
        Cards[DrawCount].GetComponent<Image>().sprite = Sprites[cardsHand[DrawCount]];
        if (cardsHand[DrawCount] == 5)
        {
            FailState();
            return;
        }
        
        Pot -= 1;
        Score += 1;
        UpdateUI();

        DrawCount++;
        
        PlayAudio(Hammer);
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < cardsHand.Count; i++)
        {
            int temp = cardsHand[i];
            int randomIndex = Random.Range(i, cardsHand.Count);
            cardsHand[i] = cardsHand[randomIndex];
            cardsHand[randomIndex] = temp;
            foreach (GameObject card in Cards)
                card.GetComponent<Image>().sprite = CardBack;
            DrawCount = 0;
            Debug.Log("Completed shuffle!");
            
            PlayAudio(Prepare);
        }
    }

    void FailState()
    {
        PlayAudio(Gunshot);
        Pot += Score;
        Score = 0;

        UpdateUI();
    }
}
