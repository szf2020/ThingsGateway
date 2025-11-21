#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#构建：docker build -t registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway:latest .
#拉取：docker pull registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway
#推送：docker push registry.cn-shenzhen.aliyuncs.com/thingsgateway/thingsgateway

#aspnetcore10.0环境
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-amd64 AS base
COPY .  /app
WORKDIR /app
#默认web
EXPOSE 5000

# 添加时区环境变量，亚洲，上海
ENV TimeZone=Asia/Shanghai
ENV TZ=Asia/Shanghai
# 转发头
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "ThingsGateway.Server.dll","--urls","http://*:5000"]


 