using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameSceneDirector : MonoBehaviour
{
    public bool debug = true;
    float debtimer;

    // ゲーム設定
    public const int TILE_X = 15;
    public const int TILE_Y = 15;
    const int PLAYER_MAX = 2;

    // タイルのプレハブ
    public GameObject[] prefabTile;


    // タイルのプレハブ追加　特殊タイル。
    public GameObject[] prefabTile2;

    // カーソルのプレハブ
    public GameObject prefabCursor;

    // 内部データ
    GameObject[,] tiles;
    UnitController[,] units;

    // ユニットのプレハブ（色ごと）
    public List<GameObject> prefabWhiteUnits;
    public List<GameObject> prefabBlackUnits;

    public static List<Player_data> Players = new List<Player_data>();


    // 1 = ポーン(平民) 2 = ルーク（貴族） 3 = ナイト（英雄） 4 = ビショップ 5 = クイーン 6 = キング（王様）
    //
    //ここで湧いて来る駒を設定している。
    public int[,] unitType =
    {

        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 13 },
        { 6, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 16 },
        { 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 13 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 12 },
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},
        { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0},


    };

    // UI関連
    GameObject txtTurnInfo;
    GameObject txtResultInfo;
    GameObject btnApply;
    GameObject btnCancel;
    
    // 選択ユニット
    UnitController selectUnit;

    // 移動関連
    List<Vector2Int> movableTiles;
    List<GameObject> cursors;

    //指揮力
    /*public int sikiryoku;
    public Text txtsikinouyoku;

    public int sikiryoku2;
    public Text txtsikinouyoku2;
    */

    // モード
    enum MODE
    {
        NONE,
        CHECK_MATE,
        NORMAL,
        STATUS_UPDATE,
        TURN_CHANGE,
        RESULT
    }

    MODE nowMode, nextMode;
    int nowPlayer;

    // 前回ユニット削除から経過ターン
    int prevDestroyTurn;

    // 前回の盤面
    List<UnitController[,]> prevUnits;


    //移動した時のフラグを作る。



    // Start is called before the first frame update
    void Start()
    {
        


        // UIオブジェクト取得
        txtTurnInfo = GameObject.Find("TextTurnInfo");
        txtResultInfo = GameObject.Find("TextResultInfo");
        btnApply = GameObject.Find("ButtonApply");
        btnCancel = GameObject.Find("ButtonCancel");

        // リザルト関連は非表示
        btnApply.SetActive(false);
        btnCancel.SetActive(false);

        // 内部データの初期化
        tiles = new GameObject[TILE_X, TILE_Y];
        units = new UnitController[TILE_X, TILE_Y];
        cursors = new List<GameObject>();
        prevUnits = new List<UnitController[,]>();

        //タイルの生成とユニットの生成 二重ループ。
        for (int i = 0; i < TILE_X; i++)//横のデータ。
        {
            for (int j = 0; j < TILE_Y; j++)//縦のデータ
            {
                // タイルとユニットのポジション
                float x = i - TILE_X / 2;　//半分だけずらす。　ワールドの原点からじゃなくて、真ん中に寄せる。
                float y = j - TILE_Y / 2;

                Vector3 pos = new Vector3(x, 0, y);　//yをzにしている。

                
                    
                
                
                    // 作成
                    int idx = (i + j) % 2;
                    GameObject tile = Instantiate(prefabTile[idx], pos, Quaternion.identity); //instantiate(プレファブの指定、場所の指定、角度の指定)

                    tiles[i, j] = tile;

                

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

                // 内部データセット
                units[i, j] = ctrl;
            }
        }

        nowPlayer = 1;
        nowMode = MODE.NONE;
        nextMode = MODE.TURN_CHANGE;
    }

    // Update is called once per frame
    void Update()
    {
        if(MODE.CHECK_MATE == nowMode)
        {
            checkMateMode();
        }
        else if(MODE.NORMAL == nowMode)
        {
            normalMode();
        }
        else if(MODE.STATUS_UPDATE == nowMode)
        {
            statusUpdateMode();
        }
        else if(MODE.TURN_CHANGE == nowMode)
        {
            turnChangeMode();
        }
        else if(MODE.RESULT == nowMode)
        {
            if (debug)
            {
                debtimer += Time.deltaTime;
                if(2 < debtimer)
                {
                    print("勝敗 "+ txtResultInfo.GetComponent<Text>().text);
                    Retry();
                }
            }
        }

        // モード変更
        if(MODE.NONE != nextMode)
        {
            nowMode = nextMode;
            nextMode = MODE.NONE;
        }

        

    }

    // チェックメイトモード
    void checkMateMode()
    {
        // 次のモード
        nextMode = MODE.NORMAL;
        Text info = txtResultInfo.GetComponent<Text>();
        info.text = "";

        // --------------------
        // ドローのチェック（簡易版）
        // --------------------

        // 1 vs 1になったら引き分け
        if ( 3 > getUnits().Count)
        {
            info.text = "チェックメイトできないので\n引き分け";
            nextMode = MODE.RESULT;
        }

        // 50ターンの誰も削除されなければドロー
        if( 1000 < prevDestroyTurn)
        {
            info.text = "1000ターンルールで\n引き分け";
            nextMode = MODE.RESULT;
        }

        // 3回同じ盤面になったらドロー
        int prevcount = 0;

        foreach (var v in prevUnits)
        {
            bool check = true;

            for (int i = 0; i < v.GetLength(0); i++)
            {
                for (int j = 0; j < v.GetLength(1); j++)
                {
                    if (v[i, j] != units[i, j]) check = false;

                }
            }

            if (check) prevcount++;
        }

        // 3回続いたか
        if( 2 < prevcount)
        {
            info.text = "同じ盤面が続いたので\n引き分け";
            nextMode = MODE.RESULT;
        }

        
        // --------------------
        // チェックのチェック
        // --------------------
        // 今回のプレイヤーのキング
        UnitController target = getUnit(nowPlayer, UnitController.TYPE.KING);
        // チェックしているユニット
        List<UnitController> checkunits = GetCheckUnits(units, nowPlayer);
        // チェック状態セット
        bool ischeck = (0 < checkunits.Count) ? true : false;

        if( null != target)
        {
            target.SetCheckStatus(ischeck);
        }

        // ゲームが続くならチェックと表示
        if( ischeck && MODE.RESULT != nextMode)
        {
            info.text = "チェック！！";
        }

        // --------------------
        // 移動可能範囲を調べる
        // --------------------
        int tilecount = 0;

        // 移動可能範囲をカウント
        foreach(var v in getUnits(nowPlayer))
        {
            tilecount += getMovableTiles(v).Count;
        }

        // 動かせない　
        if( 1 > tilecount )
        {
            info.text = "ステイルメイト\n" + "引き分け";

            if (ischeck)
            {
                info.text = "チェックメイト\n" + (getNextPlayer() + 1) + "Pの勝ち！！";
            }

            nextMode = MODE.RESULT;
        }

        // 今回の盤面をコピー
        UnitController[,] copyunits = GetCopyArray(units);
        prevUnits.Add(copyunits);

        // 次のモードの準備
        if(MODE.RESULT == nextMode)
        {
            btnApply.SetActive(true);
            btnCancel.SetActive(true);
        }
    }

    // ノーマルモード
    void normalMode()
    {
        GameObject tile = null;
        UnitController unit = null;

        // プレイヤーの処理
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // ユニットにも当たり判定があるのでヒットした全てのオブジェクト情報を取得
            foreach (RaycastHit hit in Physics.RaycastAll(ray))
            {
                if (hit.transform.name.Contains("Tile"))
                {
                    tile = hit.transform.gameObject;
                    break;
                }
            }
        }

        // CPUの処理
        while( TitleSceneDirector.PlayerCount <= nowPlayer
                && (null == selectUnit || null == tile ) )
        {
            
                // ユニット選択
                if (null == selectUnit)
                {
                    // 今回の全ユニット
                    List<UnitController> tmpunits = getUnits(nowPlayer);
                    // ランダムで1体選ぶ
                    UnitController tmp = tmpunits[Random.Range(0, tmpunits.Count)];
                    // ユニットがいるタイルを選択
                    tile = tiles[tmp.Pos.x, tmp.Pos.y];

                    // 一旦処理へ流す
                    break;
                }

                // ここから下はselectUnitが入った状態でくる
                if (1 > movableTiles.Count)
                {
                    setSelectCursors();
                    break;
                }

                // 移動可能範囲があればランダムで移動
                int rnd = Random.Range(0, movableTiles.Count);
                tile = tiles[movableTiles[rnd].x, movableTiles[rnd].y];

               


            }
           
        

        // タイルが押されていなければ処理しない
        if (null == tile) return;

        // 選んだタイルからユニット取得
        Vector2Int tilepos = new Vector2Int(
            (int)tile.transform.position.x + TILE_X / 2,
            (int)tile.transform.position.z + TILE_Y / 2);

        // ユニット
        unit = units[tilepos.x, tilepos.y];


        

        
        // ユニット選択
        if (null != unit
            && selectUnit != unit
            && nowPlayer == unit.Player )
        {
            // 移動可能範囲を取得
            List<Vector2Int> tiles = getMovableTiles(unit);

            // 選択不可
            if (1 > tiles.Count) return;

            movableTiles = tiles;
            setSelectCursors(unit);
        }
        // 移動
        else if (null != selectUnit && movableTiles.Contains(tilepos))
        {

                
                
            moveUnit(selectUnit, tilepos);


            // コメントアウトで連続行動
            nextMode = MODE.STATUS_UPDATE;

            






        }
        // 移動範囲だけ見られる
        else if( null != unit && nowPlayer != unit.Player)
        {
            setSelectCursors(unit, false);
        }
        // 選択解除
        else
        {
            setSelectCursors();
        }

        
    }

    // 移動後の処理
   void statusUpdateMode()
    {
        

        




        

        // カーソル
        setSelectCursors();

        
        
        
        
        
        
    }

    // ターン変更
    void turnChangeMode()
    {
        // ターンの処理
        nowPlayer = getNextPlayer();

        // Infoの更新
        txtTurnInfo.GetComponent<Text>().text = "" + (nowPlayer + 1) + "Pの番です";

        // 経過ターン（１P側にきたら+1）
        if( 0 == nowPlayer)
        {
            prevDestroyTurn++;
            
        }

        nextMode = MODE.CHECK_MATE;
    }

    public void tumchangebottnn()
    {
        

        turnChangeMode();
    }

    int getNextPlayer()
    {
        int next = nowPlayer + 1;
        if (PLAYER_MAX <= next) next = 0;

        return next;
    }

    // 指定のユニットを取得する
    UnitController getUnit(int player, UnitController.TYPE type)
    {
        foreach (var v in getUnits(player))
        {
            if (player != v.Player) continue;
            if(type == v.Type ) return v;
        }

        return null;
    }

    // 指定されたプレイヤー番号のユニットを取得する
    List<UnitController> getUnits(int player = -1)
    {
        List<UnitController> ret = new List<UnitController>();

        foreach (var v in units)
        {
            if (null == v) continue;

            if(player == v.Player)
            {
                ret.Add(v);
            }
            else if( 0 > player)
            {
                ret.Add(v);
            }

        }

        return ret;
    }

    // 指定された配列をコピーして返す
    public static UnitController[,] GetCopyArray(UnitController[,] org)
    {
        UnitController[,] ret = new UnitController[org.GetLength(0), org.GetLength(1)];
        Array.Copy(org, ret, org.Length);
        return ret;
    }

    // 移動可能範囲取得　　大切
    List<Vector2Int> getMovableTiles(UnitController unit)
    {
        // そこをどいたらチェックされてしまうか
        UnitController[,] copyunits = GetCopyArray(units);
        copyunits[unit.Pos.x, unit.Pos.y] = null;

        // チェックされるかどうか
        List<UnitController> checkunits = GetCheckUnits(copyunits, unit.Player);

        // チェックを回避できるタイルを返す
        if( 0 < checkunits.Count)
        {
            // 移動可能範囲
            List<Vector2Int> ret = new List<Vector2Int>();

            // 移動可能範囲
            List<Vector2Int> movetiles = unit.GetMovableTiles(units);

            // 移動してみる
            foreach(var v in movetiles)
            {
                // 移動した状態を作る
                UnitController[,] copyunits2 = GetCopyArray(units);
                copyunits2[unit.Pos.x, unit.Pos.y] = null;
                copyunits2[v.x, v.y] = unit;

                int checkcount = GetCheckUnits(copyunits2, unit.Player, false).Count;

                if (1 > checkcount) ret.Add(v);
            }

            return ret;
        }

        // 通常移動可能範囲を返す
        return unit.GetMovableTiles(units);
    }

    // 選択時の関数
    void setSelectCursors(UnitController unit=null, bool setunit = true)
    {
        // カーソル解除
        foreach (var v in cursors)
        {
            Destroy(v);
        }

        // 選択ユニットの非選択状態
        if( null != selectUnit)
        {
            selectUnit.SelectUnit(false);
            selectUnit = null;
        }

        // なにもセットされないなら終了
        if (null == unit) return;

        // カーソル作成
        foreach(var v in getMovableTiles(unit))
        {
            Vector3 pos = tiles[v.x, v.y].transform.position;
            pos.y += 0.51f;

            GameObject obj = Instantiate(prefabCursor, pos, Quaternion.identity);
            cursors.Add(obj);
        }

        // 選択状態
        if(setunit)
        {
            selectUnit = unit;
            selectUnit.SelectUnit();
        }
    }

    // ユニット移動
    void moveUnit(UnitController unit, Vector2Int tilepos)
    {
        // 現在地
        Vector2Int unitpos = unit.Pos;

        // 誰かいたら消す
        if(null != units[tilepos.x, tilepos.y])
        {
            Destroy(units[tilepos.x, tilepos.y].gameObject);
            
        }

        

        
        // 新しい場所へ移動
        unit.MoveUnit(tiles[tilepos.x, tilepos.y]);

        // 内部データ更新（元の場所）
        units[unitpos.x, unitpos.y] = null;

        // 内部データ更新（新しい場所）
        units[tilepos.x, tilepos.y] = unit;
    }

    // ユニットのプレハブを取得
    GameObject getPrefabUnit(int player, int type)
    {
        int idx = type - 1;

        if (0 > idx) return null;

        GameObject prefab = prefabWhiteUnits[idx];
        if( 1 == player ) prefab = prefabBlackUnits[idx];

        return prefab;
    }

    // 指定された配置でチェックされているかチェック
    static public List<UnitController> GetCheckUnits(UnitController[,] units, int player, bool checkking = true)
    {
        List<UnitController> ret = new List<UnitController>();

        foreach (var v in units)
        {
            if (null == v) continue;
            if (player == v.Player) continue;

            // 敵1体の移動可能範囲
            List<Vector2Int> enemytiles = v.GetMovableTiles(units, checkking);

            foreach (var t in enemytiles)
            {
                if (null == units[t.x, t.y]) continue;

                if(UnitController.TYPE.KING == units[t.x, t.y].Type)
                {
                    ret.Add(v);
                }
            }
        }

        return ret;
    }

    

    





    public void Retry()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Title()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
