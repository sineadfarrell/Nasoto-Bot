// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Extensions.Logging;
using System.Linq;


namespace Microsoft.BotBuilderSamples.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        private static readonly AzureBlobStorage _myStorage = new AzureBlobStorage("DefaultEndpointsProtocol=https;AccountName=userstudynasoto;AccountKey=ChWa3d2eq0VpdLhGEIj62TVDR7iVnZmSVj27IQ1zqichGed950SboHe2VMPtue0ZkMZ+mwetcfguJioNTuD+hA==;EndpointSuffix=core.windows.net", "userstudynasoto");
        private readonly AzureBlobTranscriptStore _myTranscripts = new AzureBlobTranscriptStore("DefaultEndpointsProtocol=https;AccountName=userstudynasoto;AccountKey=ChWa3d2eq0VpdLhGEIj62TVDR7iVnZmSVj27IQ1zqichGed950SboHe2VMPtue0ZkMZ+mwetcfguJioNTuD+hA==;EndpointSuffix=core.windows.net", "userstudynasoto");

        // Create cancellation token (used by Async Write operation).
        public CancellationToken cancellationToken { get; private set; }
        protected readonly Dialog Dialog;
        public readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public class UtteranceLog : IStoreItem
        {
            // A list of things that users have said to the bot
            public List<string> UtteranceList { get; } = new List<string>();

            // The number of conversational turns that have occurred        
            public int TurnNumber { get; set; } = 0;

            // Create concurrency control where this is used.
            public string ETag { get; set; } = "*";
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            ConversationData.PromptedUserForName = false;
            await turnContext.SendActivityAsync("My name is Nasoto. We are going to talk about university today.");

        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {


            if (ConversationData.PromptedUserForName)
            {
                ConversationData.PromptedUserForName = false;
                await turnContext.SendActivityAsync($"Bye");

            }
            else
            {

                // preserve user input.
                var utterance = turnContext.Activity.Text;
                // make empty local logitems list.
                UtteranceLog logItems = null;

                // see if there are previous messages saved in storage.
                try
                {
                    string[] utteranceList = { "UtteranceLog" };
                    logItems = _myStorage.ReadAsync<UtteranceLog>(utteranceList).Result?.FirstOrDefault().Value;
                }
                catch
                {
                    // Inform the user an error occured.
                    await turnContext.SendActivityAsync("Sorry, something went wrong reading your stored messages!");
                }
                // If no stored messages were found, create and store a new entry.
                if (logItems is null)
                {
                    // add the current utterance to a new object.
                    logItems = new UtteranceLog();
                    logItems.UtteranceList.Add(utterance);
                    // set initial turn counter to 1.
                    logItems.TurnNumber++;

                    // Show user new user message.
                    //  await turnContext.SendActivityAsync($"{logItems.TurnNumber}: The list is now: {string.Join(", ", logItems.UtteranceList)}");

                    // Create Dictionary object to hold received user messages.
                    var changes = new Dictionary<string, object>();
                    {
                        changes.Add("UtteranceLog", logItems);
                    }
                    try
                    {
                        // Save the user message to your Storage.
                        await _myStorage.WriteAsync(changes, cancellationToken);
                    }
                    catch
                    {
                        // Inform the user an error occured.
                        await turnContext.SendActivityAsync("Sorry, something went wrong storing your message!");
                    }
                }
                // Else, our Storage already contained saved user messages, add new one to the list.
                else
                {
                    // add new message to list of messages to display.
                    logItems.UtteranceList.Add(utterance);
                    // increment turn counter.
                    logItems.TurnNumber++;

                    // show user new list of saved messages.
                    //  await turnContext.SendActivityAsync($"{logItems.TurnNumber}: The list is now: {string.Join(", ", logItems.UtteranceList)}");

                    // Create Dictionary object to hold new list of messages.
                    var changes = new Dictionary<string, object>();
                    {
                        changes.Add("UtteranceLog", logItems);
                    };

                    try
                    {
                        // Save new list to your Storage.
                        await _myStorage.WriteAsync(changes, cancellationToken);
                    }
                    catch
                    {
                        // Inform the user an error occured.
                        await turnContext.SendActivityAsync("Sorry, something went wrong storing your message!");
                    }
                }
                await _myTranscripts.LogActivityAsync(turnContext.Activity);

                List<string> storedTranscripts = new List<string>();
                PagedResult<Microsoft.Bot.Builder.TranscriptInfo> pagedResult = null;
                var pageSize = 0;
                do
                {
                    pagedResult = await _myTranscripts.ListTranscriptsAsync("emulator", pagedResult?.ContinuationToken);
                    pageSize = pagedResult.Items.Count();

                    // transcript item contains ChannelId, Created, Id.
                    // save the channelIds found by "ListTranscriptsAsync" to a local list.
                    foreach (var item in pagedResult.Items)
                    {
                        storedTranscripts.Add(item.Id);
                    }
                } while (pagedResult.ContinuationToken != null);


                Logger.LogInformation("Running dialog with Message Activity.");

                // Run the Dialog with the new message Activity.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            }
        }
    }
}
