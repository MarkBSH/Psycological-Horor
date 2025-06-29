using UnityEngine;
using System.Net;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Text;

public class LocalHTTPListener : MonoBehaviour
{
    private HttpListener httpListener;
    public string authCode;
    public string accessToken;

    void Start()
    {
        StartCoroutine(StartAuthFlow());
        // string savedToken = PlayerPrefs.GetString("spotify_access_token", ""); // "" is default if not found
        // if (!string.IsNullOrEmpty(savedToken))
        // {
        //     Debug.Log("Found saved access token: " + savedToken);
        //     accessToken = savedToken;
        //     StartCoroutine(GetComponent<SpotifyUsing>().PlaySpotifyTrack("https://open.spotify.com/track/3n3Ppam7vgaVa1iaRUc9Lp")); // Example track URI
        // }
        // else
        // {
        //     Debug.Log("No saved access token found.");
        // }
    }

    IEnumerator StartAuthFlow()
    {
        StartLocalServer();

        string clientId = "27a53b702423460ebed7574a28b697ee";
        string redirectUri = "http://127.0.0.1:4002/callback";
        string scopes = "user-top-read user-modify-playback-state user-read-playback-state";

        string authUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}";
        Application.OpenURL(authUrl);

        // Wait until code is received
        while (string.IsNullOrEmpty(authCode))
            yield return null;

        Debug.Log("Received Auth Code: " + authCode);

        StartCoroutine(ExchangeCodeForToken(authCode));
    }

    void StartLocalServer()
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://127.0.0.1:4002/callback/");
        httpListener.Start();
        httpListener.BeginGetContext(new AsyncCallback(OnRequest), httpListener);
    }

    private void OnRequest(IAsyncResult result)
    {
        if (httpListener == null || !httpListener.IsListening) return;

        var context = httpListener.EndGetContext(result);
        var response = context.Response;

        // Continue listening
        httpListener.BeginGetContext(new AsyncCallback(OnRequest), httpListener);

        // Get "code" from query string
        var query = context.Request.QueryString;
        authCode = query.Get("code");

        string responseString = "<html><body><h2>Spotify auth complete. You can close this window.</h2></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    void OnApplicationQuit()
    {
        if (httpListener != null && httpListener.IsListening)
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }

    IEnumerator ExchangeCodeForToken(string authCode)
    {
        string clientId = "27a53b702423460ebed7574a28b697ee";
        string clientSecret = "5a3bed7230174c318d8c20728edf02ad";
        string redirectUri = "http://127.0.0.1:4002/callback";

        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        string body = $"grant_type=authorization_code&code={authCode}&redirect_uri={Uri.EscapeDataString(redirectUri)}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        UnityWebRequest request = new("https://accounts.spotify.com/api/token", "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Authorization", "Basic " + credentials);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            var tokenData = JsonUtility.FromJson<SpotifyTokenResponse>(response);
            Debug.Log("Access token: " + tokenData.access_token);

            PlayerPrefs.SetString("spotify_access_token", tokenData.access_token);
            PlayerPrefs.Save();

            accessToken = tokenData.access_token;


            // Start the coroutine to make the request
            StartCoroutine(GetComponent<SpotifyUsing>().SayHelloHowAreYou());
        }
        else
        {
            Debug.LogError("Token exchange failed: " + request.error);
        }
    }
}

// Class to match the Spotify token response JSON structure
[Serializable]
public class SpotifyTokenResponse
{
    public string access_token;
    public string token_type;
    public int expires_in;
    public string refresh_token;
    public string scope;
}
