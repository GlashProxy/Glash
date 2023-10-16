FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App
COPY . ./
RUN sh build.sh

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime
RUN echo 'Asia/Shanghai' >/etc/timezone
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "GlashAgent.dll"]
