using UnityEngine;

public enum GridEffect //マス目の種類の一覧(効果)
{
    Normal,
    Advance,
    Back
}

[System.Serializable]
public struct GridData //Inspectorタブでマスの座標と種類を指定する。
{
    public Vector3 position;
    public GridEffect effect;
}

public class BoardManager : MonoBehaviour
{
    [Header("マスの見た目")]
    public GameObject gridPrefab; //AssetsのGridSquareを雛型とする
    
    [Header("マスの座標と種類を入力")]
    public GridData[] gridDataList; //Inspectorタブに入力したマスの座標と種類を取得


    private Transform[] generatedWaypoints; //Transform型(座標や向きなどを指定する型)を格納する配列を宣言

    void Awake()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        if (gridDataList == null || gridDataList.Length == 0) return;
        
        generatedWaypoints = new Transform[gridDataList.Length]; //Transform型をマスの数だけ格納可能な配列を作る

        for (int i = 0; i < gridDataList.Length; i++)
        {
            Vector3 pos = gridDataList[i].position;
            
            GameObject newGrid = Instantiate(gridPrefab, pos, Quaternion.identity); //gridPrefabオブジェクトのクローンをposの位置にそのままの向きで作る
            newGrid.name = $"Grid_{i} ({gridDataList[i].effect})";
            newGrid.transform.SetParent(this.transform); //生成したマスをBoardManagerの子オブジェクトにする

            Renderer rend = newGrid.GetComponent<Renderer>();
            if (rend != null) 
            {
                //マスの種類に応じて色を変更する
                if (gridDataList[i].effect == GridEffect.Advance) rend.material.color = Color.green;
                if (gridDataList[i].effect == GridEffect.Back) rend.material.color = Color.red;

                if (i == 0 || i == gridDataList.Length - 1) newGrid.transform.localScale = new Vector3(2.2f, 2.2f, 0.1f);
            }

            generatedWaypoints[i] = newGrid.transform;
         
        }

    }

    public Vector3 GetWaypointPosition(int index) //(主に)Player.csに対してindexマス目の座標を返す
    {
        if (generatedWaypoints == null || generatedWaypoints.Length == 0) return Vector3.zero;

        if (index >= generatedWaypoints.Length)
        {
            return generatedWaypoints[generatedWaypoints.Length - 1].position;
        }
        return generatedWaypoints[index].position;

    }

    public int GetTotalWaypoints() //Player.csに対してマス数の合計を返す
{
    // 配列が作られているか確認（Nullチェック）
    if (generatedWaypoints != null)
    {
        return generatedWaypoints.Length; // マスの個数を返す
    }
    else
    {
        return 0; // まだ作られていなければ 0 を返す
    }
}

    public GridEffect GetGridEffect(int index) //Player.csに対してindexマス目の種類(効果)を返す。
    {
        if (gridDataList == null || index >= gridDataList.Length) return GridEffect.Normal;
        return gridDataList[index].effect;
    }

}
