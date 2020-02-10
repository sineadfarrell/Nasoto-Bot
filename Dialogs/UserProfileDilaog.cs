using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs

{
     public class UserProfileDialog : ComponentDialog
    {
        private IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public UserProfileDialog(UserState userState)
            : base(nameof(UserProfileDialog))
    {
           _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");


        // This array defines how the Waterfall will execute.
        var waterfallSteps = new WaterfallStep[]
        {

        NameStepAsync,
        
        };

        // Add named dialogs to the DialogSet. These names are saved in the dialog state.

        AddDialog(new TextPrompt(nameof(TextPrompt)));
       

        // The initial child Dialog to run.
        InitialDialogId = nameof(WaterfallDialog);
        }
    private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
     {
         stepContext.Values["stage"] = ((FoundChoice)stepContext.Result).Value;

    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
    }

    }
}