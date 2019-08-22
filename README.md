# Photon Counters plugin for NewRelic

[Read more details here](https://doc.photonengine.com/en-us/server/current/performance/photon-counters#_usage_newrelic).

## How To Use

1. Copy content of this repository to the root directory of your Photon Server SDK. Current tested version is v4-0-29-11263.
2. Using Visual Studio, open "src-server\CounterPublisher.NewRelic\CounterPublisher.NewRelic.sln" solution.
3. Build solution.
4. Enable CounterPublisher.NewRelic plugin and configure it properly:
   The configuration files that need to be changed are (one per server application that needs counters):
 
      - "deploy\CounterPublisher\bin\CounterPublisher.dll.config"
      - "deploy\Loadbalancing\GameServer\bin\Photon.LoadBalancing.dll.config"
      - "deploy\Loadbalancing\Master\bin\Photon.LoadBalancing.dll.config"

   What needs to be changed:

      - Replace `<Photon><CounterPublisher ...><Sender ...` with something like below.
      - Make sure to change `agentId`, `agentName` and `licenseKey` values.
      - `{0}`, if used inside `senderId` or `agentName`, will be replaced with the machine name.
      - Optionally change `version` and `initialDelay`.
      - `sendInterval` needs to be equal or higher than 60.

   Before:

      ```
      <Photon>
      <CounterPublisher enabled="True" updateInterval="1">
        <Sender
          endpoint="udp://255.255.255.255:40001"
          protocol="PhotonBinary"
          initialDelay="10"
          sendInterval="10" />
      </CounterPublisher>
      </Photon>
      ```

   After:

      ```
      <Photon>
       <CounterPublisher enabled="True" senderType="ExitGames.Diagnostics.Configuration.NewRelicAgentSettings, CounterPublisher.NewRelic">
        <Sender
          protocol="ExitGames.Diagnostics.Monitoring.Protocol.NewRelic.Agent.NewRelicWriter, CounterPublisher.NewRelic"

          agentId="CHANGE_AGENT_ID"
          agentName="CHANGE_AGENT_NAME"

          licenseKey="SET_LICENSE_KEY" 

          sendInterval="60" />
      </CounterPublisher>
     </Photon>
     ```

5. Restart Photon Server if needed.

## Enable Debugging

To enable debugging of Photon Counters add the following to a "log4net.config" file to one of the server applications ("CounterPublisher", "MasterServer" or "GameServer"):

```
<logger name="ExitGames.Diagnostics">
  <level value="DEBUG" />    
</logger>
```

Log entries will start to show for the respective application.
