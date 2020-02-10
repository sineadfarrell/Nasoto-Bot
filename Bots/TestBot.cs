// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class TestBot : ActivityHandler
    {
        private BotState _conversationState;
        private BotState _userState;

        public TestBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
            
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Hi! I'm Makoto, I want to talk to you about your University experince today. \n To begin our conversation type anything");
        }
       
         

        // protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        // {
        //     // Get the state properties from the turn context.

        //     var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
        //     var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

        //     var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
        //     var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

        //     if (string.IsNullOrEmpty(userProfile.Name))
        //     {
        //         // First time around this is set to false, so we will prompt user for name.
        //         if (conversationData.PromptedUserForName)
        //         {
        //             // Set the name to what the user provided.
        //             userProfile.Name = turnContext.Activity.Text?.Trim();

        //             // Acknowledge that we got their name.
        //             await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. ");

        //             // Reset the flag to allow the bot to go through the cycle again.
        //             conversationData.PromptedUserForName = false;
        //         }
        //         else
        //         {
        //             // Prompt the user for their name.
        //             await turnContext.SendActivityAsync($"What is your name?");

        //             // Set the flag to true, so we don't prompt in the next turn.
        //             conversationData.PromptedUserForName = true;
        //         }
        //     }
        // }
// protected  async Task OnMessageGetModuleNumberAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
//         {
//              var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
//             var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

//             var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
//             var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

//             if (userProfile.NumberOfModules == null)
//             {
//                 if (conversationData.PromptedForNumberOfModules)
//                 {
//                     //set number of modules user is taking
//                     string temp = turnContext.Activity.Text?.Trim();
//                     int number;
//                     bool isParsable = Int32.TryParse(temp, out number);
//                     if (isParsable){
//                         userProfile.NumberOfModules =  number;
//                     }
//                     else{
//                         await turnContext.SendActivityAsync($"I don't understand");
//                     }
//                 }
//                 else{
    
//                 await turnContext.SendActivityAsync($"How many modules are you taking this semester?");
//                 conversationData.PromptedForNumberOfModules = true;
//                 }
//             }


        
    // }
    }
}
