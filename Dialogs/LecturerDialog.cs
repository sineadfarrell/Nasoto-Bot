
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
            var messageText = $"What's your opinion on the lecturers?";
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
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.None)){
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput)};
                    await stepContext.PromptAsync(nameof(TextPrompt), didntUnderstandMessage, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(LecturerDialog));

            }
             if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    
           }
            
            var messageText = $"That's interesting to know!";
            var elsePromptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
             await stepContext.Context.SendActivityAsync(elsePromptMessage, cancellationToken);
            var message = $"Would you like to talk about another aspect of university?.";
            var messageFac = new PromptOptions { Prompt = MessageFactory.Text(message, message, InputHints.ExpectingInput)};
            
            return await stepContext.PromptAsync(nameof(TextPrompt), messageFac, cancellationToken);
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
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = new PromptOptions { Prompt = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.ExpectingInput)};
                    await stepContext.PromptAsync(nameof(TextPrompt), didntUnderstandMessage, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(LecturerDialog));

            }

            if(luisResult.Text.Equals("yes")){
                return await stepContext.BeginDialogAsync(nameof(MainDialog));
            }
           
            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog));;    


        }

    }
}