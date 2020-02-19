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
        public MainDialog(ConversationRecognizer luisRecognizer,  ExtracurricularDialog extracurricularDialog, UserProfileDialog userProfileDialog, ModuleDialog moduleDialog, EndConversationDialog endConversation, CampusDialog campusDialog, LecturerDialog lecturerDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(userProfileDialog);
            AddDialog(moduleDialog);
            AddDialog(endConversation);
            AddDialog(lecturerDialog);
            AddDialog(campusDialog);
            AddDialog(extracurricularDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What aspect of university would you like to talk about?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        public async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext,  CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new UserProfile(), cancellationToken);
            }


            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case Luis.Conversation.Intent.greeting:

                    // Initialize UsesrEntities with any entities we may have found in the response.
                    var userInfo = new UserProfile()
                    {
                        Name = luisResult.Entities.UserName,
                    };

                    if (userInfo.Name == null)
                    {
                        return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), userInfo, cancellationToken);
                    }
                    return await stepContext.BeginDialogAsync(nameof(ModuleDialog), userInfo, cancellationToken);

                case Luis.Conversation.Intent.discussModule:

                    var moduleInfo = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion,

                    };

                    return await stepContext.BeginDialogAsync(nameof(ModuleDialog), moduleInfo, cancellationToken);


                case Luis.Conversation.Intent.discussLecturer:
                     var moduleInfoLec = new ModuleDetails()
                    {
                        ModuleName = luisResult.Entities.Module,
                        Opinion = luisResult.Entities.Opinion,
                        Lecturer = luisResult.Entities.Lecturer,
                        Emotion = luisResult.Entities.Emotion,

                    };

                    return await stepContext.BeginDialogAsync(nameof(LecturerDialog), moduleInfoLec, cancellationToken);


                // case Luis.Conversation.Intent.discussFeeling:
                //     // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                //     var getFeelingMessageText = "TODO: get Feeling flow here";
                //     var getFeelingMessage = MessageFactory.Text(getFeelingMessageText, getFeelingMessageText, InputHints.IgnoringInput);
                //     await stepContext.Context.SendActivityAsync(getFeelingMessage, cancellationToken);
                //     break;

                case Luis.Conversation.Intent.discussExtracurricular:
                    return await stepContext.BeginDialogAsync(nameof(ExtracurricularDialog), cancellationToken);

                case Luis.Conversation.Intent.discussCampus:
                    return await stepContext.BeginDialogAsync(nameof(CampusDialog), cancellationToken);

                case Luis.Conversation.Intent.endConversation:

                    return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);

                case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                  return await stepContext.NextAsync(null, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText2 = $"Sorry, I didn't get that. Please try rephrasing your message!";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage2, cancellationToken);
                    break;

            }
            return await stepContext.NextAsync(null, cancellationToken);

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(null, cancellationToken);
        }

    }
}
