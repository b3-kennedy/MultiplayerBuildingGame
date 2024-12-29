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
                CreateMessage(errorMessageText, Color.red);
                SendMessageToServerConsole(errorMessageText, 0, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Warning:
                if (!showWarningMessages) return;
                string warningMessageText = "WARNING: " + text + "\n";
                CreateMessage(warningMessageText, Color.yellow);
                SendMessageToServerConsole(warningMessageText, 2, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Log:
                if (!showDebugMessages) return;
                string debugMessageText = "DEBUG: " + text + "\n";
                CreateMessage(debugMessageText);
                SendMessageToServerConsole(debugMessageText, 3, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Exception:
                if (!showExceptionMessages) return;
                string exceptionMessageText = "EXCEPTION: " + text + "\n";
                CreateMessage(exceptionMessageText, Color.red);
                SendMessageToServerConsole(exceptionMessageText, 4, NetworkManager.Singleton.LocalClientId);
                break;
            case LogType.Assert:
                if (!showAssertionMessages) return;
                string assertionMessageText = "ASSERTION " + text + "\n";
                CreateMessage(assertionMessageText, Color.red);
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
                CreateMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", Color.red);
                break;
            case 1:
                CreateMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", Color.magenta);
                break;
            case 2:
                CreateMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n", Color.yellow);
                break;
            case 3:
                CreateMessage("[CLIENT " + clientId.ToString() + "] " + text + "\n");
                break;
            case 4:
                CreateMessage("[CLIENT " + clientId.ToString() + "] " +text+"\n", Color.red);
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

    void CreateMessage(string msg, Color color = default)
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
                CreateMessage("Invalid command", Color.red);
                return;
            }

            if (elements[0].ToLower() == "/give")
            {
                ProcessGiveCommand(elements, lowerText);
            }
            else if (elements[0].ToLower() == "/help")
            {
                CreateMessage(text);
                ProcessHelpCommand();

            }
            else if (elements[0].ToLower() == "/display")
            {
                CreateMessage(text);
                ProcessDisplayCommand(elements, text);
            }
            else if (elements[0].ToLower() == "/clear")
            {
                for (int i = 0; i < contentParent.childCount; i++) 
                {
                    Destroy(contentParent.GetChild(i).gameObject);
                }
            }
        }
        else
        {
            CreateMessage(text);
        }

    }

    void ProcessDisplayCommand(string[] elements,string text)
    {
        DisplayCommand command = new DisplayCommand();

        if (!command.targets.Contains(elements[1]))
        {
            CreateMessage("The target of the command is not valid", Color.red);
            return;
        }
        else
        {
            ItemHolder itemHolder = ItemHolder.Instance;
            string items = "Items and Ids:\n";
            foreach (var item in itemHolder.objects)
            {
                items += "    -" + item.item.ToString() + "----id: " + item.id.ToString() + "\n";
            }
            CreateMessage(items, Color.green);
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

        CreateMessage(message, Color.green);
    }

    void ProcessGiveCommand(string[] elements, string text)
    {
        GiveCommand command = new GiveCommand();
        CreateMessage(text);

        if (elements.Length < 4 || elements.Length > 4)
        {
            CreateMessage("Part of command is missing the structure of this command is as follows: /give target identifier modifier", Color.red);
            return;
        }
        else if (!command.targets.Contains(elements[1]))
        {
            CreateMessage("The target of the command is not valid", Color.red);
            return;
        }
        else if (!IsStringAnInteger(elements[2]))
        {
            CreateMessage("Identifier must be an integer", Color.red);
            return;
        }
        else if (!IsStringAnInteger(elements[3]))
        {
            CreateMessage("Modifier must be an integer", Color.red);
            return;
        }
        else if (ItemHolder.Instance.GetItemFromId(int.Parse(elements[2])) == null)
        {
            CreateMessage("No item with id " + elements[2] + " was found", Color.red);
            return;
        }
        else
        {
            CreateMessage(elements[3] + " of item with id of " + elements[2] + " have been added to your inventory", Color.green);


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
