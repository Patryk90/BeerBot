using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeerBot.Models;
using BeerBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BeerBot
{
    public class BeerBot : ActivityHandler
    {
        private readonly BotState _conversationState;
        private readonly BotState _userState;

        public BeerBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Welcome to Beer Bot POC. Type anything to get started.");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow());

            var userStateAccessors = _userState.CreateProperty<OrderInfo>(nameof(OrderInfo));
            var profile = await userStateAccessors.GetAsync(turnContext, () => new OrderInfo());

            await FillAnOrder(flow, profile, turnContext);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private async Task FillAnOrder(ConversationFlow flow, OrderInfo profile, ITurnContext<IMessageActivity> turnContext)
        {
            string input = turnContext.Activity.Text?.Trim();
            string message;

            switch (flow.LastQuestionAsked)
            {
                case ConversationFlow.Question.None:
                    await turnContext.SendActivityAsync("Let's get started. What is your name?");
                    flow.LastQuestionAsked = ConversationFlow.Question.Name;
                    break;
                case ConversationFlow.Question.Name:
                    if (OrderValidation.ValidateName(input, out string name, out message))
                    {
                        profile.Name = name;
                        await turnContext.SendActivityAsync($"Hi {profile.Name}.");
                        await turnContext.SendActivityAsync("How old are you?");
                        flow.LastQuestionAsked = ConversationFlow.Question.Age;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
                case ConversationFlow.Question.Age:
                    if (OrderValidation.ValidateAge(input, out int age, out message))
                    {
                        profile.Age = age;
                        await turnContext.SendActivityAsync($"I have your age as {profile.Age}.");
                        await turnContext.SendActivityAsync("When do you like to drink?");
                        flow.LastQuestionAsked = ConversationFlow.Question.Date;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }

                case ConversationFlow.Question.Date:
                    if (OrderValidation.ValidateDate(input, out string date, out message))
                    {
                        profile.Date = date;
                        await turnContext.SendActivityAsync($"Your drinking session is scheduled for {profile.Date}.");
                        await turnContext.SendActivityAsync($"Thanks for completing the booking {profile.Name}.");
                        await turnContext.SendActivityAsync($"Type anything to run the bot again.");
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        profile = new OrderInfo();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.");
                        break;
                    }
            }
        }
    }
}
