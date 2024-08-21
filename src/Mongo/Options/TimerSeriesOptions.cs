namespace UCode.Mongo.Options
{
    public enum Granularity
    {

        //
        // Summary:
        //     Seconds.
        Seconds,
        //
        // Summary:
        //     Minutes.
        Minutes,
        //
        // Summary:
        //     Hours.
        Hours
    }

    public class TimerSeriesOptions
    {
        /// <summary>
        /// (requerido)
        /// representa o campo que contem o horário de cada time series
        /// </summary>
        public string TimeField
        {
            get; set;
        }

        /// <summary>
        /// (Opcional)
        /// representa o nome do campo que contem os metadados de cada time series
        /// </summary>
        public string MetaField
        {
            get; set;
        }
        /// <summary>
        /// representa a granularidade do time series
        /// </summary>
        public Granularity Granularity
        {
            get; set;
        }
        /// <summary>
        /// /// (Opcional)
        /// representa quanto tempo o dado vai permanecer na base de dados
        /// </summary>
        public long ExpireAfterSeconds
        {
            get; set;
        }

        public static implicit operator MongoDB.Driver.TimeSeriesOptions(TimerSeriesOptions options) => new MongoDB.Driver.TimeSeriesOptions(options.TimeField,
                options.MetaField, (MongoDB.Driver.TimeSeriesGranularity)options.Granularity);
    }
}
