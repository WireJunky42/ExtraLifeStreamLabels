using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WireJunky.ExtraLife.DonorData;
using WireJunky.ExtraLife.ParticipantDataModel;
using WireJunky.ServiceFramework;

#pragma warning disable 4014

// ReSharper disable UnusedVariable

// ReSharper disable once CheckNamespace
namespace WireJunky.ExtraLife
{
    public partial class ExtraLifeStreamLabelsService : IService
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private const string Url = "https://www.extra-life.org/api/";
        private static readonly string ParticipantEndpoint = $"participants/{ConfigurationManager.AppSettings["ParticipantId"]}";
        private readonly string _donationsEndpoint = $"{ParticipantEndpoint}/donations";
        private readonly string _teamEndpoint = $"teams/{ConfigurationManager.AppSettings["TeamId"]}";
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static Task _fetchData;
        private static ParticipantData _previousParticipantData = new ParticipantData();


        public ExtraLifeStreamLabelsService()
        {
            InitializeComponent();
        }

        private async Task FetchExtraLifeData()
        {
            Logger.Info($"Started Fetch Extra Life Data");
            if (!Directory.Exists($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}"))
                Directory.CreateDirectory($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}");

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    await GetParticipantData();
                    await Task.Delay(new TimeSpan(0, 0, 0, 15), _cts.Token);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        private async Task GetParticipantData()
        {
            using (var clientParticipant = new HttpClient())
            {
                clientParticipant.BaseAddress = new Uri(Url);
                HttpResponseMessage responseParticipant = await clientParticipant.GetAsync(ParticipantEndpoint, _cts.Token);
                if (responseParticipant.IsSuccessStatusCode)
                {
                    using (ParticipantData currentParticipantData =
                        ParticipantData.FromJson(responseParticipant.Content.ReadAsStringAsync().Result))
                    {
                        if (!currentParticipantData.Equals(_previousParticipantData))
                        {
                            _previousParticipantData = currentParticipantData;
                            CreateParticipantStreamLabel(currentParticipantData);

                            await GetDonationData();
                        }
                    }
                }
            }
        }

        private static void CreateParticipantStreamLabel(ParticipantData currentParticipantData)
        {
            using (FileStream progressDataStream = new FileStream($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeProgress.txt",
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                int percentage =
                    Convert.ToInt32(currentParticipantData.GetPercentOfGoalReached());
                string progress =
                    $"${currentParticipantData.SumDonations:N2} / ${currentParticipantData.FundraisingGoal:N2} ( {percentage}% )";
                progressDataStream.SetLength(0);
                progressDataStream.Write(new UTF8Encoding(true).GetBytes(progress), 0,
                    progress.Length);
                Console.WriteLine($"{progress}");
            }
        }

        private async Task GetDonationData()
        {
            using (HttpClient clientDonations = new HttpClient())
            {
                clientDonations.BaseAddress = new Uri(Url);
                HttpResponseMessage responseDonations =
                    await clientDonations.GetAsync(_donationsEndpoint, _cts.Token);
                if (responseDonations.IsSuccessStatusCode)
                {
                    CreateMostRecentDonorStreamLabel(responseDonations);
                }
            }
        }

        private static void CreateMostRecentDonorStreamLabel(HttpResponseMessage responseDonations)
        {
            using (FileStream lastDonorData = new FileStream($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeMostRecentDonation.txt",
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                DonorDataModel[] donorList =
                    DonorDataModel.FromJson(responseDonations.Content.ReadAsStringAsync().Result);
                if (donorList.Any())
                {
                    DonorDataModel mostRecentDataModel = donorList[0];
                    string donorName = mostRecentDataModel.DisplayName ?? "Anonymous";
                    string donation = $"{donorName}: ${mostRecentDataModel.Amount:N2}";
                    lastDonorData.SetLength(0);
                    lastDonorData.Write(new UTF8Encoding(true).GetBytes(donation), 0, donation.Length);
                    Console.WriteLine(donation);
                }
            }
        }

        public void Dispose()
        {
            Logger.Info("Service dispose...");
            _cts.Cancel();
        }

        public void Start()
        {
            Logger.Info("Service started...");
            FetchExtraLifeData();
        }

        public bool HandlePowerEvent(PowerBroadcastStatus powerBroadcastStatus)
        {
            Logger.Info($"Received Power Broadcast Status: {powerBroadcastStatus}");
            switch (powerBroadcastStatus)
            {

                case PowerBroadcastStatus.QuerySuspend:
                case PowerBroadcastStatus.Suspend:
                    FetchExtraLifeData().Dispose();
                    break;
                case PowerBroadcastStatus.QuerySuspendFailed:
                case PowerBroadcastStatus.ResumeAutomatic:
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeSuspend:
                    FetchExtraLifeData();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(powerBroadcastStatus), powerBroadcastStatus, null);
            }
            return true;
        }
    }
}
