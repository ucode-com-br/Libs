using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UCode.Apis.BigDataCorp.Models.Company
{
    //https://docs.bigdatacorp.com.br/plataforma/reference/empresas_registration_data

    /// <summary>
    /// References in https://docs.bigdatacorp.com.br/plataforma/reference/empresas_registration_data
    /// </summary>
    [DatasetInfo("registration_data", "RegistrationData")]
    public class RegistrationData
    {
        public BasicData BasicData { get; set; } = new BasicData();

        public DefinedStrictList<Email> Emails { get; set; } = new DefinedStrictList<Email>();

        public DefinedStrictList<Address> Addresses { get; set; } = new DefinedStrictList<Address>();

        public DefinedStrictList<Phone> Phones { get; set; } = new DefinedStrictList<Phone>();
    }
}
