# FTP Deployer

---

Simple dotnet tool to help deploying an application using FTP. 
Deployment settings are stored in a `YAML` file and are passed to
the tool as an execution argument.

### Usage
```
dotnet tool run ftp-deployer ./scripts/deploy.yaml
```

### Settings file
```
FtpHost: ftp.my-server.org
FtpPort: 21
Username: auser
Password: apassword
EmptyRemoteDirectory: true
UploadAppOffline: true
LocalDirectory: "path to published application"
Include: []
Exclude:
  - ".DS_Store"
  - "**/appsettings.Production.yaml"
RemoteDirectory: "/remote folder/"
```
#### Notes
- `Include` and `Exclude` properties support multiple glob patterns.
- `UploadAppOffline` helps deploying [IIS applications](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/app-offline)



