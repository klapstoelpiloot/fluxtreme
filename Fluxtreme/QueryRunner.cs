using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Fluxtreme
{
    public class QueryRunner : IDisposable
    {
        private const string CompilerMessage = "compilation failed: ";
        private static readonly Regex CompileErrorRegex = new Regex("^error @(\\d+):(\\d+)-(\\d+):(\\d+)");

        private InfluxDBClient client;
        private CancellationTokenSource cancelsource;
        private volatile bool running;

        public event Action<List<FluxTable>, List<TableExtraData>, TimeSpan> DataReady;
        public event Action<List<string>> QueryError;

        /// <summary>
        /// Returns True when a query is still running.
        /// </summary>
        public bool IsRunning => running;

        /// <summary>
        /// Start time for the query range variable v.timeRangeStart.
        /// </summary>
        public DateTime TimeRangeStart { get; set; } = DateTime.Today;

        /// <summary>
        /// End time for the query range variable v.timeRangeStop.
        /// </summary>
        public DateTime TimeRangeStop { get; set; } = DateTime.Today + TimeSpan.FromDays(1);

        /// <summary>
        /// When this is specified (not Zero) then TimeRangeStart and TimeRangeStop are not used.
        /// </summary>
        public TimeSpan TimeRangeRecent { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Bucket name for the query variable v.defaultBucket
        /// </summary>
        public string DefaultBucket { get; set; } = string.Empty;

        public QueryRunner()
        {
            client = new InfluxDBClient($"http://{AppSettings.Default.DatabaseAddress}/", AppSettings.Default.DatabaseAccessToken);
            cancelsource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            cancelsource.Cancel();
            lock (client)
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }

        public void Run(string query)
        {
            List<FluxTable> result;
            List<TableExtraData> extradata;
            DateTime starttime, endtime;
            running = true;

            lock(client)
            {
                try
                {
                    // Send the query and await the result
                    File extern_ = SetupExtern();
                    Query q = new Query(extern_, query, Query.TypeEnum.Flux, null, QueryApi.Dialect);
                    QueryApiSync queryapi = client.GetQueryApiSync();
                    starttime = DateTime.Now;
                    result = queryapi.QuerySync(q, AppSettings.Default.DatabaseOrganizationID, cancelsource.Token);
                    endtime = DateTime.Now;
                }
                catch (BadRequestException ex)
                {
                    List<string> errors = new List<string>();
                    foreach (string er in ex.Message.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(er))
                        {
                            string error = er.Trim();

                            // Make a nicer error message
                            if (error.StartsWith(CompilerMessage))
                            {
                                error = error.Substring(CompilerMessage.Length);
                            }

                            // Get the line number for this error message
                            Match coordinates = CompileErrorRegex.Match(error);
                            if ((coordinates.Captures.Count > 0) && (coordinates.Groups.Count > 4))
                            {
                                int lineFrom = int.Parse(coordinates.Groups[1].Value);
                                int colFrom = int.Parse(coordinates.Groups[2].Value);
                                int lineTo = int.Parse(coordinates.Groups[3].Value);
                                int colTo = int.Parse(coordinates.Groups[4].Value);
                                // TODO: Pass these to the event
                            }

                            errors.Add(er.Trim());
                        }
                    }
                    QueryError?.Invoke(errors);
                    running = false;
                    return;
                }

                // Calculate extra data for all tables received
                extradata = result.Select(t => TableExtraData.CalculateFor(t)).ToList();
            }

            // Callback
            DataReady?.Invoke(result, extradata, endtime - starttime);
            running = false;
        }

        private File SetupExtern()
        {
            // This is a nighmare. See https://github.com/influxdata/influxdb/issues/16109
            // This sets up the following Flux statement:
            // option v = { timeRangeStart: -15m, timeRangeStop: now(), defaultBucket: "data" }

            Expression startvalue;
            Expression stopvalue;
            if (TimeRangeRecent != TimeSpan.Zero)
            {
                startvalue = new UnaryExpression("UnaryExpression", "-", new DurationLiteral("DurationLiteral", TimeSpanToDurations(TimeRangeRecent)));
                stopvalue = new CallExpression("CallExpression", new Identifier("Identifier", "now"), new List<Expression>());
            }
            else
            {
                startvalue = new DateTimeLiteral("DateTimeLiteral", TimeRangeStart.ToUniversalTime());
                stopvalue = new DateTimeLiteral("DateTimeLiteral", TimeRangeStop.ToUniversalTime());
            }
            StringLiteral defaultbucket = new StringLiteral("StringLiteral", DefaultBucket);

            Property p_timeRangeStart = new Property("Property", new Identifier("Identifier", "timeRangeStart"), startvalue);
            Property p_timeRangeStop = new Property("Property", new Identifier("Identifier", "timeRangeStop"), stopvalue);
            Property p_defaultBucket = new Property("Property", new Identifier("Identifier", "defaultBucket"), defaultbucket);
            ObjectExpression objex = new ObjectExpression("ObjectExpression", new List<Property>() { p_timeRangeStart, p_timeRangeStop, p_defaultBucket });
            VariableAssignment assign = new VariableAssignment("VariableAssignment", new Identifier("Identifier", "v"), objex);
            OptionStatement option = new OptionStatement("OptionStatement", assign);

            return new File("File", null, null, new List<ImportDeclaration>(), new List<Statement>() { option });
        }
        
        private List<Duration> TimeSpanToDurations(TimeSpan span)
        {
            List<Duration> durations = new List<Duration>();
            if(span.Days > 0) durations.Add(new Duration(null, span.Days, "d"));
            if(span.Hours > 0) durations.Add(new Duration(null, span.Hours, "h"));
            if(span.Minutes > 0) durations.Add(new Duration(null, span.Minutes, "m"));
            if(span.Seconds > 0) durations.Add(new Duration(null, span.Seconds, "s"));
            if(span.Milliseconds > 0) durations.Add(new Duration(null, span.Milliseconds, "ms"));
            return durations;
        }
    }
}
