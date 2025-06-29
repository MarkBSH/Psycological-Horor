using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SpotifyUsing : MonoBehaviour
{
    private IEnumerator PlaySpotifyTrack(string trackUrlOrUri, int positionMs = 0)
    {
        string trackUri = ConvertToSpotifyUri(trackUrlOrUri);
        if (string.IsNullOrEmpty(trackUri))
        {
            Debug.LogError("Invalid Spotify track URL or URI: " + trackUrlOrUri);
            yield break;
        }

        // Construct the JSON body with optional position
        string jsonBody = "{\"uris\": [\"" + trackUri + "\"]";
        if (positionMs > 0)
        {
            jsonBody += ", \"position_ms\": " + positionMs;
        }
        jsonBody += "}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new("https://api.spotify.com/v1/me/player/play", "PUT")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Authorization", "Bearer " + GetComponent<LocalHTTPListener>().accessToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 204)
        {
            Debug.Log("Track playback started!");
        }
        else
        {
            Debug.LogError("Failed to play track: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    private string ConvertToSpotifyUri(string input)
    {
        if (input.StartsWith("spotify:track:"))
        {
            return input; // Already a URI
        }

        if (input.StartsWith("https://open.spotify.com/track/"))
        {
            Uri uri = new(input);
            string[] parts = uri.AbsolutePath.Split('/');
            if (parts.Length >= 3)
            {
                string trackId = parts[2];
                return "spotify:track:" + trackId;
            }
        }

        return null; // Invalid input
    }

    public IEnumerator StopSpotifyPlayback()
    {
        UnityWebRequest request = new("https://api.spotify.com/v1/me/player/pause", "PUT")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", "Bearer " + GetComponent<LocalHTTPListener>().accessToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 204)
        {
            Debug.Log("Playback paused.");
        }
        else
        {
            Debug.LogError("Failed to pause playback: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    public IEnumerator SayHelloHowAreYou()
    {
        yield return StartCoroutine(PlaySpotifyTrack("https://open.spotify.com/track/0ENSn4fwAbCGeFGVUbXEU3", 6000));

        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(PlaySpotifyTrack("https://open.spotify.com/track/4uaOAGGlLbUeQy6NPzmRNe", 12000));

        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(PlaySpotifyTrack("https://open.spotify.com/track/05jgkkHC2o5edhP92u9pgU", 118000));

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(StopSpotifyPlayback());

        StartCoroutine(PlaySpotifyTrack("https://open.spotify.com/track/2dhSP0OPGyAhNoj8eNswgb", 0));
    }
}
