//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Escala.Services.Observables.Features.Bigdatacorp.Models.Company
//{
//    [DatasetInfo("ondemand_data_receipt", "ReceiptData")]
//    public class ReceiptData
//    {
//        public List<Products> Products { get; set; } = new List<Products>();
//        public ReceiptDataReceiver ReceiverData { get; set; }
//        public ReceiptDataStatus StatusData { get; set; }
//        public ReceiptDataEmission EmissionData { get; set; }
//        public ReceiptDataBasic BasicData { get; set; }
//        public double AccessKey { get; set; }
//        public DateTime CaptureDate { get; set; }
//        public ReceiptDataIssuer IssuerData { get; set; }
//        public string RawResultFile { get; set; }
//        public string RawResultFileType { get; set; }

//        /// <summary>
//        /// Dados de Coleções.
//        /// </summary>
//        public ReceiptDataCollection? CollectionData {  get; set; }

//        /// <summary>
//        /// NFes Referenciados.
//        /// </summary>
//        public List<string> ReferencedNFes { get; set; } = new List<string>();

//        /// <summary>
//        /// Descricao Informacao Adicional.
//        /// </summary>
//        public string ExtraInfoDescription { get; set; }

//        /// <summary>
//        /// Formato Impressao DANFE.
//        /// </summary>
//        public string DanfePrintFormat { get; set; }

//        public ReceiptDataTransportation? TransportationData { get; set; }

//    }

//    public class ReceiptDataTransportation
//    {
//        public string Modality { get; set; }
//        public ReceiptDataConveyor? ConveyorData { get; set; }
//        public List<ReceiptDataVolumes> Volumes { get; set; }
//    }

//    public class ReceiptDataConveyor
//    {
//        public string DocNumber { get; set; }
//        public string DocType { get; set; }
//        public string Name { get; set; }
//        public string StateRegistration {  get; set; }
//        public Address Address { get; set; }

//    }

//    public class ReceiptDataVolumes
//    {
//        public int Amount { get; set; }
//        public string Species { get; set; }
//        public string Brand {  get; set; }
//        public int Number { get; set; }
//    }

//    public class ReceiptDataCollection
//    {
//        public ReceiptDataInvoice InvoiceData { get; set; }
//        public ReceiptDataDuplicate DuplicateData { get; set; }
//        public ReceiptDataPayment PaymentData { get; set; }
//    }

//    public class ReceiptDataPayment
//    {
//        public string PaymentType { get; set; }
//        public decimal PaymentValue { get; set; }
//        public string PaymentIntegrationType { get; set; }
//        public string AccreditingDocNumber { get; set; }
//        public string CarrierBanner { get; set; }
//        public string AuthorizationNumber { get; set; }
//        public int Change { get; set;}
//    }

//    public class ReceiptDataInvoice
//    {
//        public string Number { get; set; }
//        public decimal OriginalValue { get; set; }
//        public decimal DiscountValue { get; set; }
//        public decimal NetValue { get; set; }
//    }

//    public class ReceiptDataDuplicate
//    {
//        public string Number { get; set; }
//        public string DueDate { get; set; }
//        public decimal Value { get; set; }
//    }

//    public class ReceiptDataIssuer
//    {
//        public string OfficialName { get; set; }
//        public string DocNumber { get; set; }
//        public string DocType { get; set; }
//        public Address Address { get; set; }
//        public string StateRegistration { get; set; }

//    }


//    public class BasicData
//    {
//        public string Model { get; set; }
//        public string Serie { get; set; }
//        public string Number { get; set; }
//        public DateTime EmissionDate { get; set; }
//        public DateTime EntranceExitDate { get; set; }
//        public int TotalValue { get; set; }

//    }

//    public class ReceiptDataEmission
//    {
//        public string OperationNature { get; set; }
//        public string OperationType { get; set; }
//        public string DigestValue { get; set; }

//    }

//    public class ReceiptDataStatus
//    {
//        public IList<ReceiptDataEvents> Events { get; set; }

//    }

//    public class ReceiptDataEvents
//    {
//        public string NfeEvents { get; set; }
//        public string Protocol { get; set; }
//        public DateTime AuthorizationDate { get; set; }
//        public DateTime AnInclusionDate { get; set; }

//    }

//    public class ReceiptDataReceiver
//    {
//        public string OfficialName { get; set; }
//        public string DocNumber { get; set; }
//        public string DocType { get; set; }
//        public ReceiptDataAddress Address { get; set; }
//        public string StateRegistration { get; set; }

//    }

//    public class ReceiptDataAddress
//    {
//        public string County { get; set; }
//        public string State { get; set; }
//        public string Country { get; set; }

//    }

//    public class Products
//    {
//        public ReceiptDataBasic BasicData { get; set; }

//    }

//    public class ReceiptDataBasic
//    {
//        public int Number { get; set; }
//        public string Description { get; set; }
//        public int Amount { get; set; }
//        public string Unit { get; set; }
//        public string Value { get; set; }
//        public int DiscountAmount { get; set; }
//        public int TotalFreightValue { get; set; }
//        public int InsurancePrice { get; set; }
//        public int CommercialAmount { get; set; }
//        public int TaxableAmount { get; set; }
//        public string CommercialUnitValue { get; set; }
//        public int TaxUnitValue { get; set; }
//        public int TaxValue { get; set; }

//    }
//}
