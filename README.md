Extra Life Stream Labels is a Windows service that utilizes the Donor Drive public API to fetch Extra Life participant, team, and donation information.

Add your Participant Id, Team Id, and desired location for stream labels to be saved to the app.config file.

  <appSettings>
    <add key="ParticipantId" value=""/>
    <add key="TeamId" value=""/>
    <add key="StreamLabelOutputPath" value="D:\\ExtraLifeStreamLabelData\\"/>
  </appSettings>
