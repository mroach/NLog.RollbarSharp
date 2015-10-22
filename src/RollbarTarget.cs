﻿using System;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;
using RollbarSharp;

namespace NLog.RollbarSharp
{
    [Target("RollbarSharp")]
    public class RollbarTarget : TargetWithLayout
    {
        public string AccessToken { get; set; }

        public string Endpoint { get; set; }

        public Layout Environment { get; set; }

        public Layout Platform { get; set; }

        public Layout Language { get; set; }

        public Layout Framework { get; set; }

        public Layout Title { get; set; }

        public RollbarTarget()
        {
            Title = "${message}";
        }

        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var client = CreateClient(logEvent);
                var level = ConvertLogLevel(logEvent.Level);
                var title = Title.Render(logEvent);

                var notice = logEvent.Exception != null
                                 ? client.NoticeBuilder.CreateExceptionNotice(logEvent.Exception)
                                 : client.NoticeBuilder.CreateMessageNotice(logEvent.FormattedMessage);

                notice.Level = level;
                notice.Title = title;

                client.Send(notice);
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception.ToString());
            }
        }

        /// <summary>
        /// First attemps to create a RollbarClient using config read from
        /// appSettings. Uses properties of this class as overrides if specified.
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private RollbarClient CreateClient(LogEventInfo logEvent)
        {
            var configuration = new Configuration(AccessToken);

            if (!string.IsNullOrEmpty(Endpoint))
                configuration.Endpoint = Endpoint;

            if (Environment != null)
                configuration.Environment = Environment.Render(logEvent);

            if (Platform != null)
                configuration.Platform = Platform.Render(logEvent);

            if (Language != null)
                configuration.Language = Language.Render(logEvent);

            if (Framework != null)
                configuration.Framework = Framework.Render(logEvent);

            var client = new RollbarClient(configuration);
            client.RequestStarting += RollbarClientRequestStarting;
            client.RequestCompleted += RollbarClientRequestCompleted;

            return client;
        }

        /// <summary>
        /// When posting the request to Rollbar, log it to the internal NLog log
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private static void RollbarClientRequestStarting(object source, RequestStartingEventArgs args)
        {
            var client = (RollbarClient)source;
            InternalLogger.Debug("Sending request to {0}: {1}", client.Configuration.Endpoint, args.Payload);
        }

        /// <summary>
        /// When receiving a response from Rollbar, log it to the internal NLog log
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private static void RollbarClientRequestCompleted(object source, RequestCompletedEventArgs args)
        {
            if (args.Result.IsSuccess)
            {
                InternalLogger.Debug("Request was successful: " + args.Result.Message);
                return;
            }

            InternalLogger.Warn("Request failed: " + args.Result);
        }

        /// <summary>
        /// Convert the NLog level to a level string understood by Rollbar
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static string ConvertLogLevel(LogLevel level)
        {
            if (level == LogLevel.Fatal) return "critical";
            if (level == LogLevel.Error) return "error";
            if (level == LogLevel.Warn) return "warning";
            if (level == LogLevel.Info) return "info";
            if (level == LogLevel.Debug) return "debug";
            return "debug";
        }
    }
}
