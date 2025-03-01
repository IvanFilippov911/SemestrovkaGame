using System;
using System.IO;
using System.Xml;
using UnityEngine;

public class CharacterLoader : MonoBehaviour
{
    [Serializable]
    public class CharacterChoiceInfo
    {
        public string characterType;
        public GameObject characterPrefab;

    }

    [SerializeField]
    CharacterChoiceInfo[] characterChoiceInfos;
    
    private void Start()
    {
        var character = "";
        var characterName = "";

        XmlDocument playerInfo = new XmlDocument();
        playerInfo.Load(Path.Combine(Application.dataPath, "PlayerData.xml"));
        XmlElement root = playerInfo.DocumentElement;
        foreach (XmlNode node in root.ChildNodes)
        {
            if (node.Name == "Name")
                characterName = node.InnerText;

            if (node.Name == "Character")
                character = node.InnerText;
        }

        foreach (var characterChoiceInfo in characterChoiceInfos)
        {
            if (characterChoiceInfo.characterType == character)
            {
                GameObject playerObject = Instantiate(characterChoiceInfo.characterPrefab);
                playerObject.name = characterName;
                playerObject.GetComponent<Attack>().networkClient = FindAnyObjectByType<NetworkClient>();
                FindAnyObjectByType<NetworkClient>().hero = playerObject.GetComponent<Hero>();
            }
        }
    }
}
