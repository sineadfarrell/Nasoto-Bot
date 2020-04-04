// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    // Defines a state property used to track conversation data.
    public static class ConversationData
    {
        
        // Track whether we have already asked the user's name
        public static bool PromptedUserForName { get; set; } = false;

        public static bool PromptedForNumberOfModules { get; set; } = false;
    }
}