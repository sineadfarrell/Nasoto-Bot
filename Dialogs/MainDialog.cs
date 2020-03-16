// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationRecognizer luisRecognizer, ExtracurricularDialog extracurricularDialog, CampusDialog campusDialog,  ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
    
            AddDialog(campusDialog);
            AddDialog(extracurricularDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                NameStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run
            InitialDialogId = nameof(WaterfallDialog);
        }

    private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What should we talk about? Perhaps we can talk about extracurricular activities or UCD campus?")}, cancellationToken);
            }
       
        public async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new UserProfile(), cancellationToken);
            }


            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.discussCampus:
                    var moduleInfoCampus = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion
                    };
                    return await stepContext.BeginDialogAsync(nameof(CampusDialog), moduleInfoCampus, cancellationToken);
                
                case Luis.Conversation.Intent.discussExtracurricular:

                    var moduleInfoExtra = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion
                    };
                    return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), moduleInfoExtra, cancellationToken);

                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText2 = $"Sorry, it is not in my capacity to talk about that. Let's try again!";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage2, cancellationToken);
                   
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText3 = $"Sorry, it is not in my capacity to talk about that. Let's try again!";
                    var didntUnderstandMessage3 = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage3, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));
                   

            }
           

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(null, cancellationToken);
        }

    }
}
