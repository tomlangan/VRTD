using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


public class ListUIParams
{
    public delegate bool OnListItemClicked(int index, string itemString);

    public string Title;
    public List<string> Options = new List<string>();
    public OnListItemClicked Callback;
}


public class ListUI : MonoBehaviour
{
    public GameObject ListItemTemplate;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Create(ListUIParams Params)
    {
        Text title = gameObject.GetComponentInChildren<Text>();
        title.text = Params.Title;

        VerticalLayoutGroup layoutGroup = gameObject.GetComponentInChildren<VerticalLayoutGroup>();
        
        GameObject listGroup = null;

        foreach (Transform child in layoutGroup.transform)
        {
            listGroup = child.gameObject;
        }

        List<string> ListOptions = new List<string>(Params.Options);

        for (int i = 0; i < Params.Options.Count; i++)
        {
            GameObject listItem = Instantiate(ListItemTemplate);
            Text listText = listItem.GetComponentInChildren<Text>();
            listText.text = Params.Options[i];

            Button listButton = listItem.GetComponent<Button>();
            string optionSelectedString = Params.Options[i];

            int index = i;
            listButton.onClick.AddListener(() => { 

                bool leaveActive = Params.Callback(index, optionSelectedString);

                if (!leaveActive)
                {
                    this.transform.gameObject.SetActive(false);
                    this.enabled = false;
                }
            });

            listItem.transform.parent = listGroup.transform;
        }
    }
}
