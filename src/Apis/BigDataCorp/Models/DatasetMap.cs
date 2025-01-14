namespace UCode.Apis.BigDataCorp.Models
{
    public abstract class DatasetMap
    {
        /// <summary>
        /// Map dataset informations
        /// </summary>
        /// <param name="datasetName">
        /// BR - nome do dataset, o nome do dataset é usadok na consulta (... "Datasets": "[dataset_name]", ...) e na resposta da consulta um status (... "Status": { "[dataset_name]": [ ...)
        /// </param>
        /// <param name="datasetClassName">
        /// BR - nome da classe do dataset, usado (... "Result": [{ "MatchKeys": "********", "[datasetClassName]": { ...)
        /// </param>
        protected DatasetMap(string datasetName, string datasetClassName)
        {
            DatasetInfo = new DatasetInfo(datasetName, datasetClassName);
        }

        protected readonly DatasetInfo DatasetInfo;

        public static implicit operator DatasetInfo(DatasetMap source) => source.DatasetInfo;
    }
}
