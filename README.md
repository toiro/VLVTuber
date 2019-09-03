# VLVTuber
Vive + Unity + FinalIK で VTuber の運用をするためのサンプル ＆ ライブラリ

## Description
Vive で VTuber をやるのに、とりあえず動けばいいという程度であればもう自力で開発をする必要はすでにありません。
バーチャルモーションキャプチャ―やバーチャルキャストやLuppetやなど、十分なツールやサービスがいろいろあります。
それでも自力で開発をしなければならないという人のための、ただキャラクターモデルを Vive で動かせるところからさらに先へ進むサンプルとライブラリです。
好きに使ってください。

## Feature
Unity エディターで各種設定をしつつ、ビルドしたりしなかったりしながら画面出力をキャプチャして録画や配信を行うような運用を想定しています。
- マルチディスプレイへの出力
- VRIK の動的なアタッチ
- キャラクターのモデルの動的な読込
- 表情切り替えの間の補間
- 目を閉じたり閉じ気味の表情とまばたきの干渉の回避
- 頭のトラッキングを HMD からトラッカーとの切り替え
- 腰・足のトラッキング（最大6点）
- トラッキング点数の切り替え
- トラッカーの回転オフセットの自動キャリブレーション
- キャリブレーション結果の ScriptableObject への保存
- Vive コントローラーでの手のポーズ操作
- モーションキャプチャーのブレ軽減
- モーションキャプチャーのシーンと背景のシーンの分離
- 背景シーンの動的読み込み
- 背景のシーンごとのカメラやライトの設定の保持
- キーボードへの割り当ての一元管理
- キーボードに割り当てた機能の一覧表示 ＆ マウス操作

## Requirements
Unity 2018.2.21f1 で動作確認しています。2018.3 以降でも動くはずですが「SteamVR Input」の設定が必要です。
以下の Asset が必要です。
- [FinalIK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)
- [Vive Input Utility](https://github.com/ViveSoftware/ViveInputUtility-Unity)
- [UniVRM](https://github.com/vrm-c/UniVRM)
- [Oculus Lipsync Unity](https://developer.oculus.com/downloads/package/oculus-lipsync-unity)
- [AniLipSync](https://github.com/XVI/AniLipSync)
- [DebugUi](https://github.com/hiryma/UnitySamples/tree/master/DebugUi)
- [SteamVR Plugin](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)

Oculus Lipsync Unity は最新のものだとAniLipSyncが対応していないことがあります。

SteamVR Plugin は 2018.2 では必須ではありません。2018.3 以降で SteamVR Input の設定をする場合は[ここ](https://github.com/ViveSoftware/ViveInputUtility-Unity/wiki/SteamVR-Input-System-Compatibility)を参考にしてください。

サンプル用のモデルとして[ニコニ立体ちゃん](https://3d.nicovideo.jp/works/td14712)と[ニコニ立体ちゃん (VRM)](https://3d.nicovideo.jp/works/td32797)を使用します。

## License
WTFPL
