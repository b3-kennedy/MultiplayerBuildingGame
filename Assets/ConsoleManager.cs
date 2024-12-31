using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq;
using Unity.Netcode;
using System.Linq;

public class DisplayCommand
{
    public string target;

    public List<string> targets = new List<string>();

    public DisplayCommand()
    {
        targets.Add("items");
        targets.Add("coords");
        targets.Add("audio");
        targets.Add("players");
    }
}

public class AudioCommand
{

    public List<string> targets = new List<string>();

    public AudioCommand()
    {
        
    }

}

public class SendCommand
{
    public List<string> targets = new List<string>();

    public SendCommand()
    {
        targets.Add("msg");
    }
}

public class GiveCommand 
{
    public string target;
    public string identifier;
    public string modifier;

    public List<string> targets = new List<string>();


    public string CombineCommand()
    {
        return target + " " + identifier + " " + modifier;
    }

    public GiveCommand()
    {
        targets.Add("item");
    }
}

public class ConsoleManager : NetworkBehaviour
{

    public List<string> validCommands = new List<string>();

    public Queue<string> lastUserMessage = new Queue<string>();

    public GameObject console;
    public Transform contentParent;

    public ConsoleMessage message;
    public TMP_InputField inputField;
    public KeyCode consoleKey = KeyCode.F1;

    public List<GameObject> messages;

    InventoryManager inventoryManager;

    public int index = 0;

