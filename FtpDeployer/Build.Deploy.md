Build:
```
dotnet pack --configuration Release 
```

Deploy
```
dotnet nuget push ./nupkg/FtpDeployer.1.0.0.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
```
