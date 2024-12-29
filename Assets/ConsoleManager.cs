using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq;
using Unity.Netcode;

public class DisplayCommand
{
    public string target;

    public List<string> targets = new List<string>();

    public DisplayCommand()
    {
        targets.Add("items");
        targets.Add("coords");
    }
}

public class AudioCommand
{

    public List<string> targets = new List<string>();

    public AudioCommand()
    {
        
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
                CreateAndDisplayMessage(errorMessageText, Color.red);
                SendMessageToServerConsole(errorMessageText, 0, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Warning:
                if (!showWarningMessages) return;
                string warningMessageText = "WARNING: " + text + "\n";
                CreateAndDisplayMessage(warningMessageText, Color.yellow);
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
                CreateAndDisplayMessage(exceptionMessageText, Color.red);
                SendMessageToServerConsole(exceptionMessageText, 4, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Assert:
                if (!showAssertionMessages) return;
                string assertionMessageText = "ASSERTION " + text + "\n";
                CreateAndDisplayMessage(assertionMessageText, Color.red);
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
            case 0:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", Color.red);
                break;
            case 1:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", Color.magenta);
                break;
            case 2:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", Color.yellow);
                break;
            case 3:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n");
                break;
            case 4:
                CreateAndDisplayMessage("[CLIENT " + clientId.ToString() + "] " +text+"\n", Color.red);
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

    void CreateAndDisplayMessage(string msg, Color color = default)
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
        }
        else
        {
            CreateAndDisplayMessage(text);
        }

    }

    void ProcessAudioCommand(string[] elements, string text)
    {
        if(elements.Length < 5)
        {
            CreateAndDisplayMessage("Part of command is missing. The structure of this command is as follows: /audio indentifier x y z", Color.red);
            return;
        }
        else if (!IsStringAnInteger(elements[1]) || !IsStringAnInteger(elements[2]) || !IsStringAnInteger(elements[3]) || !IsStringAnInteger(elements[4]))
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
    }

    bool IsStringAnInteger(string input)
    {
        return int.TryParse(input, out _);
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


}
