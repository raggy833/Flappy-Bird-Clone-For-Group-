using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour {

	// 途中にインスタンスの作成と削除はせずに、最初に生成したもので管理する

	// To keep track of the same type
	// To check if an object is in use or not
	class PoolObject {
		public Transform transform;
		public bool inUse;                                          // To determine if it is in use or not
		public PoolObject(Transform t) { transform = t; }           // Constructor
		public void Use() { inUse = true; }                         // inUseをtrueにする
		public void Dispose() { inUse = false; }                    // inUseをfalseにする
	}

	[System.Serializable]
	public struct YSpawnRange {                                     // パイプのYポジション
		public float min;                                           // 最小のYポジション
		public float max;                                           // 最大のYポジション
	}

	public GameObject Prefab;                                       // 生成するオブジェクトのタイプ
	public int poolSize;                                            // 最初に作成するオブジェクトの数
	public float shiftSpeed;                                        // 左右に移動するスピード
	public float spawnRate;                                         // 移動（生成）する比率

	public YSpawnRange ySpawnRange;                                 // ySpawnRangeのレファレンス
	public Vector3 defaultSpawnPos;                                 // デフォルトで生成されるポジション
	public bool spawnImmediate;                                     // Prewarmの有無
	public Vector3 immediateSpawnPos;                               // Prewarmの場合のポジション
	public Vector2 targetAspectRatio;                               // 画面内で生成されないように画面の情報を管理する

	float spawnTimer;                                               // 
	float targetAspect;                                             // 
	PoolObject[] poolObjects;                                       // poolObjectsのレファレンス
	GameManager gameManager;                                        // gameManagerのレファレンス

	private void Awake() {
		Configure();                                                // 
	}

	private void Start() {                                          // GameManagerはAwakeで作成されているので、Startで設定すればnullでないことが保証される
		gameManager = GameManager.Instance;                         // GameManagerのインスタンスを

	}

	private void OnEnable() {
		GameManager.OnGameOverConfirmed += OnGameOverConfirmed;     // サブスクライブに足す
	}

	private void OnDisable() {
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;     // サブスクライブから外す
	}

	void OnGameOverConfirmed() {
		for (int i = 0; i < poolObjects.Length; i++) {                          // poolObjectをループする
			poolObjects[i].Dispose();                                           // オブジェクトにDisposeを呼ぶ
			poolObjects[i].transform.position = Vector3.one * 1000;             // 一時的に画面外に移動させる
		}
		if (spawnImmediate) {                                                   // 
			SpawnImmediate();                                                   // 
		}
	}

	private void Update() {
		if (gameManager.GameOver) return;                                       // GameOverの場合returnする
		Shift();                                                                // Shiftを呼ぶ
		spawnTimer += Time.deltaTime;                                           // spawnTimerをカウントアップする
		if (spawnTimer > spawnRate) {                                           // spawnTimerがspawnRateより大きい場合
			Spawn();                                                            // Spawnを呼ぶ
			spawnTimer = 0;                                                     // spawnTimerをリセットする
		}
	}

	void Configure() {
		targetAspect = targetAspectRatio.x / targetAspectRatio.y;               // 縦横の比率を確認
		poolObjects = new PoolObject[poolSize];                                 // poolObjectのインスタンスを作成
		for (int i = 0; i < poolObjects.Length; i++) {                          // poolObjectsをループする
			GameObject gameobject = Instantiate(Prefab) as GameObject;          // Gameobjectをinstantiateする
			Transform t = gameobject.transform;                                 // Transformを取得する
			t.SetParent(transform);                                             // このスクリプトを保持しているParentオブジェクトを設定する
			t.position = Vector3.one * 1000;                                    // 一時的に画面外に保持する
			poolObjects[i] = new PoolObject(t);                                 // 設定された情報をオブジェクトに設定する
		}
		if (spawnImmediate) {                                                   // 
			SpawnImmediate();                                                   // 
		}
	}

	void Spawn() {
		Transform t = GetPoolObject();                                          // GetPoolObjectでreturnされる使用可能なobjectを保持する
		if (t == null) return;                                                  // nullチェックを行う
		Vector3 pos = Vector3.zero;                                             // posを初期化する
		pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;        // x posを設定する
		pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);                 // y posを設定する
		t.position = pos;                                                       // posをオブジェクトのポジションに設定する
	}

	void SpawnImmediate() {
		Transform t = GetPoolObject();                                          // GetPoolObjectでreturnされる使用可能なobjectを保持する
		if (t == null) return;                                                  // nullチェックを行う
		Vector3 pos = Vector3.zero;                                             // posを初期化する
		pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;      // x posを設定する
		pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);                 // y posを設定する
		t.position = pos;                                                       // posをオブジェクトのポジションに設定する
		Spawn();                                                                // 
	}

	void Shift() {                                                                                              // オブジェクトを移動させる
		for (int i = 0; i < poolObjects.Length; i++) {                                                          // poolObjectをループする
			poolObjects[i].transform.position += Vector3.left * shiftSpeed * Time.deltaTime;                    // shiftSpeedで移動させる
			CheckDisposeObject(poolObjects[i]);                                                                 // CheckDisposeObjectを呼ぶ
		}
	}

	void CheckDisposeObject(PoolObject poolObject) {                                                            // 特定のオブジェクトを削除する
		if (poolObject.transform.position.x < (-defaultSpawnPos.x * Camera.main.aspect) / targetAspect) {       // オブジェクトが画面外にいる場合
			System.Console.WriteLine(poolObject.transform.position.x);
			System.Console.WriteLine(-defaultSpawnPos.x);
			poolObject.Dispose();                                                                               // Disposeを呼ぶ
			poolObject.transform.position = Vector3.one * 1000;                                                 // 再度使用されるまで一時的に画面外に保持する
		}
	}

	Transform GetPoolObject() {                                                 // 特定のオブジェクトを取得する
		for (int i = 0; i < poolObjects.Length; i++) {                          // poolObjectsをループする
			if (!poolObjects[i].inUse) {                                        // オブジェクトがinUseでなかった場合
				poolObjects[i].Use();                                           // オブジェクトをinUseに変換
				return poolObjects[i].transform;                                // オブジェクトをreturnする
			}
		}
		return null;
	}

}
