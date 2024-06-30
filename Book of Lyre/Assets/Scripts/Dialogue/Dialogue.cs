using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [Header("UI component")]
    public Text textLabel;
    public Image faceImage;

    [Header("file")]
    public TextAsset textFile;
    public int index;
    public float textSpeed;

    [Header("Avatar")]
    //加几个都行
    public Sprite face01, face02, face03;

    bool textFinished;
    bool cancelTyping;

    List<string> textList = new List<string>();

    // Start is called before the first frame update
    void Awake()
    {
        GetTextFormFile(textFile); 
    }
    private void OnEnable()
    {
        textFinished = true;
        StartCoroutine(SetTexUI());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) && index == textList.Count)
        {
            gameObject.SetActive(false);
            index = 0;
            return;
        }
        //if (Input.GetKeyDown(KeyCode.R) && textFinished)
        //{
        //    StartCoroutine(SetTexUI());
        //}

        if (Input.GetKeyDown(KeyCode.R))
        {
            if(textFinished && !cancelTyping)
            {
                StartCoroutine(SetTexUI());
            }
            else if (!textFinished && !cancelTyping)
            {
                cancelTyping = true;
            }
        }
    }

    void GetTextFormFile(TextAsset file)
    {
        textList.Clear();
        index = 0;

        var lineData = file.text.Split('\n');

        foreach(var line in lineData)
        {
            textList.Add(line);
        }
    }

    IEnumerator SetTexUI()
    {
        textFinished = false;
        textLabel.text = "";

        switch (textList[index])
        {
            //随便改成啥都行
            case"A":
                faceImage.sprite = face01;
                index++;
                break;
            case "B":
                faceImage.sprite = face02;
                index++;
                break;
            case "C":
                faceImage.sprite = face03;
                index++;
                break;
            //textLabel.fontSize += 25;
        }

            int letter = 0;
        while(!cancelTyping && letter < textList[index].Length - 1)
        {
            textLabel.text += textList[index][letter];
            letter++;
            yield return new WaitForSeconds(textSpeed);
        }
        textLabel.text = textList[index];
        cancelTyping = false;
        textFinished = true;
        index++;
    }
}
