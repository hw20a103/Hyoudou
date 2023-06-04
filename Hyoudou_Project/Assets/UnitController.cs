using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    // このユニットのプレイヤー番号
    public int Player;





    
    


    // ユニットの種類
    public TYPE Type;
    // 置いてからの経過ターン
    public int ProgressTurnCount;
    // 置いてる場所
    public Vector2Int Pos, OldPos;
    // 移動状態
    public List<STATUS> Status;

    // 1 = ポーン 2 = ルーク 3 = ナイト 4 = ビショップ 5 = クイーン 6 = キング
    public enum TYPE
    {
        NONE = -1,
        PAWN = 1,
        ROOK,
        KNIGHT,
        BISHOP,
        QUEEN,
        KING,

    }

    public enum STATUS
    {
        NONE=-1,
        QSIDE_CASTLING=1,
        KSIDE_CASTLING,
        EN_PASSANT,
        CHECK,
        

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 初期設定
    public void SetUnit(int player, TYPE type, GameObject tile)
    {
        Player = player;
        Type = type;
        MoveUnit(tile);
        ProgressTurnCount = -1; // 初期状態に戻す

        
    }

    // 移動可能範囲取得
    public List<Vector2Int> GetMovableTiles(UnitController[,] units, bool checkking = true)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        // クイーン
        if (TYPE.QUEEN == Type)
        {
            // ルークとビショップの動きを合成
            ret = getMovableTiles(units, TYPE.ROOK);

            foreach (var v in getMovableTiles(units, TYPE.BISHOP))
            {
                if (!ret.Contains(v)) ret.Add(v);

            }
        }
        else if (TYPE.KING == Type)
        {
            ret = getMovableTiles(units, TYPE.KING);

            // 相手の移動範囲を考慮しない
            if (!checkking) return ret;

            // 削除対象のタイル
            List<Vector2Int> removetiles = new List<Vector2Int>();

            // 敵の移動可能範囲にはいけないようにする
            foreach (var v in ret)
            {
                // 移動した状態を作る
                UnitController[,] copyunits2 = GameSceneDirector.GetCopyArray(units);
                copyunits2[Pos.x, Pos.y] = null;
                copyunits2[v.x, v.y] = this;

                int checkcount = GameSceneDirector.GetCheckUnits(copyunits2, Player, false).Count;

                if (0 < checkcount) removetiles.Add(v);
            }

            // ↑で取得した敵の移動可能範囲とかぶるタイルを削除する
            foreach (var v in removetiles)
            {
                ret.Remove(v);

                // キャスリングできる時だけ真横のタイルも全て削除する
                if (-1 < ProgressTurnCount || Pos.y != v.y) continue;

                // 方向
                int dir = -1;
                int cnt = units.GetLength(0);
                if (0 > Pos.x - v.x) dir = 1;

                for (int i = 0; i < cnt; i++)
                {
                    Vector2Int del = new Vector2Int(v.x + (i * dir), v.y);
                    ret.Remove(del);
                }

            }

        }//ポーンは相手がルークならいけないようにする。
       /* else if (TYPE.PAWN == Type)
        {
            ret = getMovableTiles(units, TYPE.PAWN);

            // 相手の移動範囲を考慮しない
            if (!checkking) return ret;

            // 削除対象のタイル
            List<Vector2Int> removetiles = new List<Vector2Int>();

            // 敵の移動可能範囲にはいけないようにする
            foreach (var v in ret)
            {
                // 移動した状態を作る
                UnitController[,] copyunits2 = GameSceneDirector.GetCopyArray(units);
                copyunits2[Pos.x, Pos.y] = null;
                

                int checkcount = GameSceneDirector.GetCUnits(copyunits2, Player, false).Count;

                if (0 < checkcount) removetiles.Add(v);
            }

            // ↑で取得した敵の移動可能範囲とかぶるタイルを削除する
            foreach (var v in removetiles)
            {
                ret.Remove(v);

                

            }

        }
       */
        else
        {
            ret = getMovableTiles(units, Type);
        }
        
        return ret;
    }


    

    // 移動可能範囲を返す（プレーンな状態）
    List<Vector2Int> getMovableTiles(UnitController[,] units, TYPE type)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        // ポーン(平民)

        if(TYPE.PAWN == type)
        {
            

            List<Vector2Int> vec = new List<Vector2Int>()
            {
               //(左のマス,右のマス)みたいな感じ。
                new Vector2Int(-1, 1),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int( 1, 0),
                new Vector2Int(-1,-1),
                new Vector2Int( 0,-1),
                new Vector2Int( 1,-1),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue; //端っこまで行ったらその先は行けないようにする。

                // 同じプレイヤーの場所へは行けない
                if (null != units[checkpos.x, checkpos.y]
                    && Player == units[checkpos.x, checkpos.y].Player)
                {
                    continue;
                }

                // 貴族のタグ相手なら行けないようにしたい。
                //そうするためには、まず相手の情報だけを条件式に入れないといけないくて。
                //相手の情報というのは、ゲームシーンのプログラムで採用されており、こちらで処理ができない。
                //定義したコードの呼び出しが出来ればいいのだが。てか、このスクリプトがどこに格納されているのかもわからん。
                //このコードはまず、自分と相手が等しい場合に出てくるコードなので。
                //この場で相手を取得できるコードはない。
                //これの一番、引っかかるポイントは行動範囲が、マスで制御されていて、そのマスがどれぐらい勧めるのかで勧めないのかで制御している。
                //自分が相手に対して何もできないようにしないとダメ。
                //相手の能力というかタグを確認するためには。

                // ルーク（貴族）に行けないようにする。
                //if(自分が、自分のポジションかつ味方の駒がある場合。)動けなくなる。
                /*if (null != units[checkpos.x, checkpos.y]
                     && Player != units[checkpos.x, checkpos.y].Player && TYPE.ROOK == Type)
                 {
                     Debug.Log("反応している");
                     continue;
                 }*/

                if (null != units[checkpos.x, checkpos.y])
                {
                    if (Player != units[checkpos.x, checkpos.y].Player)
                    {
                        Debug.Log("反応している");

                        if (TYPE.ROOK == units[checkpos.x, checkpos.y].Type)
                        {

                            Debug.Log("反応している");
                            continue;
                        }
                        


                    }
                }
                
                




                ret.Add(checkpos);
            }

            

            

            

               
         }

            
        
        
        // ルーク（貴族）
        if(TYPE.ROOK == type)
        {
            int dir = 1;
            if (1 == Player) dir = -1;

            List<Vector2Int> vec = new List<Vector2Int>()
            {
               //(Xのマス,Zのマス)みたいな感じ。
                new Vector2Int(-1, 1),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int( 1, 0),
                new Vector2Int(-1,-1),
                new Vector2Int( 0,-1),
                new Vector2Int( 1,-1),




                new Vector2Int(-1, 2 * dir ),
                new Vector2Int(0 , 2 * dir ),
                new Vector2Int(1 , 2 * dir ),
            };

           

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;

                // 同じプレイヤーの場所へは行けない
                if (null != units[checkpos.x, checkpos.y]
                    && Player == units[checkpos.x, checkpos.y].Player)
                {
                    continue;
                }

                if (null != units[checkpos.x, checkpos.y])
                {
                    if (Player != units[checkpos.x, checkpos.y].Player)
                    {
                        Debug.Log("反応している");

                        if (TYPE.KING == units[checkpos.x, checkpos.y].Type)
                        {

                            Debug.Log("反応している");
                            continue;
                        }



                    }
                }




                ret.Add(checkpos);
            }
        }
        // ナイト（英雄）
        else if( TYPE.KNIGHT == type)
        {
            int dir = 1;
            if (1 == Player) dir = -1;

            List<Vector2Int> vec = new List<Vector2Int>()
            {
               //(Xのマス,Zのマス)みたいな感じ。
                new Vector2Int(-1, 1),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int( 1, 0),
                new Vector2Int(-1,-1),
                new Vector2Int( 0,-1),
                new Vector2Int( 1,-1),

                new Vector2Int( -1,2* dir),
                new Vector2Int( 0, 2* dir),
                new Vector2Int( 1, 2* dir),
                new Vector2Int(-1,-2* dir),
                new Vector2Int( 0,-2* dir),
                new Vector2Int( 1,-2* dir),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;

                // 同じプレイヤーの場所へは行けない
                if (null != units[checkpos.x, checkpos.y]
                    && Player == units[checkpos.x, checkpos.y].Player)
                {
                    continue;
                }

                if (null != units[checkpos.x, checkpos.y])
                {
                    if (Player != units[checkpos.x, checkpos.y].Player)
                    {
                        Debug.Log("反応している");

                        if (TYPE.PAWN == units[checkpos.x, checkpos.y].Type)
                        {

                            Debug.Log("反応している");
                            continue;
                        }



                    }
                }



                ret.Add(checkpos);
            }
        }
        // ビショップ（なし）
        else if(TYPE.BISHOP == type)
        {
            // 上下左右斜めにユニットぶつかるまでどこまでも進める
            List<Vector2Int> vec = new List<Vector2Int>()
            {
                new Vector2Int( 1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int( 1,-1),
                new Vector2Int(-1,-1),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                while (isCheckable(units, checkpos))
                {
                    // 誰かいたら終了
                    if (null != units[checkpos.x, checkpos.y])
                    {
                        if (Player != units[checkpos.x, checkpos.y].Player)
                        {
                            ret.Add(checkpos);
                        }

                        break;
                    }

                    ret.Add(checkpos);
                    checkpos += v;
                }
            }
        }
        // キング（王様）
        else if( TYPE.KING == type)
        {
            int dir = 1;
            if (1 == Player) dir = -1;

            List<Vector2Int> vec = new List<Vector2Int>()
            {
                new Vector2Int(-1, 1),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int( 1, 0),
                new Vector2Int(-1,-1),
                new Vector2Int( 0,-1),
                new Vector2Int( 1,-1),

                new Vector2Int(-1,-2* dir),
                new Vector2Int( 0,-2* dir),
                new Vector2Int( 1,-2* dir),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;

                // 同じプレイヤーの場所へは行けない
                if (null != units[checkpos.x, checkpos.y]
                    && Player == units[checkpos.x, checkpos.y].Player)
                {
                    continue;
                }

                ret.Add(checkpos);
            }

            
           

        }

        return ret;
    }

    // 配列内かどうか
    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        if(    idx.x < 0 || ary.GetLength(0) <= idx.x
            || idx.y < 0 || ary.GetLength(1) <= idx.y )
        {
            return false;
        }

        return true;
    }

    


    // 選択時の処理
    public void SelectUnit(bool select = true)
    {
        Vector3 pos = transform.position;
        pos.y += 2;
        GetComponent<Rigidbody>().isKinematic = true;

        // 選択解除
        if (!select)
        {
            pos.y = 1.35f;
            GetComponent<Rigidbody>().isKinematic = false;
        }

        transform.position = pos;
    }

    // 移動処理
    public void MoveUnit(GameObject tile)
    {
        // 移動時は非選択状態にする
        SelectUnit(false);

        // タイルのポジションから配列番号に戻す
        Vector2Int idx = new Vector2Int(
            (int)tile.transform.position.x + GameSceneDirector.TILE_X / 2,
            (int)tile.transform.position.z + GameSceneDirector.TILE_Y / 2);

        // 新しい場所へ移動
        Vector3 pos = tile.transform.position;
        pos.y = 1.35f;
        transform.position = pos;

        // 移動状態リセット
        Status.Clear();


              
          
        

        // キャスリング
        if(TYPE.KING == Type)
        {
            // 横に2タイル進んだら
            if( 1 < idx.x - Pos.x)
            {
                Status.Add(STATUS.KSIDE_CASTLING);
            }

            if (-1 > idx.x - Pos.x)
            {
                Status.Add(STATUS.QSIDE_CASTLING);
            }
        }

        




        // インデックスの更新
        OldPos = Pos;
        Pos = idx;

        // 置いてからのターンをリセット
        ProgressTurnCount = 0;
    }

    // 前回移動してからのターンをカウント
    public void ProgressTurn()
    {
        // 初動は無視
        if (0 > ProgressTurnCount) return;

        ProgressTurnCount++;

        // アンパッサンフラグチェック
        if( TYPE.PAWN == Type)
        {
            if( 1 < ProgressTurnCount)
            {
                Status.Remove(STATUS.EN_PASSANT);
            }
        }
    }

    // 相手のアンパッサン状態のユニットを返す
    UnitController getEnPassantUnit(UnitController[,] units, Vector2Int pos)
    {
        foreach(var v in units)
        {
            if (null == v) continue;
            if (Player == v.Player) continue;
            if (!v.Status.Contains(STATUS.EN_PASSANT)) continue;

            if (v.OldPos == pos) return v;
        }

        return null;
    }

    // 今回のターンのチェック状態をセット
    public void SetCheckStatus(bool flag = true)
    {
        Status.Remove(STATUS.CHECK);
        if (flag) Status.Add(STATUS.CHECK);
    }
}
