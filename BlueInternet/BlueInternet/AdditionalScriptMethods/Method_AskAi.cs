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
using BlueDatabase.AdditionalScriptMethods;
using BlueScript.Enums;
using BlueScript.Structures;
using BlueScript.Variables;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mistral.SDK;
using Mistral.SDK.DTOs;
using static BlueScript.Variables.VariableMistralAi;
using System.Linq;

//using CefSharp.WinForms;

namespace BlueScript.Methods;

// ReSharper disable once UnusedMember.Global
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
internal class Method_AskAi : Method {

    #region Properties

    public override List<List<string>> Args => [MistralAiVal, StringVal];
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
        if (attvar.Attributes[0] is not VariableMistralAi mai) { return DoItFeedback.InternerFehler(ld); }
        if (mai.ValueClient is not { } maic) { return DoItFeedback.InternerFehler(ld); }

        try {
            var request = new ChatCompletionRequest(

                ModelDefinitions.OpenMistral7b,

                new List<ChatMessage>()
                {
            //new ChatMessage(ChatMessage.RoleEnum.System,   "You are an expert at writing sonnets."),
            new ChatMessage(ChatMessage.RoleEnum.User, attvar.ValueStringGet(1))
                },
                stream: false,
                maxTokens: 500

                );

            var response = maic.Completions.GetCompletionAsync(request).GetAwaiter().GetResult();

            //var response = await maic.Completions.GetCompletionAsync(request);

            return new DoItFeedback(response.Choices.First().Message.Content);

            //Console.WriteLine(response.Choices.First().Message.Content);
        } catch {
            return new DoItFeedback(ld, "Allgemeiner Fehler bei der Übergabe an die KI.");
        }
    }

    #endregion
}