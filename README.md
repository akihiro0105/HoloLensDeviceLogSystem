# DeviceLogSystem
- Twitter : [@akihiro01051](https://twitter.com/akihiro01051)

## 動作環境
- Windows10 Creators Update
- Unity 2017.4
- Visual Studio 2017
- HoloLens RS4
- Windows MixedReality Device

----------

## 概要
### DeviceLogSystem
- HoloLensの基準オブジェクトからの位置をUDPで送信するUnityプロジェクト
- WPFプロジェクトで保存された位置情報をUnity上で確認するUnityプロジェクト

### DeviceLogReceiverWPF
- UDPで送信された位置情報をWPFプロジェクトで受信，保存するVisual Studioプロジェクト

## 利用パッケージ
- [HoloLensModule](https://github.com/akihiro0105/HoloLensModule)

## 内容
### DeviceLogSystem
- DeviceLogSystemSample
    + targetに設定されたオブジェクトの位置をUDPで送信するサンプルです

- DeviceLogSystemViewerSample
    + targetに設定されたオブジェクトに```..\DeviceLogReceiverWPF\DeviceLogReceiverWPF\bin\Debug\Transform.txt```に保存された位置情報を再生します

### DeviceLogReceiverWPF
- DeviceLogReceiverWPF
    + UDPで送信された位置情報を受信して```Transform.txt```に保存します
    + HoloLensのRestAPIを利用してバッテリー残量を取得します
