using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;




public class ListUI : MonoBehaviour
{
    public GameObject ListItemTemplate;
    public delegate void OnListItemClicked(int index, string itemString);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Create(
        string Title,
        List<string> Items,
        OnListItemClicked callback)
    {
        Text title = gameObject.GetComponentInChildren<Text>();
        title.text = Title;

        VerticalLayoutGroup layoutGroup = gameObject.GetComponentInChildren<VerticalLayoutGroup>();
        
        GameObject listGroup = null;

        foreach (Transform child in layoutGroup.transform)
        {
            listGroup = child.gameObject;
        }

        for (int i = 0; i < Items.Count; i++)
        {
            GameObject listItem = Instantiate(ListItemTemplate);
            Text listText = listItem.GetComponentInChildren<Text>();
            listText.text = Items[i];
            Button listButton = listItem.GetComponent<Button>();
            listButton.onClick.AddListener(() => { 
                callback(i, Items[i]); 
            });

            listItem.transform.parent = listGroup.transform;
        }
    }
}
