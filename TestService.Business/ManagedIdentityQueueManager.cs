using Azure.Messaging.ServiceBus;
using TestService.Business.Abstraction;

namespace TestService.Business
{
    public class ManagedIdentityQueueManager : IManagedIdentityQueueManager
    {
        #region Private Properties

        // The Service Bus client
        private readonly ServiceBusSender _serviceBusSender;

        #endregion

        #region Public Constructor

        public ManagedIdentityQueueManager(ServiceBusSender serviceBusSender)
        {
            _serviceBusSender = serviceBusSender;
        }

        #endregion

        #region Public Methods

        public async Task SendMessageToQueue()
        {
            var messageBody = "{\"Id\":\"3ec743ef-ef7a-4d58-ba78-0b12da5a562b\",\"Version\":\"2.0.0\",\"Year\":2023, \"UserId\":33410\"}";
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                MessageId = "3ec743ef-ef7a-4d58-ba78-0b12da5a562b_2.0.0",
                SessionId = Guid.NewGuid().ToString(),
            };

            // Send the message to the queue
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
        #endregion
    }
}
