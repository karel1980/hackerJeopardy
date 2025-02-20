using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System;

public class gameSettings : MonoBehaviour
{   
    public Color32 questionBoxColor_unavailable;
    public Color32 questionBoxWasher_normal;
    public Color32 questionBoxWasher_onHover;
    public Color32 questionBoxColor_onClick;
    public Color32 questionBoxColor_empty;

    public string basePath;

    public GameObject playerPrefab;
    public GameObject playerScoredPrefab;

    public List<Player> players = new List<Player>();
    public List<Question> questionList = new List<Question>();
    public List<Category> categoryList = new List<Category>();

    public int selectedQ;
    public int selectedC;
    public string gameName;
    public string tagLine;
    public string fileName;

    public List<KeyCode> playerCodes;

    public int player_chosing;

    public List<GameObject> playerObjects;

    public bool firstCat;
    public bool finishedGame;

    public List<string> TeamsList;




    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        
        if(SceneManager.GetActiveScene().name == "init")
        {
            //do stuff on init
            player_chosing = -1;
            firstCat = true;
            finishedGame = false;
            Display.displays[0].Activate();
            Display.displays[0].SetRenderingResolution(1920, 1080);
            if(Display.displays.Length > 1)
            {
                Display.displays[1].Activate();
                Display.displays[1].SetRenderingResolution(1920, 1080);
            }
            //play some music
            transform.GetComponent<soundScript>().playMusic();
        }

        questionBoxColor_unavailable = new Color32(128, 128, 128, 20);
        questionBoxWasher_normal = new Color32(255, 255, 255, 0);
        questionBoxWasher_onHover = new Color32(0, 0, 0, 40);
        questionBoxColor_empty = new Color32(0, 0, 0, 0);

