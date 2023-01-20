dotnet publish Patcher -c Release --self-contained -r "win10-x64" -p:PublishSingleFile=true -o "./build"
xcopy dist build /e /h /k