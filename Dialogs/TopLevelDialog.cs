using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class TopLevelDialog : ComponentDialog
    {
        public TopLevelDialog()
           : base(nameof(TopLevelDialog))
        {
            var waterfallSteps = new WaterfallStep[]
             {
               NameStepAsync,
               NumberOfModulesAsync,
            

             };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog.
            
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What's your name?") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> NumberOfModulesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {
            stepContext.Values["name"] = (string)stepContext.Result;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

           return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("How many modules are you taking?") }, cancellationToken);

        }
    }
}