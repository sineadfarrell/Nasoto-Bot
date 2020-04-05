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



namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationRecognizer luisRecognizer, ExtracurricularDialog extracurricularDialog, CampusDialog campusDialog, ILogger<MainDialog> logger)
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

           var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What should we talk about? Extracurricular Activities or UCD campus?") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            
        }

        public async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new UserProfile(), cancellationToken);
            }


            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);

             string[] stringPos;
            stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
            string[] stringNeg;
            stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };

            switch (luisResult.TopIntent().intent)
            {
           
                case Luis.Conversation.Intent.endConversation:
                 var messageText = $"Do you want to end this conversation?";
                var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput) };
                return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
                    
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
                    var didntUnderstandMessageText2 = $"I didn't understand that. Would you prefer to talk about UCD Campus or extracurricular activities?";
                    var didntUnderstandMessage2 = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage2, cancellationToken);

                    return await stepContext.ReplaceDialogAsync(nameof(MainDialog));

                default:
                    var didntUnderstandMessageText = $"I didn't understand that. Would you prefer to talk about UCD Campus or extracurricular activities?";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = 0;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }
            


        }
       

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           string[] stringPos;
            stringPos = new string[21] { "yes", "ye", "yep", "ya", "yas", "totally", "sure", "ok", "k", "okey", "okay", "alright", "sounds good", "sure thing", "of course", "gladly", "definitely", "indeed", "absolutely","yes please", "please" };
            string[] stringNeg;
            stringNeg = new string[9] { "no", "nope", "no thanks", "unfortunately not", "apologies", "nah", "not now", "no can do", "no thank you" };


             if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

          var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
           
            var moduleDetails = new ModuleDetails(){
                Lecturer = luisResult.Entities.Lecturer,
                Opinion = luisResult.Entities.Opinion,
            };
            
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.None)){
                   var didntUnderstandMessageText3 = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage3 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText3, didntUnderstandMessageText3, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = 2;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);

            }

           if(stringPos.Any(luisResult.Text.ToLower().Contains)){
               ConversationData.PromptedUserForName = true;
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));
                
            }

            if(stringNeg.Any(luisResult.Text.ToLower().Contains)){
            return await stepContext.BeginDialogAsync(nameof(CampusDialog));;    

            }
                var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
                var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = 2;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);


        
        }

    }
}
