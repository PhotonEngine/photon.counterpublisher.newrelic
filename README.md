# Photon Counters plugin for NewRelic

[Read more details here](https://doc.photonengine.com/en-us/server/current/performance/photon-counters#_usage_newrelic).

## How To Use

1. Copy content of this repository to the root directory of your Photon Server SDK. Current tested version is v4-0-29-11263.
2. Using Visual Studio, open "src-server\CounterPublisher.NewRelic\CounterPublisher.NewRelic.sln" solution.
3. Update "src-server\CounterPublisher.NewRelic\App.config".
4. Build solution.
4. Restart Photon Server.

## Enable Debugging

To enable debugging of Photon Counters add the following to a "log4net.config" file to one of the server applications ("CounterPublisher", "MasterServer" or "GameServer"):

```
<logger name="ExitGames.Diagnostics">
  <level value="DEBUG" />    
</logger>
```

Log entries will start to show for the respective application.
