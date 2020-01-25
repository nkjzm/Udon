using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Udon
{
    public class ConfirmPopup : MonoBehaviour
    {
        // Google Geocoding APIのKey
        [SerializeField] string API_KEY = null;
        // ポップアップ本体
        [SerializeField] RectTransform Window = null;

        // 年齢に関するコンポーネント
        [SerializeField] ToggleGroup GenerationToggles = null;

        // 位置情報に香川県を設定するフラグ
        [SerializeField] bool TestDummyLocation = false;
        // 位置情報に関するコンポーネント
        [SerializeField] Button EnableLocation = null, EnableLocationManually = null;
        [SerializeField] Text LocationProgress = null, LocationSettingWarning = null;
        [SerializeField] ToggleGroup LocationToggles = null;
        // ポップアップを閉じるボタン
        [SerializeField] Button Clsoe = null;
        // ゲーム制限の状態を表示する
        [SerializeField] GameObject InfomationArea = null;
        [SerializeField] Text Infomation = null;
        // ゲーム開始ボタン
        [SerializeField] Button Launch = null;
        // プライバシーポリシー
        [SerializeField] Button PrivacyPolicy = null;
        Action<bool> onComplete = null;

        void Awake()
        {
            LocationProgress.gameObject.SetActive(false);

            var LocationManually = PlayerPrefs.GetInt("LocationManually", -1);
            LocationToggles.gameObject.SetActive(LocationManually == 1);
            EnableLocationManually.interactable = LocationManually != 1;
            if (LocationManually == 0)
            {
                StartCoroutine(GetLocation());
            }

            InfomationArea.SetActive(false);
            Launch.interactable = false;
        }

        void Start()
        {
            // 年齢を設定する
            var generationStatus = (GenerationStatus)PlayerPrefs.GetInt("Generation", -1);
            foreach (var toggle in GenerationToggles.GetComponentsInChildren<Toggle>())
            {
                var status = (GenerationStatus)int.Parse(toggle.name);
                toggle.isOn = status == generationStatus;
                toggle.onValueChanged.AddListener(flg =>
                {
                    if (!flg) { return; }
                    PlayerPrefs.SetInt("Generation", (int)status);
                    UpdateInfomation();
                });
            }

            // 位置情報を有効にする
            EnableLocation.onClick.AddListener(() =>
            {
                StartCoroutine(GetLocation());
            });
            // 位置情報を手動で設定にする
            EnableLocationManually.onClick.AddListener(() =>
            {
                EnableLocationManually.interactable = false;
                LocationToggles.gameObject.SetActive(true);
                foreach (var toggle in LocationToggles.GetComponentsInChildren<Toggle>())
                {
                    toggle.isOn = false;
                }
                // 一度未設定にする
                PlayerPrefs.SetInt("InKagawa", (int)LocationStatus.Undefined);
                LocationProgress.gameObject.SetActive(false);
                UpdateInfomation();
            });
            var inKagawaStatus = (LocationStatus)PlayerPrefs.GetInt("InKagawa", -1);
            foreach (var toggle in LocationToggles.GetComponentsInChildren<Toggle>())
            {
                var status = (LocationStatus)int.Parse(toggle.name);
                toggle.isOn = status == inKagawaStatus;
                toggle.onValueChanged.AddListener(flg =>
                {
                    if (!flg) { return; }
                    PlayerPrefs.SetInt("InKagawa", (int)status);
                    PlayerPrefs.SetInt("LocationManually", 1);
                    UpdateInfomation();
                });
            }

            // ゲームを始める
            Launch.onClick.AddListener(() =>
            {
                Destroy(gameObject);
                onComplete?.Invoke(true);
            });
            // ポップアップを閉じる
            Clsoe.onClick.AddListener(() =>
            {
                Destroy(gameObject);
                onComplete?.Invoke(false);
            });

            // プライバシーポリシーを開く
            PrivacyPolicy.onClick.AddListener(() => Application.OpenURL("https://example.com/"));

            UpdateInfomation();
        }

        public void Open(Action<bool> onComplete = null)
        {
            this.onComplete = onComplete;
            StartCoroutine(OpenAnimation());
        }

        IEnumerator OpenAnimation()
        {
            Window.anchoredPosition = new Vector2(0, -2500f);
            var time = .5f;
            while (time > 0f)
            {
                time -= Time.deltaTime;
                Window.anchoredPosition = new Vector2(0, -2500f * 4 * Mathf.Pow(time, 2));
                yield return null;
            }
            Window.anchoredPosition = Vector2.zero;
        }

        void UpdateInfomation()
        {
            var generationStatus = (GenerationStatus)PlayerPrefs.GetInt("Generation", -1);
            var inKagawaStatus = (LocationStatus)PlayerPrefs.GetInt("InKagawa", -1);

            // 両方設定されていない場合は表示しない
            if (generationStatus == GenerationStatus.Undefined || inKagawaStatus == LocationStatus.Undefined)
            {
                InfomationArea.SetActive(false);
                Launch.interactable = false;
                return;
            }

            // 表示を有効に
            InfomationArea.SetActive(true);
            Launch.interactable = true;

            if (generationStatus == GenerationStatus.Upper18 || inKagawaStatus == LocationStatus.OutKagawa)
            {
                Infomation.text =
                $"あなたのプレイ制限はありません\n" +
                $"節度を守ってお楽しみください";
            }
            else
            {
                var timeLimit = generationStatus == GenerationStatus.Upper15 ? "10" : "9";
                Infomation.text =
                $"あなたの1日のプレイ可能時間は<color=red>60分</color>です\n" +
                $"また<color=red>夜{timeLimit}時以降</color>のプレイは制限されます";
            }
        }

        IEnumerator GetLocation()
        {
            if (TestDummyLocation)
            {
                // 香川県庁の座標に置き換える
                var lat = 34.340117f;
                var lng = 134.043312f;
                StartCoroutine(GetAreaName(lat, lng));
                yield break;
            }

            // 端末自体の位置情報が有効か
            if (!Input.location.isEnabledByUser)
            {
                LocationSettingWarning.text = $"「設定」アプリから位置情報を有効にしてください";
                LocationSettingWarning.color = Color.red;
                yield break;
            }

            while (true)
            {
                var status = Input.location.status;
                switch (status)
                {
                    case LocationServiceStatus.Stopped:
                        Input.location.Start();
                        break;
                    // 位置情報が有効になった場合
                    case LocationServiceStatus.Running:
                        // Reverse Geocoding APIから件名を取得
                        var data = Input.location.lastData;
                        StartCoroutine(GetAreaName(data.latitude, data.longitude));
                        yield break;
                    // ユーザーが位置情報を許可しなかった場合
                    case LocationServiceStatus.Failed:
                        LocationSettingWarning.text = $"「設定」アプリから位置情報を有効にしてください";
                        LocationSettingWarning.color = Color.red;
                        yield break;
                    default:
                        break;
                }
                // 1秒毎に状態を再取得
                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator GetAreaName(float lat, float lng)
        {
            LocationProgress.gameObject.SetActive(true);
            LocationProgress.text = $"位置情報を取得中";

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?" +
            $"latlng={lat},{lng}&result_type=administrative_area_level_1&key={API_KEY}&language=ja";
            var request = UnityWebRequest.Get(url);

            yield return request.SendWebRequest();

            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log(request.error);
                yield break;
            }

            var response = JsonUtility.FromJson<Response>(request.downloadHandler.text);

            var prefecture = response.results?[0].address_components?[0].long_name;
            LocationProgress.text = $"あなたの現在地は<color=red>{prefecture}</color>です";

            PlayerPrefs.SetInt("InKagawa", prefecture == "香川県" ? 1 : 0);
            PlayerPrefs.SetInt("LocationManually", 0);
            EnableLocationManually.interactable = true;
            LocationToggles.gameObject.SetActive(false);

            UpdateInfomation();
        }

        [System.Serializable] class Response { public Result[] results; }
        [System.Serializable] class Result { public Adress[] address_components; }
        [System.Serializable] class Adress { public string long_name; }
    }
}