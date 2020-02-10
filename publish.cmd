nuget restore
msbuild CoreBot.sln -p:DeployOnBuild=true -p:PublishProfile=makoto-bot-Web-Deploy.pubxml -p:Password=SdqmzbmZuRnsNJvaZSdcAxHQomMqaKcWBTmsjpEvDxWAn3EfHNKwPbQ4stC8

