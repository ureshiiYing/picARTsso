using UnityEngine;
using Firebase;
using UnityEngine.Events;


// script to use to initialise firebase when the game starts
// https://firebase.google.com/docs/unity/setup#analytics-enabled
public class FirebaseInit : MonoBehaviour
{
    public UnityEvent OnFirebaseInitialised = new UnityEvent();
    // private FirebaseApp app;

    // Start is called before the first frame update
    private async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            OnFirebaseInitialised.Invoke();
        }
        else
        {
            Debug.LogError(System.String.Format(
            "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
        }
            
        
    }
}
