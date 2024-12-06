using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = System.Random;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
public class MainController : MonoBehaviour
{
    private string startJsonPath = "word.json";

    private int index = 0, correctCount = 0;

    public GameObject hintObj,countObj;

    public GameObject soundTrue,soundFalse;
    
    public GameObject wrodlistPanel,textPrefab,wordListScrollView,addWordPrefab;
    
    public GameObject confrimPanel,addWordPanel;
    
    public GameObject Toggle1,Toggle2;

    public GameObject InputText1,InputText2;


    public List<GameObject> textUIList;
    
    private List<(string word, string translation)> wordList = new List<(string, string)>();
    
    private List<(string word, string translation)> sentenceList = new List<(string, string)>();

    private List<(string word, string translation)> randomList = new List<(string, string)>();

    private List<string> options = new List<string>();

    public static string confrimWord;

    public static bool openConfrimPanelBool = false;

    public static bool openAddWordPanelBool = false;

    private JsonData json;

    private string url;
    void Start()
    {
        
        ToggleGroup toggleGroup = new ToggleGroup();

        Toggle1.GetComponent<Toggle>().group = toggleGroup;
        Toggle2.GetComponent<Toggle>().group = toggleGroup;

        Toggle1.GetComponent<Toggle>().isOn = true;
        Toggle2.GetComponent<Toggle>().isOn = false;
        
        url = Path.Combine(Application.persistentDataPath, startJsonPath);
        
        if (File.Exists(url))
        {
            StartCoroutine(LoadJsonFromPath("file://"+url));
        }
        else
        {

            var filePath = Path.Combine(Application.streamingAssetsPath, startJsonPath);
            
            
            StartCoroutine(LoadJsonFromPath(filePath));
        }
        
        
       
        // json = JsonManager<JsonData>.Read(Path.Combine(Application.streamingAssetsPath, startJsonPath));
        
    }
    
