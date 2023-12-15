using CodeImp.Fluxtreme.Configuration;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CodeImp.Fluxtreme.Data
{
    public class QueryRunner : IDisposable
    {
        private const string CompilerMessage = "compilation failed: ";
        private static readonly Regex CompileErrorRegex = new Regex("@(\\d+):(\\d+)-(\\d+):(\\d+)");

        private DatasourceSettings datasource;
        private InfluxDBClient client;
        private CancellationTokenSource cancelsource = new CancellationTokenSource();
        private volatile bool running;
        private object syncObject = new object();

        public event Action<List<FluxTable>, TimeSpan> DataReady;
        public event Action<List<QueryError>> QueryError;

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
        public TimeSpan TimeRangeRecent { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Window period for the query variable v.windowPeriod.
        /// </summary>
        public TimeSpan WindowPeriod { get; set; } = TimeSpan.FromSeconds(1);

        public QueryRunner()
        {
        }

        public void Dispose()
        {
            cancelsource.Cancel();
            lock (syncObject)
            {
                client?.Dispose();
                client = null;
            }
        }

        public void SetDatasource(DatasourceSettings settings)
        {
            cancelsource.Cancel();
            lock (syncObject)
            {
                client?.Dispose();
                client = null;
                datasource = settings;
            }
        }

        public void Run(string query)
        {
            List<FluxTable> result;
            DateTime starttime, endtime;
            running = true;
            cancelsource = new CancellationTokenSource();

            lock (syncObject)
            {
                try
                {
                    if (client == null)
                    {
                        if (datasource != null)
                        {
                            client = new InfluxDBClient($"http://{datasource.Address}/", datasource.AccessToken);
                        }
                        else
                        {
                            QueryError?.Invoke(new List<QueryError>() { new QueryError("No data source selected.") });
                            running = false;
                            return;
                        }
                    }

                    // Send the query and await the result
                    File extern_ = SetupExtern();
                    Query q = new Query(extern_, query, Query.TypeEnum.Flux, null, QueryApi.Dialect);
                    QueryApiSync queryapi = client.GetQueryApiSync();
                    starttime = DateTime.Now;
                    result = queryapi.QuerySync(q, datasource.OrganizationID, cancelsource.Token);
                    endtime = DateTime.Now;
                }
                catch (FluxQueryException ex)
                {
                    QueryError?.Invoke(new List<QueryError>() { new QueryError(ex.Message) });
                    running = false;
                    return;
                }
                catch (BadRequestException ex)
                {
                    List<QueryError> errors = new List<QueryError>();
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
                            QueryError e = new QueryError(error);
                            Match coordinates = CompileErrorRegex.Match(error);
                            if ((coordinates.Captures.Count > 0) && (coordinates.Groups.Count > 4))
                            {
                                e.StartLine = int.Parse(coordinates.Groups[1].Value) - 1;
                                e.StartColumn = int.Parse(coordinates.Groups[2].Value);
                                e.EndLine = int.Parse(coordinates.Groups[3].Value) - 1;
                                e.EndColumn = int.Parse(coordinates.Groups[4].Value);
                            }

                            errors.Add(e);
                        }
                    }
                    QueryError?.Invoke(errors);
                    running = false;
                    return;
                }
                catch (Exception ex)
                {
                    Exception basex = ex.GetBaseException();
                    QueryError?.Invoke(new List<QueryError>() { new QueryError(basex.Message) });
                    running = false;
                    return;
                }
            }

            // Callback
            DataReady?.Invoke(result, endtime - starttime);
            running = false;
        }

        private File SetupExtern()
        {
            // This is a nighmare. See https://github.com/influxdata/influxdb/issues/16109
            // This sets up the following Flux statement:
            // option v = { timeRangeStart: -1h, timeRangeStop: now(), windowPeriod: 1s, defaultBucket: "data" }
            // They have all these classes to represent the tokens, why don't they have something like Query.ParseToTokens(...)?

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
            StringLiteral defaultbucket = new StringLiteral("StringLiteral", datasource.DefaultBucket);
            DurationLiteral windowperiod = new DurationLiteral("DurationLiteral", TimeSpanToDurations(WindowPeriod));

            Property p_timeRangeStart = new Property("Property", new Identifier("Identifier", "timeRangeStart"), startvalue);
            Property p_timeRangeStop = new Property("Property", new Identifier("Identifier", "timeRangeStop"), stopvalue);
            Property p_windowPeriod = new Property("Property", new Identifier("Identifier", "windowPeriod"), windowperiod);
            Property p_defaultBucket = new Property("Property", new Identifier("Identifier", "defaultBucket"), defaultbucket);
            ObjectExpression objex = new ObjectExpression("ObjectExpression", new List<Property>() { p_timeRangeStart, p_timeRangeStop, p_windowPeriod, p_defaultBucket });
            VariableAssignment assign = new VariableAssignment("VariableAssignment", new Identifier("Identifier", "v"), objex);
            OptionStatement option = new OptionStatement("OptionStatement", assign);

            return new File("File", null, null, new List<ImportDeclaration>(), new List<Statement>() { option });
        }

        private List<Duration> TimeSpanToDurations(TimeSpan span)
        {
            List<Duration> durations = new List<Duration>();
            if (span.Days > 0) durations.Add(new Duration(null, span.Days, "d"));
            if (span.Hours > 0) durations.Add(new Duration(null, span.Hours, "h"));
            if (span.Minutes > 0) durations.Add(new Duration(null, span.Minutes, "m"));
            if (span.Seconds > 0) durations.Add(new Duration(null, span.Seconds, "s"));
            if (span.Milliseconds > 0) durations.Add(new Duration(null, span.Milliseconds, "ms"));
            return durations;
        }
    }
}
