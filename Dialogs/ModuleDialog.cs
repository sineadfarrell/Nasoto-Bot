// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class ModuleDialog : ComponentDialog
    {
        private const string NumberModulesMsgText = "How many modules are you doing?";
        
        public ModuleDialog()
            : base(nameof(ModuleDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NumberModulesStepAsync,
                LecturerStepAsync,
                ExamStepAsync,
                CAStepAsync,
                OpinionStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NumberModulesStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var moduleDetails = (ModuleDetails)stepContext.Options;

            if (moduleDetails.ModuleName == null)
            {
                var promptMessage = MessageFactory.Text(NumberModulesMsgText, NumberModulesMsgText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(moduleDetails.ModuleName, cancellationToken);
        }

        
        private async Task<DialogTurnResult> LecturerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var moduleDetails = (ModuleDetails)stepContext.Options;

            moduleDetails.Lecturer = (string)stepContext.Result;

            var messageText = $"Who is the lecturer for the {moduleDetails.ModuleName} module?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            // return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(moduleDetails.Lecturer, cancellationToken);

        }

        private async Task<DialogTurnResult> ExamStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var moduleDetails = (ModuleDetails)stepContext.Options;

            moduleDetails.Exam = (string)stepContext.Result;

            var messageText = $"Do you have a final exam in {moduleDetails.ModuleName}?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(moduleDetails.Exam, cancellationToken);
        }


    private async Task<DialogTurnResult> CAStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var moduleDetails = (ModuleDetails)stepContext.Options;

            moduleDetails.ContinousAssesment = (string)stepContext.Result;

            var messageText = $"Is there a continous assesment component for the {moduleDetails.ModuleName} module?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(moduleDetails.ContinousAssesment, cancellationToken);
        }

         private async Task<DialogTurnResult> OpinionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var moduleDetails = (ModuleDetails)stepContext.Options;

            moduleDetails.Opinion = (string)stepContext.Result;

            var messageText = $"Do you like {moduleDetails.ModuleName}?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(moduleDetails.Opinion, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var moduleDetails = (ModuleDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(moduleDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
