using UnityEngine;

namespace Udon.Example
{
    public class Title : MonoBehaviour
    {
        [SerializeField] Canvas Canvas = null;
        [SerializeField] Udon.ConfirmPopup Prefab = null;
        void Start()
        {
            var popup = Instantiate(Prefab, Canvas.transform);
            popup.Open(onComplete: flg =>
            {
                Debug.Log(flg ? "設定完了" : "未完了");
            });
        }
    }
}
