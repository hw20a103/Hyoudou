using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kizokutagukoukann : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void tugukoukann()
    {
        this.tag = "Climbable";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "king")  // 壁にぶつかったら
                                                 // これで設定するとポートとキングが同時に来た時にバグる。
        {
            this.tag = "Climbable";             // タグを変更する

        }
    }
}
