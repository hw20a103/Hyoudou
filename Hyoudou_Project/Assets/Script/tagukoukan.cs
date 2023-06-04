using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tagukoukan : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        this.tag = "BHK";
        this.tag = "WHK";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")  // ï«Ç…Ç‘Ç¬Ç©Ç¡ÇΩÇÁ
        {
            this.tag = "Climbable";             // É^ÉOÇïœçXÇ∑ÇÈ
            
        }
    }
}
