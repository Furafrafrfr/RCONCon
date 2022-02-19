# RCON Controller

Minecraft 用の RCON のクライアントライブラリ。4096 バイトを超えるレスポンスには対応していません。

## サンプルコード

```csharp
class App
{
    public static void Main()
    {
        RCONController rcon = new RCONController("127.0.0.1", 25575);
        rcon.Authenticate("testing");
        if (rcon.IsAuthenticated)
        {
            string response = rcon.SendCommand("/list");
            Console.WriteLine(response);
        }
    }
}
```

## `RCONController`

### メソッド

#### `void RCONController.Authenticate(string Password)`

サーバーに接続・認証しコマンドを送信できるようにする。

- `string Password` サーバーの RCON のパスワード。

#### `string RCONController.SendCommand(string Command)`

サーバーにコマンドを送信する。Command に ASCII に変換できない文字が含まれていた場合例外が発生します。

- `string Command` サーバーで実行するコマンド。

### プロパティ

#### `bool RCONController.IsAuthenticated`

認証済みの場合`true`、認証が済んでいない場合`false`
