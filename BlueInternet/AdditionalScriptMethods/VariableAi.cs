// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BlueScript.Variables;

public class VariableAi : Variable {

    #region Fields

    public static readonly List<string> AiVal = [ShortName_Variable];

    // Ein einzelner, statischer HttpClient ist die empfohlene Vorgehensweise in .NET.
    // Er wird für alle OpenAI-kompatiblen Endpunkte wiederverwendet.
    // Authentifizierung und Ziel-URL werden pro Anfrage gesetzt, nicht am Client.
    private static readonly HttpClient HttpClient = new() {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private string? _apiKey;
    private string? _endpoint;
    private string? _model;

    #endregion

    #region Constructors

    public VariableAi(string name, string? apiKey, string? endpoint, string? model, bool ronly, string comment) : base(name, ronly, comment) {
        _apiKey = apiKey;
        _endpoint = endpoint;
        _model = model;
    }

    /// <summary>
    /// Wichtig für: GetEnumerableOfType&lt;Variable&gt;("NAME");
    /// </summary>
    public VariableAi(string name) : this(name, null, null, null, true, string.Empty) { }

    public VariableAi(string? apiKey, string? endpoint, string? model) : this(DummyName(), apiKey, endpoint, model, true, string.Empty) { }

    #endregion

    #region Properties

    public static string ClassId => "ari";
    public static string ShortName_Variable => "*ari";

    public string? ApiKey {
        get => _apiKey;
        set {
            if (ReadOnly) { return; }
            _apiKey = value;
        }
    }

    public override int CheckOrder => 99;

    public string? Endpoint {
        get => _endpoint;
        set {
            if (ReadOnly) { return; }
            _endpoint = value;
        }
    }

    public override bool GetFromStringPossible => false;
    public override bool IsNullOrEmpty => string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_endpoint) || string.IsNullOrWhiteSpace(_model);

    public string? Model {
        get => _model;
        set {
            if (ReadOnly) { return; }
            _model = value;
        }
    }

    public override string SearchValue => ReadableText;
    public override bool ToStringPossible => false;
    public override string ValueForCell => string.Empty;

    #endregion

    #region Methods

    /// <summary>
    /// Führt einen OpenAI-kompatiblen Chat-Completion-Aufruf aus und gibt die
    /// Text-Antwort des Modells zurück. Wirft bei Fehlern keine Exception,
    /// sondern gibt null zurück.
    /// </summary>
    public static async Task<string?> AskAsync(string? apiKey, string? endpoint, string? model, string prompt, Bitmap? image) {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(model)) {
            return null;
        }

        var body = new JsonObject {
            ["model"] = model,
            ["max_tokens"] = image is null ? 1024 : 4096,
            ["temperature"] = 1.0
        };

        if (image is null) {
            body["messages"] = new JsonArray {
                new JsonObject {
                    ["role"] = "user",
                    ["content"] = prompt
                }
            };
        } else {
            var base64 = Converter.BitmapToBase64(image, ImageFormat.Jpeg);
            body["messages"] = new JsonArray {
                new JsonObject {
                    ["role"] = "user",
                    ["content"] = new JsonArray {
                        new JsonObject {
                            ["type"] = "image_url",
                            ["image_url"] = new JsonObject {
                                ["url"] = "data:image/jpeg;base64," + base64
                            }
                        },
                        new JsonObject {
                            ["type"] = "text",
                            ["text"] = prompt
                        }
                    }
                }
            };
        }

        var json = await SendAsync(apiKey, endpoint, "chat/completions", body).ConfigureAwait(false);
        if (json is null) { return null; }

        try {
            var root = JsonNode.Parse(json);
            // Fehler-Antworten haben ein "error"-Feld
            if (root?["error"]?["message"] is not null) {
                return null;
            }
            return root?["choices"]?[0]?["message"]?["content"]?.ToString();
        } catch (Exception ex) {
            Develop.DebugPrint("Antwort konnte nicht geparsed werden.", ex);
            return null;
        }
    }

    /// <summary>
    /// Führt einen OpenAI-kompatiblen Image-Generation-Aufruf aus und gibt das
    /// erzeugte Bild als Bitmap zurück. Wirft bei Fehlern keine Exception,
    /// sondern gibt null zurück.
    /// </summary>
    public static async Task<Bitmap?> GenerateImageAsync(string? apiKey, string? endpoint, string imageModel, string prompt) {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(imageModel)) {
            return null;
        }

        var body = new JsonObject {
            ["model"] = imageModel,
            ["prompt"] = prompt,
            ["n"] = 1,
            ["size"] = "1024x1024",
            ["response_format"] = "b64_json"
        };

        var json = await SendAsync(apiKey, endpoint, "images/generations", body).ConfigureAwait(false);
        if (json is null) { return null; }

        try {
            var root = JsonNode.Parse(json);
            if (root?["error"]?["message"] is not null) {
                return null;
            }

            // Bevorzugt b64_json auswerten, alternativ URL herunterladen.
            if (root?["data"]?[0]?["b64_json"] is { } b64) {
                return Converter.Base64ToBitmap(b64.ToString());
            }

            if (root?["data"]?[0]?["url"] is { } url) {
                var img = Generic.DownloadImage(url.ToString());
                return img is null ? null : new Bitmap(img);
            }

            return null;
        } catch (Exception ex) {
            Develop.DebugPrint("Bild-Antwort konnte nicht geparsed werden.", ex);
            return null;
        }
    }

    public override void DisposeContent() {
        _apiKey = null;
        _endpoint = null;
        _model = null;
    }

    public override string GetValueFrom(Variable variable) {
        if (variable is not VariableAi v) { return VerschiedeneTypen(variable); }
        if (ReadOnly) { return Schreibgschützt(); }
        ApiKey = v.ApiKey;
        Endpoint = v.Endpoint;
        Model = v.Model;
        return string.Empty;
    }

    protected override void SetValue(object? x) { }

    protected override bool TryParseValue(string txt, out object? result) {
        result = null;
        return false;
    }

    /// <summary>
    /// Sendet einen JSON-POST an den angegebenen, OpenAI-kompatiblen Endpunkt.
    /// Die Basis-URL (z. B. https://api.openai.com/v1) wird um den Pfad ergänzt.
    /// Authentifizierung erfolgt über den Bearer-Token (API-Schlüssel).
    /// </summary>
    private static async Task<string?> SendAsync(string? apiKey, string? endpoint, string path, JsonObject body) {
        var baseUrl = (endpoint ?? string.Empty).TrimEnd('/');
        var url = baseUrl + "/" + path;

        try {
            using var req = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var resp = await HttpClient.SendAsync(req).ConfigureAwait(false);
            var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode) {
                Develop.DebugPrint($"KI-Aufruf fehlgeschlagen ({(int)resp.StatusCode} {resp.ReasonPhrase}): {content}");
                return null;
            }

            return content;
        } catch (Exception ex) {
            Develop.DebugPrint("KI-HTTP-Aufruf fehlgeschlagen.", ex);
            return null;
        }
    }

    #endregion
}