﻿// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using BlueBasics;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using static BlueScript.Variables.VariableAi;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_AskAi : Method {

    #region Properties

    public override List<List<string>> Args => [AiVal, StringVal];
    public override string Command => "askai";
    public override List<string> Constants => [];
    public override string Description => "Gibt einen Text an die KI weiter";
    public override bool GetCodeBlockAfter => false;
    public override int LastArgMinCount => -1;
    public override MethodType MethodType => MethodType.Standard;
    public override bool MustUseReturnValue => true;
    public override string Returns => VariableString.ShortName_Plain;
    public override string StartSequence => "(";
    public override string Syntax => "AskAi(Ai, text)";

    #endregion

    #region Methods

    public override DoItFeedback DoIt(VariableCollection varCol, SplittedAttributesFeedback attvar, ScriptProperties scp, LogData ld) {
        if (attvar.Attributes[0] is not VariableAi mai) { return DoItFeedback.InternerFehler(ld); }
        if (mai.ValueClient is not { } client) { return DoItFeedback.InternerFehler(ld); }

        if (!scp.ProduktivPhase) { return DoItFeedback.TestModusInaktiv(ld); }

        var tries = 0;

        do {
            try {
                var messages = new List<Message> { new Message(RoleType.User, attvar.ValueStringGet(1)) };

                var parameters = new MessageParameters {
                    Messages = messages,
                    MaxTokens = 1024,
                    Model = AnthropicModels.Claude35Sonnet,
                    Stream = false,
                    Temperature = 1.0m,
                };
                var firstResult = client.Messages.GetClaudeMessageAsync(parameters).GetAwaiter().GetResult();

                return new DoItFeedback(firstResult.Message.ToString());
            } catch {
                tries++;
                Generic.Pause(10, false);
            }
        } while (tries < 10);
        return new DoItFeedback("Allgemeiner Fehler bei der Übergabe an die KI.", false, ld);
    }

    #endregion
}