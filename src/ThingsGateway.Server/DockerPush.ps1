docker pull mcr.microsoft.com/dotnet/aspnet:8.0-noble

docker build -t registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway:latest .

docker push registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway
