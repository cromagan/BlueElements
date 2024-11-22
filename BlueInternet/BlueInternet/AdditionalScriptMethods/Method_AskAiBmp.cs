// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Policy;
using Anthropic.SDK;
using static BlueScript.Variables.VariableAi;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using CefSharp.DevTools.DOM;
using static BlueScript.Variables.VariableBitmap;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_AskAiBmp : Method {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal, BmpVal];
    public override string Command => "askaibmp";
    public override List<string> Constants => [];
    public override string Description => "Gibt einen Text und ein Bild an die KI weiter";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "AskAiBmp(Ai, text, image)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableAi mai) { return DoItFeedback.InternerFehler(ld); }
        if (mai.ValueClient is not { } client) { return DoItFeedback.InternerFehler(ld); }
        if (attvar.Attributes[2] is not VariableBitmap vbmp || vbmp.ValueBitmap is not { } bmp) { return DoItFeedback.InternerFehler(ld); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        try {
            // Convert the byte array to a base64 string
            string base64String = BlueBasics.Converter.BitmapToBase64(bmp, ImageFormat.Jpeg);// Convert.ToBase64String(imageBytes);

            var messages = new List<Message>();
            messages.Add(new Message() {
                Role = RoleType.User,
                Content = new List<ContentBase>()
                {
                    new ImageContent()
                    {
                        Source = new ImageSource()
                        {
                            MediaType = "image/jpeg",
                            Data = base64String
                        }
                    },
                    new TextContent()
                    {
                        Text = attvar.ValueStringGet(1)
                    }
                }
            });
            var parameters = new MessageParameters() {
                Messages = messages,
                MaxTokens = 512,
                Model = AnthropicModels.Claude35Sonnet,
                Stream = true,
                Temperature = 1.0m,
            };

            //var outputs = new List<MessageResponse>();
            //await foreach (var res in client.Messages.StreamClaudeMessageAsync(parameters)) {
            //    if (res.Delta != null) {
            //        Console.Write(res.Delta.Text);
            //    }

            //    outputs.Add(res);
            //}
            //Console.WriteLine(string.Empty);
            //Console.WriteLine($@"Used Tokens - Input:{outputs.First().StreamStartMessage.Usage.InputTokens}.
            //                Output: {outputs.Last().Usage.OutputTokens}");

            var firstResult = client.Messages.GetClaudeMessageAsync(parameters).GetAwaiter().GetResult();

            return new DoItFeedback(firstResult.Message.ToString());
        } catch {
            return new DoItFeedback(ld, "Allgemeiner Fehler bei der Übergabe an die KI.");
        }
    }

    #endregion
}