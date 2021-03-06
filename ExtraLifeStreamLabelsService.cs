﻿using System;
using System.Collections.Generic;
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
using WireJunky.ExtraLife.Properties;
using WireJunky.ServiceFramework;

#pragma warning disable 4014

// ReSharper disable UnusedVariable

// ReSharper disable once CheckNamespace
namespace WireJunky.ExtraLife
{
    public partial class ExtraLifeStreamLabelsService : IService
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private const string Url = "https://www.extra-life.org/api/";
        private static readonly string ParticipantEndpoint = $"participants/{ConfigurationManager.AppSettings["ParticipantId"]}";
        private readonly string _donationsEndpoint = $"{ParticipantEndpoint}/donations";
        private readonly string _teamEndpoint = $"teams/{ConfigurationManager.AppSettings["TeamId"]}";
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static ParticipantData _previousParticipantData = new ParticipantData();


        public ExtraLifeStreamLabelsService()
        {
            InitializeComponent();
        }

        private async Task FetchExtraLifeData()
        {
            if (!Directory.Exists($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}"))
                Directory.CreateDirectory($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}");
            {
                while (!_cts.IsCancellationRequested)
                {
                    await GetParticipantData();
                    await Task.Delay(new TimeSpan(0, 0, 0, 15), _cts.Token);
                }
            }
        }

        private async Task GetParticipantData()
        {
            using (var clientParticipant = new HttpClient())
            {
                try
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
                catch (Exception e)
                {
                    Logger.Error(e);
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
                try
                {
                    clientDonations.BaseAddress = new Uri(Url);
                    HttpResponseMessage responseDonations =
                        await clientDonations.GetAsync(_donationsEndpoint, _cts.Token);
                    if (responseDonations.IsSuccessStatusCode)
                    {
                        CreateDonorInfoStreamLabels(responseDonations);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private static void CreateDonorInfoStreamLabels(HttpResponseMessage responseDonations)
        {
            DonorDataModel[] donorList =
                DonorDataModel.FromJson(responseDonations.Content.ReadAsStringAsync().Result);

            using (FileStream lastDonorData = new FileStream($"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeMostRecentDonation.txt",
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                if (donorList.Any())
                {
                    DonorDataModel mostRecentDataModel = donorList[0];
                    string donation = $"{GetDonorName(mostRecentDataModel)}{GetDonationAmount(mostRecentDataModel)}    ";  //$"{donorName}{donationAmount}";
                    lastDonorData.SetLength(0);
                    lastDonorData.Write(new UTF8Encoding(true).GetBytes(donation), 0, donation.Length);
                    Console.WriteLine(donation);
                }
            }

            using (FileStream fullDonorListData = new FileStream(
                $"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeFullDonorList.txt",
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StringBuilder sb = new StringBuilder();
                string donation = string.Empty;
                fullDonorListData.SetLength(0);
                List<DonorDataModel> fullDonorList = donorList.ToList();
                foreach (var donor in fullDonorList)
                {
                    sb.Append($"{GetDonorName(donor)}{GetDonationAmount(donor)}    ");
                }

                string fullDonorData = sb.ToString();
                fullDonorListData.Write(new UTF8Encoding(true).GetBytes(fullDonorData), 0, fullDonorData.Length);
            }

            using (FileStream fullDonorListWithMessageData = new FileStream(
                $"{ConfigurationManager.AppSettings["StreamLabelOutputPath"]}//ExtraLifeFullDonorListWithMessages.txt",
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                StringBuilder sb = new StringBuilder();
                string donation = string.Empty;
                fullDonorListWithMessageData.SetLength(0);
                List<DonorDataModel> fullDonorList = donorList.ToList();
                foreach (var donor in fullDonorList)
                {
                    sb.Append($"{GetDonorName(donor)}{GetDonationAmount(donor)}{GetDonationMessage(donor)}    ");
                }

                string fullDonorData = sb.ToString();
                fullDonorListWithMessageData.Write(new UTF8Encoding(true).GetBytes(fullDonorData), 0, fullDonorData.Length);
            }
        }

        private static string GetDonorName(DonorDataModel donorDataModel)
        {
            return donorDataModel.DisplayName ?? Resources.AnonymousDonorName;
        }

        private static string GetDonationAmount(DonorDataModel donorDataModel)
        {
            return donorDataModel.Amount == null ? string.Empty : $": ${donorDataModel.Amount:N2}";
        }

        private static string GetDonationMessage(DonorDataModel donorDataModel)
        {
            return donorDataModel.Message == null ? string.Empty : $" Message: {donorDataModel.Message}";
        }

        public void Dispose()
        {
            _cts.Cancel(false);
        }

        public void Start()
        {
            FetchExtraLifeData();
        }

        public bool HandlePowerEvent(PowerBroadcastStatus powerBroadcastStatus)
        {
            switch (powerBroadcastStatus)
            {

                case PowerBroadcastStatus.Suspend:
                    _cts.Cancel(false);
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                    _previousParticipantData = new ParticipantData();
                    _cts = new CancellationTokenSource();
                    FetchExtraLifeData();
                    break;
                case PowerBroadcastStatus.BatteryLow:
                case PowerBroadcastStatus.OemEvent:
                case PowerBroadcastStatus.PowerStatusChange:
                case PowerBroadcastStatus.QuerySuspend:
                case PowerBroadcastStatus.QuerySuspendFailed:
                case PowerBroadcastStatus.ResumeAutomatic:
                case PowerBroadcastStatus.ResumeCritical:
                    break;
            }
            return true;
        }
    }
}