    IEnumerator LoadJsonFromPath(string filePath)
    {
        Debug.Log(filePath);

        UnityWebRequest request = UnityWebRequest.Get(filePath);

        yield return request.SendWebRequest();
        Debug.LogError("error2");

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            Debug.LogError("error3");

        }
        else
        {
            json = JsonUtility.FromJson<JsonData>(request.downloadHandler.text);
            Debug.Log(request.downloadHandler.text);

            // textUIList[0].GetComponent<Text>().text = request.downloadHandler.text;
            
            wordList = new List<(string word, string translation)>();
        
            foreach (var str in json.word)
            {
                var parts = str.Split(new[] { ",,," }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    wordList.Add((parts[0], parts[1]));
                }
            }
        
            foreach (var str in json.sentence)
            {
                var parts = str.Split(new[] { ",,," }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    sentenceList.Add((parts[0], parts[1]));
                }
            }
            
            GenerateRandomList();
        
            // PrintRandomList();

            updateWord();

            SaveJsonToFile(url,json);

        }
        

    }
    
    void SaveJsonToFile(string filePath, JsonData data)
    {
        string jsonText = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonText);
    }
    
    public void UpdateWordListDisplay()
    {
        foreach (Transform child in wordListScrollView.transform)
        {
            Destroy(child.gameObject);
        }

        var delta = new Vector3(0,0,0);

        foreach (var item in wordList)
        {
            GameObject textObject = Instantiate(textPrefab, wordListScrollView.transform);

            textObject.transform.position = delta + textObject.transform.position;
            delta.y -= 100;
            Text textComponent = textObject.GetComponent<Text>();
            textComponent.text = $"{item.word}: {item.translation}";
            textObject.name = $"WordText";
        }

        foreach (var item in sentenceList)
        {
            GameObject textObject = Instantiate(textPrefab, wordListScrollView.transform);
            
            textObject.transform.position = delta + textObject.transform.position;
            delta.y -= 100;
            Text textComponent = textObject.GetComponent<Text>();
            textComponent.text = $"{item.word}: {item.translation}";
            textObject.name = $"WordText";
        }
        
        GameObject buttonObject = Instantiate(addWordPrefab, wordListScrollView.transform);
            
        buttonObject.transform.position = delta + buttonObject.transform.position;

        wordListScrollView.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (wordList.Count+sentenceList.Count + 2)*100);

    }

    public void confirmTrue()
    {
        
        RemoveWord(confrimWord);
        UpdateWordListDisplay();
        confrimPanel.SetActive(false);

    }
    
    public void RemoveWord(string wordToRemove)
    {
        for (int i = 0; i < wordList.Count; i++)
        {
            if (wordList[i].word == wordToRemove)
            {
                wordList.RemoveAt(i);
                json.word.RemoveAt(i);
                SaveJsonToFile(url,json);

                return;
            }
        }
        
        for (int i = 0; i < sentenceList.Count; i++)
        {
            if (sentenceList[i].word == wordToRemove)
            {
                sentenceList.RemoveAt(i);
                json.sentence.RemoveAt(i);
                SaveJsonToFile(url,json);
                

                return;
            }
        }
    }
    
    
    public void confirmFalse()
    {
        confrimPanel.SetActive(false);
    }
    
    public void AddWordTrue()
    {
        // wrodlistPanel.SetActive(true);
        
        var pair = (InputText1.GetComponent<Text>().text,InputText2.GetComponent<Text>().text); 

        if (Toggle1.GetComponent<Toggle>().isOn)
        {
            wordList.Add(pair);
            json.word.Add(pair.Item1 + ",,," + pair.Item2);
        }
        else
        {
            sentenceList.Add(pair);
            json.sentence.Add(pair.Item1 + ",,," + pair.Item2);
        }
        
        SaveJsonToFile(url,json);

        UpdateWordListDisplay();
        addWordPanel.SetActive(false);

    }

    public void AddWordFalse()
    {        
        addWordPanel.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (openConfrimPanelBool)
        {
            openConfrimPanelBool = false;

            confrimPanel.SetActive(true);
            foreach (Transform child in confrimPanel.transform)
            {
                if (child.name == "text")
                {
                    child.GetComponent<Text>().text = "确认要删除" + confrimWord + "吗？";
                }
            }
        }

        if (openAddWordPanelBool)
        {
            openAddWordPanelBool = false;
            addWordPanel.SetActive(true);

        }
        
    }
    

    
    public void openWordlist()
    {
        wrodlistPanel.SetActive(true);
        // StartCoroutine(Fade(1, 1,wrodlistPanel));
        UpdateWordListDisplay();
    }

    public void closeWordlist()
    {

        correctCount = 0;
        index = 0;
        hintObj.GetComponent<Text>().text = "";
        countObj.GetComponent<Text>().text = correctCount.ToString() + " / " + index.ToString();
        GenerateRandomList();
        updateWord();

        
        // StartCoroutine(Fade(0, 1,wrodlistPanel));
        wrodlistPanel.SetActive(false);
        
        
    }


    private IEnumerator Fade(float toAlpha, float duration,GameObject panelToFade)
    {
        Image image = panelToFade.GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("Panel does not have an Image component.");
            yield break;
        }

        float elapsedTime = 0;
        float startAlpha = image.color.a;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, toAlpha, elapsedTime / duration);
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, toAlpha); 
    }
    
    
    public void clickButton1()
    {
        checkOptions(1);
    }
 
    public void clickButton2()
    {
        checkOptions(2);
    }
    
    public void clickButton3()
    {
        checkOptions(3);
    }
    
    public void clickButton4()
    {
        checkOptions(4);
    }

    void checkOptions(int i)
    {
        Debug.Log(i);

        if (options[i - 1] == randomList[index].translation)
        {
            hintObj.GetComponent<Text>().text = "正确！！";
            hintObj.GetComponent<Text>().color = Color.green;
            soundTrue.GetComponent<AudioSource>().Play();
            correctCount++;
        }
        else
        {
            hintObj.GetComponent<Text>().text = "错误！";
            hintObj.GetComponent<Text>().color = Color.red;
            soundFalse.GetComponent<AudioSource>().Play();

        }

        index++;

        countObj.GetComponent<Text>().text = correctCount.ToString() + " / " + index.ToString();
        
        updateWord();
    }

    void GenerateRandomList()
    {
        Random random = new Random();
    
        // 初始化随机列表
        randomList = new List<(string, string)>();
    
        // 为words和sentences创建一个临时列表，用于存储随机选择的元素
        List<(string, string)> tempWordList = new List<(string, string)>(wordList);
        List<(string, string)> tempSentenceList = new List<(string, string)>(sentenceList);
    
        // 随机选择words
        while (tempWordList.Count > 0)
        {
            int k = random.Next(tempWordList.Count);
            (string word, string translation) value = tempWordList[k];
            randomList.Add(value);
            tempWordList.RemoveAt(k); // 移除已选择的元素，防止重复
        }
    
        // 随机选择sentences
        while (tempSentenceList.Count > 0)
        {
            int k = random.Next(tempSentenceList.Count);
            (string word, string translation) value = tempSentenceList[k];
            randomList.Add(value);
            tempSentenceList.RemoveAt(k); // 移除已选择的元素，防止重复
        }
    }

    void PrintRandomList()
    {
        foreach (var (word, translation) in randomList)
        {
            Debug.Log($"Word: {word}, Translation: {translation}");
        }
    }

    public void updateWord()
    {
        (string word, string[] options) = GetNextWordWithOptions();
        
        textUIList[0].GetComponent<Text>().text = word;

        for (int i = 1; i < textUIList.Count; i++)
        {
            textUIList[i].GetComponentInChildren<Text>().text = options[i-1];
        }
        
        // randomList.RemoveAt(0); 

    }
    
    public (string word, string[] options) GetNextWordWithOptions()
    {
        if (index == randomList.Count)
        {
            GenerateRandomList();
            index = 0;
            correctCount = 0;
        }

        var (word, correctTranslation) = randomList[index];
        // randomList.RemoveAt(0); 
        options = new List<string> { correctTranslation };

        while (options.Count < 4)
        {
            if (word.Contains(" "))
            {
                var randomTranslation = sentenceList[new Random().Next(sentenceList.Count)].translation;
                if (!options.Contains(randomTranslation))
                {
                    options.Add(randomTranslation);
                }
            }
            else
            {
                var randomTranslation = wordList[new Random().Next(wordList.Count)].translation;
                if (!options.Contains(randomTranslation))
                {
                    options.Add(randomTranslation);
                }
                
            }

        }

        // 打乱选项
        Shuffle(options);



        return (word, options.ToArray());
    }
    
    public void Shuffle<T>(List<T> list)
    {
        Random rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


}
