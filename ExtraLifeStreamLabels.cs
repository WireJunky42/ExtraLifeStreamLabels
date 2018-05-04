using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using WireJunky.ExtraLife.DonorData;
using WireJunky.ExtraLife.ParticipantDataModel;

#pragma warning disable 4014

// ReSharper disable UnusedVariable

namespace WireJunky.ExtraLife.ExtraLifeStreamLabels
{
    public partial class ExtraLifeStreamLabels : ServiceBase
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const string Url = "https://www.extra-life.org/api/";

        private static readonly string ParticipantEndpoint = $"participants/{ConfigurationManager.AppSettings["ParticipantId"]}";

        private readonly string _donationsEndpoint = $"{ParticipantEndpoint}/donations";

        private readonly string _teamEndpoint = $"teams/{ConfigurationManager.AppSettings["TeamId"]}";

        public ExtraLifeStreamLabels()
        {
            InitializeComponent();

            const string source = "Extra Life Stream Labels Service";
            const string log = "Application";

            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, log);

            //EventLog.WriteEntry(source, evt);
        }

        protected override void OnStart(string[] args)
        {
            Debugger.Launch();
            BeginFetchingExtraLifeData();
        }

        protected override void OnStop()
        {
            _cts.Cancel();
        }

        private async Task BeginFetchingExtraLifeData()
        {
            ParticipantData previousParticipantData = new ParticipantData();
            DonorDataModel previousDonorData = new DonorDataModel();
            while (!_cts.IsCancellationRequested)
            {
                //https://www.extra-life.org/api/participants/296936

                using (var clientParticipant = new HttpClient())
                {
                    clientParticipant.BaseAddress = new Uri(Url);
                    HttpResponseMessage responseParticipant = await clientParticipant.GetAsync(ParticipantEndpoint, _cts.Token);
                    if (responseParticipant.IsSuccessStatusCode)
                    {
                        ParticipantData currentParticipantData =
                            ParticipantData.FromJson(responseParticipant.Content.ReadAsStringAsync().Result);

                        int percentage = Convert.ToInt32(currentParticipantData.GetPercentOfGoalReached());

                        if (!previousParticipantData.Equals(currentParticipantData))
                        {
                            previousParticipantData = currentParticipantData;
                            //TODO: Create and/or write out stream lables here. Do this to prevent writing the file if the data hasn't changed.
                        }
                    }

                    Debug.WriteLine($"Participant Data fetched at: {DateTime.Now.ToLongTimeString()}");
                    await Task.Delay(new TimeSpan(0, 0, 0, 15), _cts.Token);
                    
                    using (var clientDonations = new HttpClient())
                    {
                        //DonorDataModel currentDonorData;
                        clientDonations.BaseAddress = new Uri(Url);
                        HttpResponseMessage responseDonations = await clientDonations.GetAsync(_donationsEndpoint, _cts.Token);
                        if (responseDonations.IsSuccessStatusCode)
                        {
                            DonorDataModel[] donorList = DonorDataModel.FromJson(responseDonations.Content.ReadAsStringAsync().Result);
                            if (donorList.Any())
                            {
                                DonorDataModel curreDataModel = donorList[0];
                            }
                        }
                        Debug.WriteLine($"Fetched Donations at {DateTime.Now.ToLongTimeString()}");
                        await Task.Delay(new TimeSpan(0, 0, 0, 15), _cts.Token);
                    }
                }
            }
        }
    }
}
