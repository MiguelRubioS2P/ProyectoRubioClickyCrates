using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Registrar : MonoBehaviour
{
    // Tenemos la ruta de la api
    private const string httpServer = "";

    // Bloque de añadir los elementos directamente al script
    public InputField emailInputField;
    public InputField contraInputField;
    public InputField confirmarContraInputField;
    public InputField nombreInputField;
    public InputField apellidosInputField;
    public InputField ciudadInputField;
    public InputField fechaNacimientoInputField;
    public InputField apodoInputField;

    public PlayerSerializable player;
    private string token;

    // Falta añadir un GO que sea para el uso del DontDestoryOnLoad


    void Start()
    {
        
    }

    // Llamaremos a un método que contendra los distintos métodos
    public void BotonRegistrar()
    {
        StartCoroutine(RegisterNewPlayer());
    }

    private IEnumerator RegisterNewPlayer()
    {
        if(contraInputField.text == confirmarContraInputField.text)
        {
            if (!string.IsNullOrEmpty(emailInputField.text))
            {
                yield return RegistrarAspNetUser();
                yield return GetAuthenticationToken();
                yield return GetAspNetUserId();
                yield return InsertPlayer();
            }
        }
    }

    // Método que vamos a implementar para primero registrar un usuario en AspNetUser
    // para crear la base de datos por defecto.
    private IEnumerator RegistrarAspNetUser()
    {
        // Necesitamos la clase AspNetUserModel para crear un objeto de este tipo
        AspNetUserModel aspPlayer = new AspNetUserModel();

        // Ahora le damos los valores de los inputs del formulario al objeto que hemos creado
        aspPlayer.Email = emailInputField.text;
        aspPlayer.Password = contraInputField.text;
        aspPlayer.ConfirmPassword = confirmarContraInputField.text;

        // Vamos a crear ya el bloque de conexión con la api
        using(UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/Register", "POST"))
        {
            // Creamos un string formato json a partir del objeto aspPlayer
            string bodyJson = JsonUtility.ToJson(aspPlayer);
            // Obtenemos los bytes del string serializado
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            // Descargamos una cabecera con la información en bytes
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // Ponemos las cabeceras necesarias para poder enviar la información en modo json
            httpClient.SetRequestHeader("Content-type", "application/json");

            // Enviamos la petición a la api
            yield return httpClient.SendWebRequest();

            // Como puede fallar, preparamos un entorno para atrapar los errores
            if(httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegistarAspNetUser > Error: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegistrarAspNetUser > Info: " + httpClient.responseCode);
            }
        }
    }

    // Método para obtener el token del usuario registrado en la tabla de AspNetUser
    private IEnumerator GetAuthenticationToken()
    {
        // Creamos una cabecera wwwForm
        WWWForm data = new WWWForm();
        // Añadimos los elementos necesarios para la cabecera que hemos creado
        data.AddField("grant_type", "password");
        data.AddField("username", emailInputField.text);
        data.AddField("password", contraInputField.text);

        // Creamos el bloque para conectarnos a la api
        using (UnityWebRequest httpClient = UnityWebRequest.Post(httpServer + "/Token", data))
        {
            // Enviamos la petición a la api
            yield return httpClient.SendWebRequest();

            // Controlamos la posibilidad de error de la petición a la api
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("GetAuthenticationToken > Error: " + httpClient.error);
            }
            else
            {
                // Obtenemos el valor que nos devuelve la petición en formato string
                string jsonResponse = httpClient.downloadHandler.text;
                // Creamos el objeto del tipo authtoken y deserializamos el string que nos devuelve la api
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                // Guardamos el token en una variable 
                token = authToken.access_token;
            }

        }
    }

    // Método para obtener el id del usuario 
    private IEnumerator GetAspNetUserId()
    {
        // Bloque para interactuar con la api, este endPoint hay que crearlo en la api
        using(UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Account/UserId", "GET"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes("Nothing");
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + token);

            yield return httpClient.SendWebRequest();

            if(httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("GetAspNetUserId > Error: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
            }
        }
    }

    // Método para insertar un jugador, hace falta crear la tabla de la base de datos y un endPoint en la api
    private IEnumerator InsertPlayer()
    {
        player.Email = emailInputField.text;
        player.FirstName = nombreInputField.text;
        player.LastName = apellidosInputField.text;
        player.NickName = apodoInputField.text;
        player.City = ciudadInputField.text;
        player.DateOfBirth = fechaNacimientoInputField.text;
        player.BlobUri = "";

        using(UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/InsertNewPlayer","POST"))
        {
            string playerData = JsonUtility.ToJson(player);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + token);

            yield return httpClient.SendWebRequest();

            if(httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("InsertPlayer > Error: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertPlayer > Info: " + httpClient.responseCode);
            }

        }
    }

    // Método para obtener la fecha en que se registro, hay que crear un endPoint
    private IEnumerator GetPlayerDateJoined()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(httpServer + "/api/Player/GetPlayerDateJoined", "GET"))
        {
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterNewPlayer > GetPlayerDateJoined: " + httpClient.error);
            }
            else
            {
                player.DateJoined = httpClient.downloadHandler.text.Replace("\"", "");
            }
        }
    }

    // Método para saber la edad del jugador
    private void GetPlayerAge()
    {
        DateTime birthday = DateTime.Parse(player.DateOfBirth);
        DateTime today = DateTime.Now;
        TimeSpan difference = today - birthday;
        //debugConsoleText.text += "\n Player is " + difference.Days / 365 +
        //    " years and " + difference.Days % 365 + " days old";
    }

}
