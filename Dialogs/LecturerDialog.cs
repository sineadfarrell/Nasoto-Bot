
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
        
        
        public LecturerDialog(ConversationRecognizer luisRecognizer,  ILogger<LecturerDialog> logger)
            : base(nameof(LecturerDialog))

        {   
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                GetInfoAsync,
                
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
            var messageText = $"What do you think about your lectures?";
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
            if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
           }
            var moduleDetails = new ModuleDetails(){
                Lecturer = luisResult.Entities.Lecturer,
                Opinion = luisResult.Entities.Opinion,
            };
            
            var messageText = $"I wouldn't be intrested in that topic myself, do you have a final exam?";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.NextAsync(null, cancellationToken);
        }
    }
}