    public bool showDebugMessages = true;
    public bool showErrorMessages = true;
    public bool showWarningMessages = true;
    public bool showExceptionMessages = true;
    public bool showAssertionMessages = true;

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
        Debug.LogWarning("test warning");
    }

    private void OnEnable()
    {

        Application.logMessageReceived += AddConsoleLog;

    }

    private void OnDisable()
    {
        Application.logMessageReceived -= AddConsoleLog;
    }

    void AddConsoleLog(string logString, string stackTrace, LogType type)
    {
        string[] logSplit = logString.Split('\n');
        string[] stackSplit = stackTrace.Split('\n');

        string text =  logSplit[0] + " " + stackSplit[0];

        switch (type)
        {
            case LogType.Error:
                if (!showErrorMessages) return;
                string errorMessageText = "ERROR: " + text + "\n";
                CreateAndDisplayMessage(errorMessageText, CustomColours.Red);
                SendMessageToServerConsole(errorMessageText, 0, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Warning:
                if (!showWarningMessages) return;
                string warningMessageText = "WARNING: " + text + "\n";
                CreateAndDisplayMessage(warningMessageText, CustomColours.Amber);
                SendMessageToServerConsole(warningMessageText, 2, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Log:
                if (!showDebugMessages) return;
                string debugMessageText = "DEBUG: " + text + "\n";
                CreateAndDisplayMessage(debugMessageText);
                SendMessageToServerConsole(debugMessageText, 3, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Exception:
                if (!showExceptionMessages) return;
                string exceptionMessageText = "EXCEPTION: " + text + "\n";
                CreateAndDisplayMessage(exceptionMessageText, CustomColours.Red);
                SendMessageToServerConsole(exceptionMessageText, 4, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Assert:
                if (!showAssertionMessages) return;
                string assertionMessageText = "ASSERTION " + text + "\n";
                CreateAndDisplayMessage(assertionMessageText, CustomColours.Red);
                SendMessageToServerConsole(assertionMessageText, 1, NetworkManager.Singleton.LocalClientId);
                break;
        }
    }

    void SendMessageToServerConsole(string text, int type, ulong clientId)
    {
        if (!IsServer)
        {
            SendMessageToServerRpc(text, type, clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendMessageToServerRpc(string text, int type, ulong clientId)
    {
        
        switch (type)
        {
            //error
            case 0:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", CustomColours.Red);
                break;
            //assertion
            case 1:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", CustomColours.Magenta);
                break;
            //warning
            case 2:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", CustomColours.Amber);
                break;
            //log
            case 3:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n");
                break;
            //exception
            case 4:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " +text+"\n", CustomColours.Red);
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsOwner) return;

        if (Input.GetKeyDown(consoleKey) && !console.activeSelf)
        {
            console.SetActive(true);
            GetComponent<PlayerLook>().enabled = false;
            GetComponent<PlayerMovement>().enabled = false;
            GetComponent<InventoryManager>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            inputField.ActivateInputField();
        }
        else if(Input.GetKeyDown(consoleKey) && console.activeSelf)
        {
            console.SetActive(false);
            GetComponent<PlayerLook>().enabled = true;
            GetComponent<PlayerMovement>().enabled = true;
            GetComponent<InventoryManager>().enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            inputField.DeactivateInputField();
            
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var lastUserMessageArray = lastUserMessage.ToArray();

            inputField.text = lastUserMessageArray[(lastUserMessageArray.Length-1) - index];
            index++;

            if(index > lastUserMessage.Count - 1)
            {
                index = lastUserMessage.Count - 1;
            }

        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            var lastUserMessageArray = lastUserMessage.ToArray();


            if (index > 0)
            {
                index--;
                inputField.text = lastUserMessageArray[(lastUserMessageArray.Length - 1) - index];
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && inputField.text != string.Empty)
        {

            if (messages.Count >= 25)
            {
                messages.Remove(messages[0]);
            }

            if(lastUserMessage.Count >= 5)
            {
                lastUserMessage.Dequeue();
            }


           
            ProcessCommand(inputField.text);
            lastUserMessage.Enqueue(inputField.text);
            index = 0;
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }
        else if (Input.GetKeyDown(KeyCode.Return) && inputField.text == string.Empty)
        {
            inputField.ActivateInputField();
        }
    }

    public void CreateAndDisplayMessage(string msg, Color color = default)
    {
        if(color == default)
        {
            color = Color.white;
        }

        GameObject spawnedMsg = Instantiate(message.gameObject, contentParent);
        var msgComponent = spawnedMsg.GetComponent<ConsoleMessage>();
        msgComponent.SetMessage(msg, color);
        messages.Add(spawnedMsg);
    }

    void ProcessCommand(string text)
    {
        string lowerText = text.ToLower();
        string[] elements = lowerText.Split(' ');
        if (text[0] == '/')
        {
            if (!validCommands.Contains(elements[0]))
            {
                CreateAndDisplayMessage("Invalid command", Color.red);
                return;
            }

            if (elements[0].ToLower() == "/give")
            {
                ProcessGiveCommand(elements, lowerText);
            }
            else if (elements[0].ToLower() == "/help")
            {
                CreateAndDisplayMessage(text);
                ProcessHelpCommand();

            }
            else if (elements[0].ToLower() == "/display")
            {
                CreateAndDisplayMessage(text);
                ProcessDisplayCommand(elements, text);
            }
            else if (elements[0].ToLower() == "/clear")
            {
                for (int i = 0; i < contentParent.childCount; i++) 
                {
                    Destroy(contentParent.GetChild(i).gameObject);
                }
            }
            else if (elements[0].ToLower() == "/audio")
            {
                CreateAndDisplayMessage(text);
                ProcessAudioCommand(elements, text);
            }
            else if (elements[0].ToLower() == "/mute")
            {
                CreateAndDisplayMessage(text);
                AudioListener.volume = 0;
                CreateAndDisplayMessage("Audio has been muted", Color.green);
            }
            else if (elements[0].ToLower() == "/unmute")
            {
                CreateAndDisplayMessage(text);
                AudioListener.volume = 1;
                CreateAndDisplayMessage("Audio has been unmuted", Color.green);
            }
            else if (elements[0].ToLower() == "/send")
            {
                //CreateAndDisplayMessage(text);
                ProcessSendCommand(elements, text);

            }
        }
        else
        {
            CreateAndDisplayMessage(text);
        }

    }

    void ProcessSendCommand(string[] elements, string text)
    {
        if(elements.Length < 4)
        {
            CreateAndDisplayMessage("Part of command is missing. The structure of this command is as follows: /send type clientID subject", Color.red);
        }
        else if (elements[1].ToLower() == "msg")
        {
            if (!IsStringAnInteger(elements[2]) && elements[2].ToLower() != "all" && !PlayerNames.Instance.playerNames.Contains(elements[2].ToLower()))
            {
                CreateAndDisplayMessage("Incorrect clientID. You can use integers or their names to send other players messages. Use /display playes to see both names and ids", Color.red);
                return;
            }

            index = 0;
            string msg = "";
            foreach (var section in elements)
            {
                if (index >= 3)
                {
                    msg += section + " ";
                }
                index++;
  
            }

            SendMessageServerRpc(msg, elements[2], NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SendMessageServerRpc(string message, string recipient, ulong senderClientId)
    {
        if(recipient.ToLower() == "all")
        {
            ReceiveAllMessageClientRpc(message, senderClientId);
        }
        else if(IsStringAnInteger(recipient))
        {

            if (NetworkManager.Singleton.ConnectedClients.ContainsKey((ulong)int.Parse(recipient)))
            {
                ReceiveMessageClientRpc(message, senderClientId, (ulong)int.Parse(recipient), true);
            }
            else
            {
                ReceiveMessageClientRpc(message, senderClientId, (ulong)int.Parse(recipient), false);
            }
        }
        else
        {
            if (PlayerNames.Instance.playerNames.Contains(recipient.ToLower()))
            {
                int recipientIndex = PlayerNames.Instance.GetIndexFromName(recipient);
                ReceiveMessageClientRpc(message, senderClientId, (ulong)recipientIndex, true);
            }
            else
            {
                int recipientIndex = PlayerNames.Instance.GetIndexFromName(recipient);
                ReceiveMessageClientRpc(message, senderClientId, (ulong)recipientIndex, false);
            }
        }


    }


    [ClientRpc]
    void ReceiveAllMessageClientRpc(string message, ulong senderClientId)
    {
        var receiverConsole = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId).GetComponent<ConsoleManager>();

        //id 99 is impossible, 99 is sent if the user sends a message to themself
        if (senderClientId == 99)
        {
            receiverConsole.CreateAndDisplayMessage(message);
            return;
        }

        if(NetworkManager.Singleton.LocalClientId != senderClientId)
        {
            receiverConsole.CreateAndDisplayMessage(PlayerManager.Instance.GetClientName(senderClientId) + " (To All): " + message);
        }
        else
        {
            CreateAndDisplayMessage("You (To All): " + message);
        }
        
    }

    [ClientRpc]
    void ReceiveMessageClientRpc(string message, ulong senderClientId, ulong receiverClientId, bool doesClientExist)
    {

        var receiverConsole = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId).GetComponent<ConsoleManager>();

        if (NetworkManager.Singleton.LocalClientId == receiverClientId && NetworkManager.Singleton.LocalClientId == senderClientId)
        {
            string newMessage = PlayerManager.Instance.GetClientName(receiverClientId) + " is talking to themself";
            SendMessageServerRpc(newMessage, "all", 99);
            return;
        }

        

        if(NetworkManager.Singleton.LocalClientId == receiverClientId)
        {
            receiverConsole.CreateAndDisplayMessage(PlayerManager.Instance.GetClientName(senderClientId) + " (To You): " + message);
        }
        else if(NetworkManager.Singleton.LocalClientId == senderClientId)
        {
            if (doesClientExist)
            {
                CreateAndDisplayMessage("You (To " + PlayerManager.Instance.GetClientName(receiverClientId) + "): " + message);
            }
            else
            {
                CreateAndDisplayMessage("Player does not exist", Color.red);
            }
            
        }
        
    }


    void ProcessAudioCommand(string[] elements, string text)
    {
        if(elements.Length < 5)
        {
            CreateAndDisplayMessage("Part of command is missing. The structure of this command is as follows: /audio indentifier x y z", Color.red);
            return;
        }
        else if (!IsStringAnInteger(elements[1]) || !isStringAFloat(elements[2]) || !isStringAFloat(elements[3]) || !isStringAFloat(elements[4]))
        {
            CreateAndDisplayMessage("Incorrect type. The command is as follows: /audio integer float float float", Color.red);
            return;
        }
        else
        {
            int index = int.Parse(elements[1]);

            if(index > AudioManager.Instance.audioClips.Count - 1)
            {
                CreateAndDisplayMessage("Audio clip at index " + index.ToString() + " does not exist", Color.red);
                return;
            }

            float x = float.Parse(elements[2]);
            float y = float.Parse(elements[3]);
            float z = float.Parse(elements[4]);

            AudioManager.Instance.PlayAudio(index, x, y, z);
        }
    }

    void ProcessDisplayCommand(string[] elements,string text)
    {
        DisplayCommand command = new DisplayCommand();

        if(elements.Length < 2)
        {
            CreateAndDisplayMessage("Structure of command invalid", Color.red);
            return;
        }
        else if (!command.targets.Contains(elements[1]))
        {
            CreateAndDisplayMessage("The target of the command is not valid", Color.red);
            return;
        }
        else if (elements[1].ToLower() == "items")
        {
            ItemHolder itemHolder = ItemHolder.Instance;
            string items = "Items and Ids:\n";
            foreach (var item in itemHolder.objects)
            {
                items += "    -" + item.item.ToString() + "----id: " + item.id.ToString() + "\n";
            }
            CreateAndDisplayMessage(items, Color.green);
        }
        else if (elements[1].ToLower() == "coords")
        {
            GameObject player = PlayerManager.Instance.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
            string x = "x: " + player.transform.position.x.ToString() + " ";
            string y = "y: " + player.transform.position.y.ToString() + " ";
            string z = "z: " + player.transform.position.z.ToString() + " ";
            string msg = "Current position: " + x + y + z;
            CreateAndDisplayMessage(msg, Color.green);
        }
        else if (elements[1].ToLower() == "audio")
        {
            var audioManager = AudioManager.Instance;
            string audio = "Audio Clips and Ids:\n";
            foreach (var clip in audioManager.audioClips)
            {
                audio += "    -" + clip.audioClip.name + "----id: " + clip.id.ToString() + "\n";
            }
            CreateAndDisplayMessage(audio, Color.green);
        }
        else if (elements[1].ToLower() == "players")
        {
            string players = "Player Ids and Names:\n";
            foreach (var name in PlayerNames.Instance.playerNames)
            {
                if(name != "")
                {
                    players += "    -" + name + "----id: " + PlayerNames.Instance.GetIndexFromName(name).ToString() + "\n";
                }
            }
            CreateAndDisplayMessage(players, Color.green);
        }
    }

    bool IsStringAnInteger(string input)
    {
        return int.TryParse(input, out _);
    }

    bool isStringAFloat(string input)
    {
        return float.TryParse(input, out _);
    }

    void ProcessHelpCommand()
    {

        string message = "Commands:\n";

        foreach (var command in validCommands)
        {
            message += "    - " + command+"\n";
        }

        CreateAndDisplayMessage(message, Color.green);
    }

    void ProcessGiveCommand(string[] elements, string text)
    {
        GiveCommand command = new GiveCommand();
        CreateAndDisplayMessage(text);

        if (elements.Length < 4 || elements.Length > 4)
        {
            CreateAndDisplayMessage("Part of command is missing. The structure of this command is as follows: /give target identifier modifier", Color.red);
            return;
        }
        else if (!command.targets.Contains(elements[1]))
        {
            CreateAndDisplayMessage("The target of the command is not valid", Color.red);
            return;
        }
        else if (!IsStringAnInteger(elements[2]))
        {
            CreateAndDisplayMessage("Identifier must be an integer", Color.red);
            return;
        }
        else if (!IsStringAnInteger(elements[3]))
        {
            CreateAndDisplayMessage("Modifier must be an integer", Color.red);
            return;
        }
        else if (ItemHolder.Instance.GetItemFromId(int.Parse(elements[2])) == null)
        {
            CreateAndDisplayMessage("No item with id " + elements[2] + " was found", Color.red);
            return;
        }
        else
        {
            CreateAndDisplayMessage(elements[3] + " of item with id of " + elements[2] + " have been added to your inventory", Color.green);


            int count = int.Parse(elements[3]);
            int id = int.Parse(elements[2]);
            Item item = ItemHolder.Instance.GetItemFromId(id);

            for (int i = 0; i < count; i++)
            {
                inventoryManager.AddItem(item);
            }
        }
    }

    public Color ConvertToUnityColor(float r, float g, float b)
    {
        float newR = 0;
        float newG = 0;
        float newB = 0;

        if(r > 0)
        {
            newR = r / 255f;
        }

        if(g > 0)
        {
            newG = g / 255f;
        }

        if(b > 0)
        {
            newB = b / 255f;
        }

        Debug.Log(newR + " " + newG + " " + newB);

        return new Color(newR, newG, newB, 1);
    }

}
