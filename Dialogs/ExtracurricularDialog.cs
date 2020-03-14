
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System; 
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class ExtracurricularDialog : ComponentDialog
    {
         private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        
        
        public ExtracurricularDialog(ConversationRecognizer luisRecognizer,  ILogger<ExtracurricularDialog> logger, EndConversationDialog endConversationDialog)
            : base(nameof(ExtracurricularDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(endConversationDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GetInfoAsync,
                MoveConvoAsync,
                
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
            var messageText = $"I enjoy going to the gym on campus, do you do much in your spare time?";
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
            
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), moduleDetails, cancellationToken);;    
           }
           if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.discussExtracurricular)){
            var messageText = $"The facilities are just great here";
            var elsePromptMessage = new PromptOptions { Prompt = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput)};
            await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage, cancellationToken);
        }
           
            var messageText2 = $"Do you want to continue talking to me?";
            var elsePromptMessage2 = new PromptOptions { Prompt = MessageFactory.Text(messageText2, messageText2, InputHints.ExpectingInput)};
            return await stepContext.PromptAsync(nameof(TextPrompt), elsePromptMessage2, cancellationToken);
        }
    
    private async Task<DialogTurnResult> MoveConvoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
           
        if(luisResult.Text.Equals("no")){
            return await stepContext.BeginDialogAsync(nameof(EndOfConversationCodes), cancellationToken);
        }
          return await stepContext.EndDialogAsync();

    }
    }
}