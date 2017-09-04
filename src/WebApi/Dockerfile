 # Stage 1
 FROM microsoft/aspnetcore-build:2.0 AS builder
 ARG VERSION_SUFFIX="1.0.0"
 ARG CONFIGURATION="Release"
 WORKDIR /source

 # caches restore result by copying csproj file separately
 COPY *.csproj .
 RUN dotnet restore

 # copies the rest of your code
 COPY . .
 RUN dotnet publish --output /app/ --configuration $CONFIGURATION --version-suffix $VERSION_SUFFIX

 # Stage 2
 FROM microsoft/aspnetcore:2.0
 WORKDIR /app
 COPY --from=builder /app .
 ENTRYPOINT ["dotnet", "WebApi.dll"]