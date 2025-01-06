namespace ConsoleApp2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            UCode.Apis.CnpjJa.Client client= new UCode.Apis.CnpjJa.Client(new HttpClient());

            var t = await client.CnpjAsync("00076302000143", null, null, null, null, null, null, null, null, null, null,
                "daba46da-4d65-47f5-bf1f-0e49027e3ff8-e8532f51-8d67-434a-b2ac-1b547f2464df", default);


            System.Console.WriteLine("Hello, World!");
        }
    }
}
