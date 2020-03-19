
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

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

                    stepContext.ActiveDialog.State[key: "stepIndex"] = 0;
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

                    stepContext.ActiveDialog.State[key: "stepIndex"] = 1;
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
                   var didntUnderstandMessageText = $"I didn't understand that. Could you please rephrase";
                    var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput) };

                    stepContext.ActiveDialog.State[key: "stepIndex"] = 2;
                    return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);

            }

            if(luisResult.Text.Equals("yes")){
                return await stepContext.BeginDialogAsync(nameof(MainDialog));
            }
           
            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    


        }

    }
}