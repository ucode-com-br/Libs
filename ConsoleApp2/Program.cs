namespace ConsoleApp2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            UCode.Apis.CnpjJa.Client client= new UCode.Apis.CnpjJa.Client("daba46da-4d65-47f5-bf1f-0e49027e3ff8-e8532f51-8d67-434a-b2ac-1b547f2464df", "https://api.cnpja.com");

            var r = new UCode.Apis.CnpjJa.RequestOffice("00076302000143");

            var t = await client.GetOfficeAsync(r);


            System.Console.WriteLine("Hello, World!");
        }
    }
}
