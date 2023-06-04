using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneDirector : MonoBehaviour
{
    // ゲーム全体のプレイヤー数
    static public int PlayerCount;

    // タイルのプレハブ
    public GameObject[] prefabTile;

    // ユニットのプレハブ（色ごと）
    public List<GameObject> prefabWhiteUnits;
    public List<GameObject> prefabBlackUnits;

    // 1 = ポーン 2 = ルーク 3 = ナイト 4 = ビショップ 5 = クイーン 6 = キング
    public int[,] unitType =
    {
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12},
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 13 },
        { 6, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 16 },
        { 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 13 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
    };

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < GameSceneDirector.TILE_X; i++)
        {
            for (int j = 0; j < GameSceneDirector.TILE_Y; j++)
            {
                // タイルとユニットのポジション
                float x = i - GameSceneDirector.TILE_X / 2;
                float y = j - GameSceneDirector.TILE_Y / 2;

                Vector3 pos = new Vector3(x, 0, y);

                // 作成
                int idx = (i + j) % 2;
                GameObject tile = Instantiate(prefabTile[idx], pos, Quaternion.identity);

                // ユニットの作成
                int type = unitType[i, j] % 10;
                int player = unitType[i, j] / 10;

                GameObject prefab = getPrefabUnit(player, type);
                GameObject unit = null;
                UnitController ctrl = null;

                if (null == prefab) continue;

                pos.y += 1.5f;
                unit = Instantiate(prefab);

                // 初期化処理
                ctrl = unit.GetComponent<UnitController>();
                ctrl.SetUnit(player, (UnitController.TYPE)type, tile);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ユニットのプレハブを取得
    GameObject getPrefabUnit(int player, int type)
    {
        int idx = type - 1;

        if (0 > idx) return null;

        GameObject prefab = prefabWhiteUnits[idx];
        if (1 == player) prefab = prefabBlackUnits[idx];

        return prefab;
    }

    public void PvP()
    {
        PlayerCount = 2;
        SceneManager.LoadScene("SampleScene");
    }

    public void PvE()
    {
        PlayerCount = 1;
        SceneManager.LoadScene("SampleScene");
    }

    public void EvE()
    {
        PlayerCount = 0;
        SceneManager.LoadScene("SampleScene");
    }
}
