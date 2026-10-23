namespace FeteCam.Services;

public static class DeepLinkService
{
    public const string Scheme = "fetecam";

    public static string? PendingToken { get; set; }
    public static event Action<string>? LinkReceived;

    public static string LogPath => Path.Combine(Path.GetTempPath(), "fetecam_deeplink.log");

    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(LogPath,
                $"{DateTime.Now:HH:mm:ss.fff} [pid {Environment.ProcessId}] {message}{Environment.NewLine}");
        }
        catch { }
    }

    public static void Init()
    {
#if WINDOWS
        try
        {
            RegisterProtocol();
        }
        catch { }

        try
        {
            var current = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent();
            current.Activated += OnActivated;

            var main = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("fetecam-main");
            Log($"Init isMain={ReferenceEquals(main, current)}");
            if (!ReferenceEquals(main, current))
            {
                var token = TokenFromArgs();
                if (token is not null)
                {
                    Log("Second instance: signal token");
                    SignalToken(token);
                }

                var args = current.GetActivatedEventArgs();
                Log($"Redirect kind={args.Kind} dataType={args.Data?.GetType().FullName}");
                try
                {
                    main.RedirectActivationToAsync(args);
                }
                catch (Exception ex) { Log($"Redirect ERROR {ex.Message}"); }
                Thread.Sleep(1000);
                Environment.Exit(0);
                return;
            }

            StartTokenListener();
        }
        catch (Exception ex) { Log($"Init ERROR {ex.Message}"); }

        ParseCommandLine();
#endif
    }

    private static readonly string PendingFile =
        Path.Combine(Path.GetTempPath(), "fetecam_pending.link");
    private const string LinkEventName = "FeteCamLinkEvent";

    private static string? TokenFromArgs()
    {
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            var token = ExtractToken(arg);
            if (token is not null)
                return token;
        }
        return null;
    }

    private static void SignalToken(string token)
    {
        try { File.WriteAllText(PendingFile, token); } catch { }
        try
        {
            using var ev = new EventWaitHandle(false, EventResetMode.AutoReset, LinkEventName);
            ev.Set();
        }
        catch (Exception ex) { Log($"Signal ERROR {ex.Message}"); }
    }

    private static void StartTokenListener()
    {
        try { File.Delete(PendingFile); } catch { }

        var thread = new Thread(() =>
        {
            try
            {
                using var ev = new EventWaitHandle(false, EventResetMode.AutoReset, LinkEventName);
                while (true)
                {
                    ev.WaitOne();
                    try
                    {
                        if (!File.Exists(PendingFile))
                            continue;
                        var token = File.ReadAllText(PendingFile).Trim();
                        File.Delete(PendingFile);
                        if (token.Length > 0)
                            Receive($"fetecam://invite/{token}");
                    }
                    catch (Exception ex) { Log($"Listener ERROR {ex.Message}"); }
                }
            }
            catch (Exception ex) { Log($"Listener FATAL {ex.Message}"); }
        });
        thread.IsBackground = true;
        thread.Start();
        Log("Token listener started");
    }

    public static void ParseCommandLine()
    {
        foreach (var arg in Environment.GetCommandLineArgs())
        {
            var token = ExtractToken(arg);
            if (token is not null)
            {
                PendingToken = token;
                Log("ParseCommandLine: pending token set");
                return;
            }
        }
        Log("ParseCommandLine: no token in args");
    }

    public static string? ExtractToken(string uriOrArg)
    {
        if (string.IsNullOrWhiteSpace(uriOrArg))
            return null;

        if (Uri.TryCreate(uriOrArg.Trim(), UriKind.Absolute, out var uri))
        {
            var direct = FromUri(uri);
            if (direct is not null)
                return direct;
        }

        var idx = uriOrArg.IndexOf("fetecam://", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var end = uriOrArg.IndexOfAny(new[] { ' ', '"', '\n', '\r', '\t' }, idx);
            var piece = end < 0 ? uriOrArg[idx..] : uriOrArg[idx..end];
            if (Uri.TryCreate(piece, UriKind.Absolute, out var pieceUri) && FromUri(pieceUri) is string token)
                return token;
        }

        return null;
    }

    private static string? FromUri(Uri uri)
    {
        if (uri.Scheme.Equals(Scheme, StringComparison.OrdinalIgnoreCase)
            && uri.Host.Equals("invite", StringComparison.OrdinalIgnoreCase))
        {
            var token = uri.AbsolutePath.Trim('/');
            return string.IsNullOrEmpty(token) ? null : token;
        }

        if ((uri.Scheme == "http" || uri.Scheme == "https")
            && uri.AbsolutePath.StartsWith("/i/", StringComparison.Ordinal))
        {
            var token = uri.AbsolutePath[3..].Trim('/');
            return string.IsNullOrEmpty(token) ? null : token;
        }

        return null;
    }

    public static void Receive(string uri)
    {
        Log($"Receive '{uri}'");
        var token = ExtractToken(uri);
        if (token is null)
        {
            Log("Receive: token=null");
            return;
        }

        PendingToken = token;
        var handler = LinkReceived;
        if (handler is not null)
        {
            Log($"Receive: dispatch token={token[..Math.Min(8, token.Length)]}...");
            MainThread.BeginInvokeOnMainThread(() => handler(token));
        }
        else
        {
            PendingToken = token;
            Log("Receive: no handler, stored");
        }
    }

#if WINDOWS
    private static void OnActivated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments args)
    {
        Log($"OnActivated kind={args.Kind} dataType={args.Data?.GetType().FullName}");
        if (args.Data is Windows.ApplicationModel.Activation.ProtocolActivatedEventArgs protocol
            && protocol.Uri is not null)
        {
            Receive(protocol.Uri.ToString());
            return;
        }

        var data = args.Data;
        if (data is null)
            return;

        var dataType = data.GetType();
        if (dataType.Name.Contains("CommandLine", StringComparison.OrdinalIgnoreCase))
        {
            var prop = dataType.GetProperty("Arguments");
            if (prop?.GetValue(data) is string arguments)
            {
                var token = ExtractToken(arguments);
                if (token is not null)
                    Receive($"fetecam://invite/{token}");
            }
        }
    }

    private static void RegisterProtocol()
    {
        using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\fetecam");
        key.SetValue("", "URL:FeteCam Protocol");
        key.SetValue("URL Protocol", "");

        using var command = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Classes\fetecam\shell\open\command");
        var exe = Environment.ProcessPath ?? "FeteCam.exe";
        command.SetValue("", $"\"{exe}\" \"%1\"");
    }
#endif
}
