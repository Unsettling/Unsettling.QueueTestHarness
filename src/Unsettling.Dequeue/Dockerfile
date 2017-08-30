FROM microsoft/dotnet:1-runtime
ENV NUGET_XMLDOC_MODE skip
WORKDIR /app
COPY ./bin/release/netcoreapp1.1/publish /app
ENTRYPOINT ["dotnet", "Unsettling.Dequeue.dll"]