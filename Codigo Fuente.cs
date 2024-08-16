using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class HttpTest : MonoBehaviour
{
    [SerializeField]
    string DBUrl = "https://my-json-server.typicode.com/VelikiyKnyaz/DIST-Consulta-API-JSON/";
    [SerializeField]
    string APIUrl = "https://rickandmortyapi.com/api/character";

    private int currentUserIndex = 0; // Índice actual del usuario
    private User[] users; // Array para almacenar los usuarios

    void Start()
    {
        // Inicializar botones
        Button previousButton = GameObject.Find("Anterior").GetComponent<Button>();
        Button nextButton = GameObject.Find("Siguiente").GetComponent<Button>();
        Button requestButton = GameObject.Find("Button").GetComponent<Button>(); // Botón para realizar la petición

        previousButton.onClick.AddListener(PreviousUser);
        nextButton.onClick.AddListener(NextUser);
        requestButton.onClick.AddListener(SendRequest); // Vincular la solicitud al botón
    }

    void SendRequest()
    {
        StartCoroutine(GetUsers());
    }

    IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(DBUrl + "/users");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                // Deserializar el JSON como un array de User
                users = JsonUtility.FromJson<UserArrayWrapper>("{\"users\":" + request.downloadHandler.text + "}").users;

                LoadUser(currentUserIndex); // Cargar el primer usuario al inicio
            }
            else
            {
                string mensaje = "status:" + request.responseCode;
                mensaje += "\nError: " + request.error;
                Debug.Log(mensaje);
            }
        }
    }

    void LoadUser(int index)
    {
        if (users != null && users.Length > 0)
        {
            User user = users[index];

            GameObject.Find("username").GetComponent<TMP_Text>().text = user.username;

            for (int i = 0; i < user.deck.Length; i++)
            {
                StartCoroutine(GetCharacter(user.deck[i], i));
            }
        }
    }

    public void PreviousUser()
    {
        if (currentUserIndex > 0)
        {
            currentUserIndex--;
            LoadUser(currentUserIndex);
        }
    }

    public void NextUser()
    {
        if (currentUserIndex < users.Length - 1)
        {
            currentUserIndex++;
            LoadUser(currentUserIndex);
        }
    }

    IEnumerator GetCharacter(int id, int index)
    {
        UnityWebRequest www = UnityWebRequest.Get(APIUrl + "/" + id);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (www.responseCode == 200)
            {
                Character character = JsonUtility.FromJson<Character>(www.downloadHandler.text);
                //Debug.Log(character.name + " is a " + character.species);
                StartCoroutine(GetImage(character.image, index));
            }
            else
            {
                string mensaje = "status:" + www.responseCode;
                mensaje += "\nError: " + www.error;
                Debug.Log(mensaje);
            }
        }
    }

    IEnumerator GetImage(string imageUrl, int index)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            var texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            GameObject.Find("RawImage (" + index + ")").GetComponent<RawImage>().texture = texture;
        }
    }
}

[Serializable]
class UsersResponse
{
    public User[] users;
}

[Serializable]
class Character
{
    public int id;
    public string name;
    public string species;
    public string image;
}

[Serializable]
class User
{
    public int id;
    public string username;
    public bool state;
    public int[] deck;
}

[Serializable]
class UserArrayWrapper
{
    public User[] users;
}
