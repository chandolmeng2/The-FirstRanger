using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    [SerializeField] private GameObject _managerParent;

    void Start()
    {
        SceneTransitionManager.Instance.LoadScene("MainMenu");
        DataManager.Instance.Initialize();
        Initialize();
    }

    private void Initialize()
    {
        var managers = _managerParent.GetComponentsInChildren<IManager>();

        foreach (var manager in managers)
        {
            if (manager is MonoBehaviour mono)
            {
                bool isRoot = mono.transform.parent == null;
                bool isAlreadyPreserved = mono.gameObject.scene.name == null;

                if (!isRoot && !isAlreadyPreserved)
                {
                    mono.transform.SetParent(null);
                    DontDestroyOnLoad(mono.gameObject);
                    manager.Initialize();
                }
            }
        }
    }

}
