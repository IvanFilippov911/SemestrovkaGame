using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Xml;
using System.IO;

public class StartGame : MonoBehaviour
{
    [SerializeField] 
    private Dropdown characterDropdown;
    [SerializeField] 
    private Button startButton;
    [SerializeField] 
    private InputField nameInput;
    [SerializeField]
    private NetworkClient networkClient;

    private void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            string playerName = nameInput.text;
            string characterChoice = characterDropdown.options[characterDropdown.value].text;

            XmlDocument playerInfo = new XmlDocument();
            XmlNode declaration = playerInfo.CreateXmlDeclaration("1.0", "UTF-8", "");
            playerInfo.AppendChild(declaration);
            XmlNode root = playerInfo.CreateElement("PlayerInfo");
            playerInfo.AppendChild(root);
            XmlNode nameNode = playerInfo.CreateElement("Name");
            nameNode.InnerText = playerName;
            root.AppendChild(nameNode);
            XmlNode characterNode = playerInfo.CreateElement("Character");
            characterNode.InnerText = characterChoice;
            root.AppendChild(characterNode);
            playerInfo.Save(Path.Combine (Application.dataPath, "PlayerData.xml"));

            networkClient.Initiate(playerName, characterChoice);
            
            SceneManager.LoadScene("main");
            

        });
    }
}
