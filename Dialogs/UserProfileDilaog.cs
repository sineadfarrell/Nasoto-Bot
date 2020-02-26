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

       public UserProfileDialog(ConversationRecognizer luisRecognizer,  ModuleDialog moduleDialog,  ILogger<UserProfileDialog> logger, EndConversationDialog endConversationDialog)
            : base(nameof(UserProfileDialog))
        {
            // _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(moduleDialog);
            AddDialog(endConversationDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {

            NameStepAsync,
            NumberOfModulesAsync,

            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Hello! Could you please tell me your name.") }, cancellationToken);
        }
         private async Task<DialogTurnResult> NumberOfModulesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)

        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.Conversation>(stepContext.Context, cancellationToken);
            var userInfo = new UserProfile()
                    {
                        Name = luisResult.Entities.UserName,

                    };
                
                if(luisResult.TopIntent().Equals(Luis.Conversation.Intent.endConversation)){
                return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), cancellationToken);;    
           }
                
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Let's begin our convesation on your university experience."), cancellationToken);

                return await stepContext.BeginDialogAsync(nameof(ModuleDialog), new ModuleDetails(), cancellationToken);
            }

           
        
    }

       
}