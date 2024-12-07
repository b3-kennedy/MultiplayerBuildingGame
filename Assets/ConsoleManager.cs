using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

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

public class ConsoleManager : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {

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

    GameObject CreateMessage(string msg, Color color = default)
    {
        if(color == default)
        {
            color = Color.white;
        }

        GameObject spawnedMsg = Instantiate(message.gameObject, contentParent);
        var msgComponent = spawnedMsg.GetComponent<ConsoleMessage>();
        msgComponent.SetMessage(msg, color);
        return spawnedMsg;
    }

    void ProcessCommand(string text)
    {
        string lowerText = text.ToLower();
        string[] elements = lowerText.Split(' ');
        if (text[0] == '/')
        {
            if (!validCommands.Contains(elements[0]))
            {
                GameObject messageObject = CreateMessage("Invalid command", Color.red);
                messages.Add(messageObject);
                return;
            }

            if (elements[0].ToLower() == "/give")
            {
                ProcessGiveCommand(elements, lowerText);
            }
            else if (elements[0].ToLower() == "/help")
            {
                GameObject commandMessage = CreateMessage(text);
                messages.Add(commandMessage);
                ProcessHelpCommand();

            }
            else if (elements[0].ToLower() == "/display")
            {
                GameObject commandMessage = CreateMessage(text);
                messages.Add(commandMessage);
                ProcessDisplayCommand(elements, text);
            }
        }
        else
        {
            GameObject messageObject = CreateMessage(text);
            messages.Add(messageObject);
        }

    }

    void ProcessDisplayCommand(string[] elements,string text)
    {
        DisplayCommand command = new DisplayCommand();

        if (!command.targets.Contains(elements[1]))
        {
            GameObject messageObject = CreateMessage("The target of the command is not valid", Color.red);
            messages.Add(messageObject);
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
            GameObject messageObject = CreateMessage(items, Color.green);
            messages.Add(messageObject);
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

        GameObject messageObject = CreateMessage(message, Color.green);
        messages.Add(messageObject);
    }

    void ProcessGiveCommand(string[] elements, string text)
    {
        GiveCommand command = new GiveCommand();
        GameObject commandMessage = CreateMessage(text);
        messages.Add(commandMessage);

        if (elements.Length < 4 || elements.Length > 4)
        {
            GameObject messageObject = CreateMessage("Part of command is missing the structure of this command is as follows: /give target identifier modifier", Color.red);
            messages.Add(messageObject);
            return;
        }
        else if (!command.targets.Contains(elements[1]))
        {
            GameObject messageObject = CreateMessage("The target of the command is not valid", Color.red);
            messages.Add(messageObject);
            return;
        }
        else if (!IsStringAnInteger(elements[2]))
        {
            GameObject messageObject = CreateMessage("Identifier must be an integer", Color.red);
            messages.Add(messageObject);
            return;
        }
        else if (!IsStringAnInteger(elements[3]))
        {
            GameObject messageObject = CreateMessage("Modifier must be an integer", Color.red);
            messages.Add(messageObject);
            return;
        }
        else if (ItemHolder.Instance.GetItemFromId(int.Parse(elements[2])) == null)
        {
            GameObject messageObject = CreateMessage("No item with id " + elements[2] + " was found", Color.red);
            messages.Add(messageObject);
            return;
        }
        else
        {
            GameObject messageObject = CreateMessage(elements[3] + " of item with id of " + elements[2] + " have been added to your inventory", Color.green);

            messages.Add(messageObject);

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
