
using Azure.Messaging.ServiceBus;
using Groove.SP.Application.AppDocument.Services.Interfaces;
using Groove.SP.Application.AppDocument.Validators;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Invoices.Services.Interfaces;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Core.Models;
using Hangfire;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Groove.SP.Application.AppDocument.Services
{
    /// <summary>
    /// A processor to CSED Shipping Document via Azure Service Bus 
    /// </summary>
    public class CSEDShippingDocumentProcessor : ICSEDShippingDocumentProcessor
    {
        private readonly CSEDShippingDocumentServiceBus _csedShippingDocumentServiceBusConfig;
        private readonly IInvoiceService _invoiceService;
        private readonly IAttachmentService _attachmentService;
        private readonly IBlobStorage _blobStorage;


        public CSEDShippingDocumentProcessor(
            IOptions<CSEDShippingDocumentServiceBus> csedShippingDocumentServiceBusConfig, 
            IBlobStorage blobStorage,
            IInvoiceService invoiceService,
            IAttachmentService attachmentService
            )
        {
            this._csedShippingDocumentServiceBusConfig = csedShippingDocumentServiceBusConfig.Value;
            this._blobStorage = blobStorage;
            this._invoiceService = invoiceService;
            this._attachmentService = attachmentService;
        }

        /// <summary>
        /// To start to subscribe new message on Azure Service Bus as the system starts
        /// </summary>
        /// <returns></returns>
        public async Task StartProccessorAsync()
        {
            // As the system is starting, the processor will be registered
            // Skip if connection string to ASB is unavailable
            var connectionString = $@"{_csedShippingDocumentServiceBusConfig.ConnectionString}";
            if(string.IsNullOrEmpty(connectionString))
            {
                return;
            }
            // queue name or topic name
            var queueName = $@"{_csedShippingDocumentServiceBusConfig.Topic}";

            // subscription name
            var subscriptionName = $@"{_csedShippingDocumentServiceBusConfig.Subscription}";

            var client = new ServiceBusClient(connectionString, new ServiceBusClientOptions());

            // create a processor that we can use to process the messages
            //client.CreateProcessor(queueName, subName, new ServiceBusProcessorOptions());
            var processor = client.CreateProcessor(queueName, subscriptionName, new ServiceBusProcessorOptions());

            // add handler to process messages
            processor.ProcessMessageAsync += NewDocumentArrivalMessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += NewDocumentArrivalErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();
        }

        /// <summary>
        /// To define the handler to process new message
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task NewDocumentArrivalMessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                var message = args.Message.Body.ToObjectFromJson<CSEDShippingDocumentViewModel>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // validate message for correct data
                var messageValidator = new CSEDShippingDocumentViewModelValidator();
                var validationResult = messageValidator.Validate(message);
                if (!validationResult.IsValid)
                {
                    var newException = new Exception($"Bad data found in message content: {string.Join(" ", validationResult.Errors?.Select(x => x.ErrorMessage))}");
                    BackgroundJob.Enqueue<CSEDShippingDocumentProcessor>(x => x.LogErrorOnProceedingEdiSonShippingDocument(DateTime.UtcNow, args.Message.Body.ToString(), newException));
                    return;
                }

                // transfer to Hangfire to handle the message
                // using the Hangfire dashboard to manage CSED shipping document processing
                var documentType = message.documentType;
                var documentId = message.documentId;
                var documentCode = message.documentCode;

                BackgroundJob.Enqueue<CSEDShippingDocumentProcessor>(x => x.HandleEdiSonShippingDocumentAsync(documentType, documentId, documentCode, message));

                // complete the message. messages is deleted from the queue. 
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                // Any exception will be logged via Hangfire job then view it on Hangfire dashboard
                BackgroundJob.Enqueue<CSEDShippingDocumentProcessor>(x => x.LogErrorOnProceedingEdiSonShippingDocument(DateTime.UtcNow, args.Message.Body.ToString(), ex));
            }
        }

        /// <summary>
        /// To handle any errors when receiving messages, it is fired by Azure Service Bus.
        /// Will be logged via Hangfire job then view it on Hangfire dashboard
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public Task NewDocumentArrivalErrorHandler(ProcessErrorEventArgs args)
        {
            ServiceBusException serviceBusException = args.Exception as ServiceBusException;

            // Ignore if configuration is not correct/can not integrate -> No log
            if (serviceBusException.Reason != ServiceBusFailureReason.MessagingEntityNotFound)
            {
                BackgroundJob.Enqueue<CSEDShippingDocumentProcessor>(x => x.LogErrorOnHandlingEdiSonShippingDocument(DateTime.UtcNow, args));
            }

            return Task.CompletedTask;

        }


        /// <summary>
        /// Hangfire job to proceed message.
        /// Try 100 times every 6 hours.
        /// </summary>
        /// <param name="documentType"></param>
        /// <param name="documentId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [JobDisplayName("Handle CSED New Shipping Document - documentType: {0}, documentCode: {2}")]
        [AutomaticRetry(Attempts = 100, DelaysInSeconds = new[] { 21600 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task HandleEdiSonShippingDocumentAsync(string documentType, Guid documentId, string documentCode, CSEDShippingDocumentViewModel message)
        {
            var fileName = message.documentName;
            var filePath = message.documentPath;
            var fileType = message.documentType;

            // If it is invoice
            if (documentType.Equals(CSEDShippingDocumentDocumentType.SeaInvoice, StringComparison.OrdinalIgnoreCase))
            {
                var blobId = _blobStorage.GenerateCSEDDocumentBlobId(documentId, filePath, fileName, fileType);
                await _invoiceService.ImportCSEDSeaInvoiceAsync(message, blobId);
            }
            // If it is house bl
            else if (documentType.Equals(CSEDShippingDocumentDocumentType.SeaHouseBL, StringComparison.OrdinalIgnoreCase))
            {
                var blobId = _blobStorage.GenerateCSEDDocumentBlobId(documentId, filePath, fileName, fileType);
                await _attachmentService.ImportCSEDSeaHouseBillAsync(message, blobId);
            }
            // If it is sea manifest
            else if (documentType.Equals(CSEDShippingDocumentDocumentType.SeaManifest, StringComparison.OrdinalIgnoreCase))
            {
                var blobId = _blobStorage.GenerateCSEDDocumentBlobId(documentId, filePath, fileName, fileType);
                await _attachmentService.ImportCSEDSeaManifestAsync(message, blobId);
            }
            // If it is attachment
            else if (documentType.Equals(CSEDShippingDocumentDocumentType.Attachment, StringComparison.OrdinalIgnoreCase))
            {
                var blobId = _blobStorage.GenerateCSEDDocumentBlobId(documentId, filePath, fileName, fileType);
                await _attachmentService.ImportCSEDAttachmentAsync(message, blobId);
            }

        }

        /// <summary>
        /// Hangfire job to log exception/error on proceeding message (after received the message)
        /// </summary>
        /// <param name="errorAtGMT"></param>
        /// <param name="messageContent"></param>
        /// <param name="ex"></param>
        [JobDisplayName("Not retry! Error proceeding CSED New Shipping Document at GMT {0}")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void LogErrorOnProceedingEdiSonShippingDocument(DateTime errorAtGMT, string messageContent, Exception ex)
        {
            var errorTitle = $"Error on handling EdiSon new document arrival at GMT {errorAtGMT.ToString("yyyy-MM-dd HH:mm:ss")}" +
                $"\r\n+ Message content: {messageContent}" +
                $"\r\n+ Application exception: {ex}";
            throw new ApplicationException($"NOT retry the job! {errorTitle}");
        }

         /// <summary>
        /// Hangfire job to log exception/error on handling message (as integrating to ASB)
        /// </summary>
        /// <param name="errorAtGMT"></param>
        /// <param name="serviceBusException"></param>
        [JobDisplayName("Not retry! Error handling CSED New Shipping Document at GMT {0}")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void LogErrorOnHandlingEdiSonShippingDocument(DateTime errorAtGMT, ProcessErrorEventArgs serviceBusException)
        {
            var errorTitle = $"Error on handling EdiSon new document arrival at GMT {errorAtGMT.ToString("yyyy-MM-dd HH:mm:ss")}" +
                $"\r\n+ ServiceBus exception: {serviceBusException}";
            throw new ApplicationException($"NOT retry the job! {errorTitle}");
        }

    }
}
