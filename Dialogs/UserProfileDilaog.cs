using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs

{
    // Dialog to collect initial information from the user 
    public class UserProfileDialog : ComponentDialog
    {
       
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

          // Initialise the dialog

       public UserProfileDialog(ConversationRecognizer luisRecognizer,  ILogger<UserProfileDialog> logger, EndConversationDialog endConversationDialog, ModuleDialog moduleDialog)
            : base(nameof(UserProfileDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(endConversationDialog);
            AddDialog(moduleDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
            IntroStepAsync,
            GetNameAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }


    // Begin Dialog 
       private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation))
            {
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog)); ;
            }


            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What is your name?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
       
    
         private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            var userInfo = new UserProfile()
                    {
                        Name = luisResult.Entities.UserName,
                    };
            if (luisResult.TopIntent().Equals(Luis.Conversation.Intent.None))
            {
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                  await stepContext.Context.SendActivityAsync(
                     MessageFactory.Text("Do you want to end this conversation?"));
                    //  EndConversation
                     return await stepContext.ReplaceDialogAsync(nameof(EndStepAsync));
            }
                var didntUnderstandMessageText2 = $"I didn't understand that. Could you please rephrase";
                 var elsePromptMessage2 =  new PromptOptions {Prompt = MessageFactory.Text(didntUnderstandMessageText2, didntUnderstandMessageText2, InputHints.ExpectingInput)};
                 
                 stepContext.ActiveDialog.State[key: "stepIndex"] =  (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                 return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
            }
           
            if (userInfo.Name == null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ok we will now begin."), cancellationToken);
                // begin Module Dialog 
                return await stepContext.BeginDialogAsync(nameof(ModuleDialog));    
                 }

                 await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ok {StringExtensions.FirstCharToUpper(userInfo.Name.FirstOrDefault())} we will now begin."), cancellationToken);
                // begin ModuleDialog 
                return await stepContext.BeginDialogAsync(nameof(ModuleDialog));   
            }

            // EndConversation 
             private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            if (stringPos.Any(luisResult.Text.ToLower().Contains))
            {
                ConversationData.PromptedUserForName = true;
                await stepContext.Context.SendActivityAsync(
                     MessageFactory.Text("Goodbye."));
                    //  End dialog 
                    return await stepContext.EndDialogAsync();
                 
            }
            if (stringNeg.Any(luisResult.Text.ToLower().Contains))
            {
                var messageText = $"Ok the conversation will continue.";
                var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                // Begin MainDialog - to chose what to talk about 
                return await stepContext.BeginDialogAsync(nameof(MainDialog));
            }
            var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
            var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

            stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

        }



           
        
    }

       
}