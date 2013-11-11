# NLog.RollbarSharp

An [NLog](http://nlog-project.org/) target using the [RollbarSharp](https://github.com/mroach/rollbarsharp) library for sending notifications to [Rollbar](http://www.rollbar.com/).

## Setup

You'll need to edit your configuration file that holds your NLog settings. For ASP.NET applications this will likely be `web.config`. Three additions need to be made to `extensions`, `targets`, and `rules`.

```xml
<nlog>
    <extensions>
        <add addembly="NLog.RollbarSharp" />
    </extensions>
    <targets>
        <target xsi:type="RollbarSharp" name="rollbar" />
    </targets>
    <rules>
        <logger name="*" minlevel="Warn" writeTo="rollbar" />
    </rules>
</nlog>
```

## Config

You'll need to set your Rollbar access token and environment. This can be done inline with the NLog configuration or in your `appSettings`.

You *have* to set the access token. You can optionally set:

* Endpoint (to override the Rollbar API URL)
* Environment (e.g. development, production)
* Platform
* Language
* Framework
* Title

### App settings

If you set the configuration here then you'll be able to use the same configuration for the standalone [RollbarSharp](https://github.com/mroach/rollbarsharp) client if you choose to do so.

```xml
<appSettings>
  <add key="Rollbar.AccessToken" value="6703358e9f54081e59bb0d65ee066363"/>
  <add key="Rollbar.Environment" value="development"/>
</appSettings>
```

### Inline

You can configure NLog.RollbarSharp inline like so.

```xml
<targets>
  <target xsi:type="RollbarSharp" name="rollbar" accessToken="6703358e9f54081e59bb0d65ee066363" environment="development" />
</target>
```
