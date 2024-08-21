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

        /// <summary>
        /// This implicit operator allows us to convert a TimerSeriesOptions object to a MongoDB.Driver.TimeSeriesOptions object.
        /// </summary>
        /// <param name="options">The TimerSeriesOptions object to convert.</param>
        /// <returns>A MongoDB.Driver.TimeSeriesOptions object.</returns>
        public static implicit operator MongoDB.Driver.TimeSeriesOptions(TimerSeriesOptions options) =>
            new MongoDB.Driver.TimeSeriesOptions(
                options.TimeField, // The field that stores the time of the data point.
                options.MetaField, // The field that stores metadata about the data point.
                (MongoDB.Driver.TimeSeriesGranularity)options.Granularity);  // The granularity of the time series.
    }
}
