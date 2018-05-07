using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WireJunky.ExtraLife.DonorData;
using WireJunky.ExtraLife.ParticipantDataModel;

#pragma warning disable 4014

// ReSharper disable UnusedVariable

// ReSharper disable once CheckNamespace
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
        }

        protected override void OnStart(string[] args)
        {
            BeginFetchingExtraLifeData();
        }

        protected override void OnStop()
        {
            _cts.Cancel();
        }

        private async Task BeginFetchingExtraLifeData()
        {
            if (!Directory.Exists($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}"))
                Directory.CreateDirectory($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}");

            while (!_cts.IsCancellationRequested)
            {
                using (var clientParticipant = new HttpClient())
                {
                    clientParticipant.BaseAddress = new Uri(Url);
                    HttpResponseMessage responseParticipant = await clientParticipant.GetAsync(ParticipantEndpoint, _cts.Token);
                    if (responseParticipant.IsSuccessStatusCode)
                    {
                        using (FileStream progressDataStream = new FileStream($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeProgress.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            ParticipantData currentParticipantData =
                                ParticipantData.FromJson(responseParticipant.Content.ReadAsStringAsync().Result);

                            int percentage = Convert.ToInt32(currentParticipantData.GetPercentOfGoalReached());

                            string progress = $"{currentParticipantData.SumDonations} / {currentParticipantData.FundraisingGoal} ( {percentage}% )";
                            progressDataStream.Write(new UTF8Encoding(true).GetBytes(progress), 0, progress.Length);
                        }
                    }
                }
                await Task.Delay(new TimeSpan(0, 0, 0, 15), _cts.Token);

                using (HttpClient clientDonations = new HttpClient())
                {
                    clientDonations.BaseAddress = new Uri(Url);
                    HttpResponseMessage responseDonations = await clientDonations.GetAsync(_donationsEndpoint, _cts.Token);
                    if (responseDonations.IsSuccessStatusCode)
                    {
                        using (FileStream lastDonorData = new FileStream($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeMostRecentDonation.txt",
                            FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            DonorDataModel[] donorList = DonorDataModel.FromJson(responseDonations.Content.ReadAsStringAsync().Result);
                            if (donorList.Any())
                            {
                                DonorDataModel mostRecentDataModel = donorList[0];
                                string donorName = mostRecentDataModel.DisplayName ?? "Anonymous";
                                string donation = $"{donorName}: ${mostRecentDataModel.Amount}";
                                lastDonorData.Write(new UTF8Encoding(true).GetBytes(donation), 0, donation.Length);
                            }
                        }
                    }
                    await Task.Delay(new TimeSpan(0, 0, 0, 15), _cts.Token);
                }
            }
        }
    }
}
