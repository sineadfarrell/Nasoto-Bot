using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class TopLevelDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        public TopLevelDialog(ConversationRecognizer luisRecognizer, TopLevelDialog topLevelDialog, ILogger<TopLevelDialog> logger)
           : base(nameof(TopLevelDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            var waterfallSteps = new WaterfallStep[]
             {
               NameStepAsync,
               NumberOfModulesAsync,
               FinalStepAsync

             };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Create an object in which to collect the user's information within the dialog

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What's your name?") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> NumberOfModulesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.greeting:
                    var userProfile = new UserProfile()
                    {
                        Name = luisResult.Entities.UserName,
                    };

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("How many modules are you taking?") }, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
         private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var UserDetails = (UserProfile)stepContext.Options;

                return await stepContext.EndDialogAsync(UserDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}