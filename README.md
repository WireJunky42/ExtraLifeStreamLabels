Extra Life Stream Labels is a Windows service that utilizes the Donor Drive public API to fetch Extra Life participant, team, and donation information.

Add your Participant Id, Team Id, and desired location for stream labels to be saved to the app.config file.

```
<appSettings>
    <add key="ParticipantId" value=""/>
    <add key="TeamId" value=""/>
    <add key="StreamLabelOutputPath" value="D:\\ExtraLifeStreamLabelData\\"/>
  </appSettings>

```

To install the service, open the command prompt as admin. Navigate to your extracted directory and enter ExtraLifeStreamLabelsService.exe i

To uninstall the service enter ExtraLifeStreamLabelsService.exe u

The service is set to automatic and will start when Windows starts, however if you wish to start it immediately you may either use the Windows Services Manager to start it or use the following command at the command line:

sc Start ExtraLifeStreamLabelService

Like my work?  Consider supporting my Extra Life efforts by visiting http://wirejunky.net and making a donation.
