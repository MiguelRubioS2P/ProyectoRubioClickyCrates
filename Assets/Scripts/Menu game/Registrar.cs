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

    // Falta añadir un GO que sea para el uso del DontDestoryOnLoad


    void Start()
    {
        
    }

    public void BotonRegistrar()
    {

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
    
}
