using UnityEngine;

public enum GridEffect //マス目の種類の一覧(効果)
{
    Normal,
    Advance,
    Back
}

[System.Serializable]
public struct SpecialGrid // 座標ではなく「何番目のマスに何の効果をつけるか」だけを指定
{
    public int gridIndex; // 何番目のマスか
    public GridEffect effect; // マス効果
}

public class BoardManager : MonoBehaviour
{
    [Header("マスの見た目")]
    public GameObject gridPrefab; //AssetsのGridSquareを雛型とする

    [Header("自動生成設定")]
    [SerializeField] private int totalGridCount = 50; // 生成したいマスの合計数
    [SerializeField] private float gridSpacing = 2f; // マスとマスの間隔（X軸方向）
    [SerializeField] private Vector3 startPosition = Vector3.zero; // スタート位置

    [Header("特殊マスの個別設定（任意）")]
    [SerializeField] private SpecialGrid[] specialGrids; // 効果を変えたいマスだけここに入力

    private Transform[] generatedWaypoints; //Transform型(座標や向きなどを指定する型)を格納する配列を宣言
    private GridEffect[] generatedGridEffects; // 生成されたマスの効果を保持

    void Awake()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        if (gridPrefab == null || totalGridCount <= 0) return;
        
        generatedWaypoints = new Transform[totalGridCount]; //Transform型をマスの数だけ格納可能な配列を作る
        generatedGridEffects = new GridEffect[totalGridCount];

        for (int i = 0; i < totalGridCount; i++)　  //  全マスをデフォルトの Normal で初期化
        {
            generatedGridEffects[i] = GridEffect.Normal;
        }

        if (specialGrids != null)　    // Inspector で指定された特殊マスの効果だけを上書き
        {
            foreach (var special in specialGrids)
            {
                if (special.gridIndex >= 0 && special.gridIndex < totalGridCount)
                {
                    generatedGridEffects[special.gridIndex] = special.effect;
                }
            }
        }

        for (int i = 0; i < totalGridCount; i++)
        {
            Vector3 pos = startPosition + new Vector3(i * gridSpacing, 0, 0);
            // gridDataListの要素数が足りない場合は安全にNormal扱いにする
            GridEffect currentEffect = generatedGridEffects[i];
            
            GameObject newGrid = Instantiate(gridPrefab, pos, Quaternion.identity); //gridPrefabオブジェクトのクローンをposの位置にそのままの向きで作る
            newGrid.name = $"Grid_{i} ({currentEffect})";
            newGrid.transform.SetParent(this.transform); //生成したマスをBoardManagerの子オブジェクトにする

            Renderer rend = newGrid.GetComponent<Renderer>();
            if (rend != null) 
            {
                if (currentEffect == GridEffect.Advance) rend.material.color = Color.green;
                if (currentEffect == GridEffect.Back) rend.material.color = Color.red;
                if (i == 0 || i == totalGridCount - 1) newGrid.transform.localScale = new Vector3(2.2f, 2.2f, 0.1f);
            }

            generatedWaypoints[i] = newGrid.transform;
         
        }

    }

    public Vector3 GetWaypointPosition(int index) //(主に)Player.csに対してindexマス目の座標を返す
    {
        if (generatedWaypoints == null || generatedWaypoints.Length == 0) return Vector3.zero;

        // ゴールを超えたら最後のマス（totalGridCount - 1）の位置を返す
        if (index >= generatedWaypoints.Length)
        {
            index = generatedWaypoints.Length - 1;
        }

        if (generatedWaypoints[index] != null)
        {
            return generatedWaypoints[index].position;
        }

        return Vector3.zero;

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
        if (generatedGridEffects == null || index < 0 || index >= generatedGridEffects.Length)  // 配列が未生成、またはインデックスが範囲外の場合は Normal を返す
        {
            return GridEffect.Normal;
        }
        
        return generatedGridEffects[index]; // 正しいインデックスの場合は該当するマスの効果を返す
    }

}
