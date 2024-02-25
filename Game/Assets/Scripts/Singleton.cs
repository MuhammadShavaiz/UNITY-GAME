using UnityEngine;

//Generic Singleton Class
//Singletons are classes that can only have 1 object (or instance) at a time.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;

    public static T Instance    //using Capital I for Public
    {
        get
        {
            //If the instance is null, set the instance to this current class
            //otherwise if its not == we want to destroy the gameObject
            if (instance == null)           
            { 
                instance = FindObjectOfType<T>();

                // If still null, create a new GameObject and add the component
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                }

                // Make the instance persistent across scenes
                DontDestroyOnLoad(instance.gameObject);
            }
            //else if (instance != null)      { Destroy(FindObjectOfType<T>()); }

            //DontDestroyOnLoad(FindObjectOfType<T>());
            return instance;
        }
    }
}
