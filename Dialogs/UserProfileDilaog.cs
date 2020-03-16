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
    public class UserProfileDialog : ComponentDialog
    {
       
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

       public UserProfileDialog(ConversationRecognizer luisRecognizer,  ILogger<UserProfileDialog> logger, EndConversationDialog endConversationDialog, ModuleDialog moduleDialog)
            : base(nameof(UserProfileDialog))
        {
            // _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
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
       private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
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
            

            if(string.IsNullOrEmpty(userInfo.Name.FirstOrDefault())){
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ok we will now begin."), cancellationToken);

                return await stepContext.BeginDialogAsync(nameof(ModuleDialog));    
                 }

            if((userInfo.Name.FirstOrDefault()).Equals("")){
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                     await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = didntUnderstandMessage }, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(nameof(UserProfileDialog));
            }
            if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
                }
                
                 await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ok {userInfo.Name.FirstOrDefault()} we will now begin."), cancellationToken);

                return await stepContext.BeginDialogAsync(nameof(ModuleDialog));   
            }



           
        
    }

       
}