        playerCodes = new List<KeyCode>();
        playerCodes.Add(KeyCode.F1);
        playerCodes.Add(KeyCode.F2);
        playerCodes.Add(KeyCode.F3);
        playerCodes.Add(KeyCode.F4);
        playerCodes.Add(KeyCode.F5);
        playerCodes.Add(KeyCode.F6);
        playerCodes.Add(KeyCode.F7);
        playerCodes.Add(KeyCode.F8);
        playerCodes.Add(KeyCode.F9);
        playerCodes.Add(KeyCode.F10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadPlayers()
    {
        playerCodes = new List<KeyCode>();
        playerObjects = new List<GameObject>();

        //spawn player objects

        int desiredPWidth = Camera.main.pixelWidth / players.Count;
        if (desiredPWidth > Camera.main.pixelWidth / 5)
        {
            desiredPWidth = (Camera.main.pixelWidth / 5);
        }
        for (int p = 0; p < players.Count; p++)
        {
            playerCodes.Add(players[p].answerButton);
            GameObject thisObj = Instantiate(playerPrefab, GameObject.Find("Players").transform);
            thisObj.GetComponent<playerScript>().updateName(players[p].playerName);
            thisObj.GetComponent<playerScript>().updateScore(players[p].playerScore);
            if(player_chosing == p)
            {
                //player is choosing
                thisObj.GetComponent<playerScript>().updateColor(new Color32(0, 255, 0, 255));
            }
            else
            {
                //normal color
                thisObj.GetComponent<playerScript>().updateColor(new Color32(128, 128, 128, 255));
            }
            
            thisObj.GetComponent<playerScript>().myID = p;

            //place object
            int left = 0 + (desiredPWidth * p);
            int bottom = 0;
            int right = 0 - (Camera.main.pixelWidth - (desiredPWidth * (p + 1)));
            int top = 0;

            thisObj.GetComponent<RectTransform>().offsetMin = new Vector2(left, bottom); // left, bottom
            thisObj.GetComponent<RectTransform>().offsetMax = new Vector2(right, top); // right,top
            playerObjects.Add(thisObj);
        }
    }

    public void addPlayer(string playerName)
    {
        //check if player limit is reached
        if (GameObject.Find("scriptHolder").GetComponent<gameSettings>().players.Count < 10)
        {
            //check if name is taken
            bool found = false;
            foreach (Player findPlayer in GameObject.Find("scriptHolder").GetComponent<gameSettings>().players)
            {
                if (findPlayer.playerName == playerName)
                {
                    found = true;
                }
            }
            if (found == false)
            {
                //add this player
                Player thisPlayer = new Player();
                thisPlayer.playerName = playerName;
                thisPlayer.playerScore = 0;
                GameObject.Find("scriptHolder").GetComponent<gameSettings>().players.Add(thisPlayer);
                
                //remove window (if on init scene)
                if(SceneManager.GetActiveScene().name == "init")
                {
                    GameObject.Find("operator_init_scripts").GetComponent<op_initScripts>().renderPlayers();
                    GameObject.Find("operator_init_scripts").GetComponent<op_initScripts>().hideAddPlayerWindow();

                    //clear name
                    GameObject.Find("inp_playerName").GetComponent<TMP_InputField>().text = "";
                }
                
            }
            else
            {
                //show error
                if (SceneManager.GetActiveScene().name == "init")
                {
                    GameObject.Find("txt_playerNameError").GetComponent<TextMeshProUGUI>().color = new Color(128, 0, 0, 255);
                }
            }
        }
        else
        {
            //player limit is reached
            if (SceneManager.GetActiveScene().name == "init")
            {
                GameObject.Find("txt_playerLimitError").GetComponent<TextMeshProUGUI>().color = new Color(128, 0, 0, 255);
            }
        }
    }

    public void updateCredits(int playerId, int credits, bool positive)
    {
        GameObject thisObj = Instantiate(playerScoredPrefab, GameObject.Find("Canvas").transform);
        thisObj.transform.position = playerObjects[playerId].transform.position;
        if(positive)
        {
            thisObj.transform.Find("scoreTxt").GetComponent<TextMeshProUGUI>().text = "+" + credits.ToString() + "c";
        }
        else
        {
            thisObj.transform.Find("scoreTxt").GetComponent<TextMeshProUGUI>().text = "-" + credits.ToString() + "c";
        }
        
    }

    public void saveFile()
    {


        //wipe file
        File.WriteAllText(fileName, "");
        //write game name
        File.AppendAllText(fileName, gameName + "\n");
        //write tagline
        File.AppendAllText(fileName, tagLine + "\n");
        //write categories
        string jsonCats = JsonConvert.SerializeObject(categoryList);
        File.AppendAllText(fileName, jsonCats);
    }

    public void loadFile()
    {
        string[] allText = File.ReadAllText(fileName).Split("\n");
        gameName = allText[0];
        tagLine = allText[1];
        string toDeserialize = allText[2];
        categoryList = JsonConvert.DeserializeObject<List<Category>>(toDeserialize);

        bool lostFiles = false;
        string mediaDrive = "";
        //check if media files are present
        foreach(Category cat in categoryList)
        {
            foreach(Question q in cat.questions)
            {
                //if(q.PresentationType == 1 || q.PresentationType == 2)
                if (q.questionVideo != "" && q.questionVideo != null && q.questionVideo != "null")
                {
                    if (File.Exists(q.questionVideo) == false)
                    {
                        lostFiles = true;
                        mediaDrive = q.questionVideo.Substring(0, 3);
                    }
                }
                if (q.questionImage != "" && q.questionImage != null && q.questionImage != "null")
                {
                    if (File.Exists(q.questionImage) == false)
                    {
                        lostFiles = true;
                        mediaDrive = q.questionImage.Substring(0, 3);
                    }
                }
                if (q.answerVideo != "" && q.answerVideo != null && q.answerVideo != "null")
                {
                    if (File.Exists(q.answerVideo) == false)
                    {
                        lostFiles = true;
                        mediaDrive = q.answerVideo.Substring(0, 3);
                    }
                }
                if (q.answerImage != "" && q.answerImage != null && q.answerImage != "null")
                {
                    if (File.Exists(q.answerImage) == false)
                    {
                        lostFiles = true;
                        mediaDrive = q.answerImage.Substring(0, 3);
                    }
                }               
            }
        }

        if(lostFiles == true)
        {
            //show error window:
            GameObject.Find("imp_lostMediaDrive").GetComponent<TMP_InputField>().text = mediaDrive;
            GameObject.Find("operator_init_scripts").GetComponent<op_initScripts>().showLostMediaWindow();
        }
    }

    public void loadTeams(string theFile)
    {
        TeamsList = new List<string>();
        string[] allText = File.ReadAllText(theFile).Split("\n");
        foreach(string line in allText)
        {
            TeamsList.Add(line);
        }
        if(TeamsList.Count > 0)
        {
            GameObject.Find("scriptHolder").GetComponent<soundScript>().stopMusic();
            SceneManager.LoadScene("teamSelector");
        }
    }

}

public class Category
{
    public string categoryName;
    public int categoryColorR;
    public int categoryColorG;
    public int categoryColorB;
    public string categoryHint;
    public List<Question> questions;
}

public class Question
{
    public int value;
    public int questionColorR;
    public int questionColorG;
    public int questionColorB;
    public int PresentationType;
    public string questionText;
    public string questionImage;
    public string questionVideo;
    public string answerImage;
    public string answerVideo;
    public string answer;
    public bool isAvailable;
    public string questionNote;
}

public class Player
{
    public string playerName { get; set; }
    public int playerScore { get; set; }
    public KeyCode answerButton { get; set; }
}

