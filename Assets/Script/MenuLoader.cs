using System.Collections;
using UnityEditor;
using UnityEngine;

public class MenuLoader : MonoBehaviour
{
    public static MenuLoader Instance { get; private set; }

    public GameObject Logo1;
    public GameObject VisualFeedBack;
    //  public GameObject Logo2;
    public GameObject Logo3;
    public Animator Animator1;
    public Animator Animator2;
    public GameObject Panel;

  

    private static bool hasPlayed = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Optional: if you need this object in other scenes
    }

    void Start()
    {
        StartCoroutine(LoadScenePanel());
    }


    public IEnumerator ConnectedToServerFeedback()
    {
        VisualFeedBack.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        VisualFeedBack.SetActive(false);
    }



        IEnumerator LoadScenePanel()
    {
      
        if (!hasPlayed)
        {
            yield return new WaitForSeconds(1.5f);
            Animator1.SetTrigger("Start");
            yield return new WaitForSeconds(1.5f);
            Logo1.SetActive(true);
           Animator2.SetTrigger("Start");
            yield return new WaitForSeconds(1.5f);
      
            

            hasPlayed = true;
           
     
            Logo3.SetActive(false);
            yield return new WaitForSeconds(2f);
            Panel.SetActive(false);
            Logo1.SetActive(false);
        






        }

    }
}
