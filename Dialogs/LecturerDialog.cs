
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class LecturerDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public LecturerDialog(ConversationRecognizer luisRecognizer,  ILogger<LecturerDialog> logger, MainDialog mainDialog, EndConversationDialog endConversationDialog, ExtracurricularDialog extracurricularDialog )
            : base(nameof(LecturerDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(endConversationDialog);
            AddDialog(extracurricularDialog);
            AddDialog(mainDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GetInfoAsync,
                infoAsync,
                GetAnswerAsync,  
            }));
            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

         private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             ConversationData.PromptedUserForName = false;
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            
            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = $"On the topic of lectures, generally what is your opinion on your lecturers?";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }


        private async Task<DialogTurnResult> GetInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             ConversationData.PromptedUserForName = false;
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

            switch (luisResult.TopIntent().intent){

             case Luis.Conversation.Intent.None:
                    var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            
             case Luis.Conversation.Intent.endConversation:
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    
           
            default:
            var messageText = $"Ok. Presumably they are not all like this?";
            // var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            var messageFac = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), messageFac, cancellationToken);
            }
        }

         private async Task<DialogTurnResult> infoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
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

             switch (luisResult.TopIntent().intent){
             case Luis.Conversation.Intent.None:
                   var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            
             case Luis.Conversation.Intent.endConversation:
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    
           
            default:
            var message = $"Ok. Would you like to talk about another aspect of university?.";
            var messageFac = new PromptOptions { Prompt = MessageFactory.Text(message, message, InputHints.ExpectingInput)};
            
            return await stepContext.PromptAsync(nameof(TextPrompt), messageFac, cancellationToken);
             }
        }


         private async Task<DialogTurnResult> GetAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage3, cancellationToken);

            }

           if(stringPos.Any(luisResult.Text.ToLower().Contains)){
                return await stepContext.BeginDialogAsync(nameof(MainDialog));
                
            }

            if(stringNeg.Any(luisResult.Text.ToLower().Contains)){
            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    

            }
                var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
                var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);


        }

    }
}