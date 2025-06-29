using UnityEngine;

public class BackGroungdRunning : MonoBehaviour
{
    [SerializeField] private bool m_CanRunInBackground = true;

    private void Start()
    {
        Application.runInBackground = m_CanRunInBackground;
    }
}
