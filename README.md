# Udon

![](https://user-images.githubusercontent.com/7017772/73128795-15f51f00-4018-11ea-9f30-0b19a34192d9.gif)

_Gif画像にはリポジトリには含まれていないフォント([URW MARU GOTHIC](https://jp.designcuts.com/product/greatest-japanese-fonts-bundle/))と画像アセット([Simple UI](https://assetstore.unity.com/packages/2d/gui/icons/simple-ui-103969))を使用しています_

## Environment

- macOS Catalina 10.15.2
- Unity 2019.2.14f1
- iOS 13.3

## Get Started

[Releases](https://github.com/nkjzm/Udon/releases)から最新の`*.unitypackage`をダウンロードし、Unityプロジェクトにインポートしてください。[Google Geocoding API](https://developers.google.com/maps/documentation/geocoding)を使用しているため、別途API Keyの取得が必要です。

```.cs
var popup = Instantiate(Prefab, Canvas.transform);
popup.Open(onComplete: flg =>
{
    Debug.Log(flg ? "設定完了" : "未完了");
});
```

表示の際には`Instantiate`メソッドで生成し、`Open()`メソッドを呼んでください。

また、位置情報の取得の際にはPlayer SettingからiOS/Other Settings/Location Usage Descriptionに使用用途を記載する必要がある点にも注意してください。

## LICENSE

[MIT](https://github.com/nkjzm/Udon/blob/master/LICENSE)

## Author

Twitter: [@nkjzm](https://twitter.com/nkjzm)
